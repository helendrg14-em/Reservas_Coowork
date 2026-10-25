using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.Data.SqlClient;
using Workhub.Data;
using Workhub.Models;
using Workhub.Models.Enums;
using Workhub.Services;
using Workhub.ViewModels;
namespace Workhub.Controllers;

public class PagosController(ApplicationContext db, PagoService service, ArchivoService archivos,
    IWebHostEnvironment environment, IConfiguration configuration) : Controller
{
    private int UsuarioId => int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
    private bool Admin => User.IsInRole("Administrador");
    private IQueryable<Pago> Visibles => db.Pagos.Where(p => Admin || p.Reserva.IdUsuario == UsuarioId);
    public async Task<IActionResult> Index(string? buscar, int pagina = 1, int paginaPendientes = 1, int paginaVencidas = 1)
    {
        var q = Visibles.AsNoTracking().Include(p => p.Reserva).ThenInclude(r => r.Usuario).AsQueryable();
        var pendientes = db.Reservas.AsNoTracking().Include(r => r.Espacio).Include(r => r.Usuario)
            .Where(r => (Admin || r.IdUsuario == UsuarioId) && r.Estado == EstadoReserva.Pendiente && r.Pago == null);
        if (!string.IsNullOrWhiteSpace(buscar))
        {
            q = q.Where(p => p.CodigoTransaccion.Contains(buscar) || p.Reserva.Folio.Contains(buscar) || p.Reserva.Espacio.Nombre.Contains(buscar));
            pendientes = pendientes.Where(r => r.Folio.Contains(buscar) || r.Espacio.Nombre.Contains(buscar));
        }
        var ahora = RelojBolivia.Ahora;
        var vigentes = pendientes.Where(r => r.FechaInicio > ahora);
        var vencidas = pendientes.Where(r => r.FechaInicio <= ahora);
        var vm = new PagosIndexVm
        {
            Buscar = buscar, PagoClienteHabilitado = Presentacion.PagoDemo(environment, configuration),
            MontoPendiente = await vigentes.SumAsync(r => r.CostoTotal),
            PaginacionPendientes = Paginacion.Crear(paginaPendientes, await vigentes.CountAsync(), 5, "paginaPendientes", "pendientes"),
            PaginacionVencidas = Paginacion.Crear(paginaVencidas, await vencidas.CountAsync(), 5, "paginaVencidas", "vencidas"),
            PaginacionHistorial = Paginacion.Crear(pagina, await q.CountAsync(), 10, "pagina", "historial")
        };
        vm.PorPagar = await vigentes.OrderBy(r => r.FechaInicio).ThenBy(r => r.IdReserva)
            .Skip(vm.PaginacionPendientes.Saltar).Take(vm.PaginacionPendientes.TamanoPagina).ToListAsync();
        vm.Vencidas = await vencidas.OrderByDescending(r => r.FechaInicio).ThenBy(r => r.IdReserva)
            .Skip(vm.PaginacionVencidas.Saltar).Take(vm.PaginacionVencidas.TamanoPagina).ToListAsync();
        vm.Historial = await q.OrderByDescending(p => p.FechaPago).ThenByDescending(p => p.IdPago)
            .Skip(vm.PaginacionHistorial.Saltar).Take(vm.PaginacionHistorial.TamanoPagina).ToListAsync();
        return View(vm);
    }
    public async Task<IActionResult> Pagar(int id)
    {
        var reserva = await ReservaPropia(id);
        if (reserva is null) return NotFound();
        if (reserva.Pago is not null) return RedirectToAction(nameof(Detalles), new { id = reserva.Pago.IdPago });
        return View(new PagoClienteForm { Reserva = reserva,
            SimulacionHabilitada = Presentacion.PagoDemo(environment, configuration) });
    }
    [HttpPost]
    [RequestSizeLimit(ArchivoService.TamanoMaximo + 1024 * 1024)]
    public async Task<IActionResult> Pagar(int id, PagoClienteForm form)
    {
        if (!Presentacion.PagoDemo(environment, configuration)) return Forbid();
        var reserva = await ReservaPropia(id);
        if (reserva is null) return NotFound();
        form.Reserva = reserva;
        form.SimulacionHabilitada = true;
        if (!Presentacion.PuedePagar(reserva))
            ModelState.AddModelError("", "Esta reserva ya no admite pago: está vencida, cerrada o tiene un comprobante.");
        if (ModelState.IsValid)
        {
            string? archivo = null;
            try
            {
                if (form.Archivo is { Length: > 0 }) archivo = await archivos.GuardarComprobanteAsync(form.Archivo);
                var pagoId = await service.RegistrarAsync(new PagoForm
                {
                    IdReserva = id, MetodoPago = form.MetodoPago,
                    CodigoTransaccion = string.IsNullOrWhiteSpace(form.CodigoTransaccion)
                        ? "SIM-" + Guid.NewGuid().ToString("N").ToUpperInvariant()
                        : form.CodigoTransaccion
                }, UsuarioId, archivo);
                archivo = null;
                TempData["Exito"] = "Registro completado. Tu reserva está confirmada.";
                return RedirectToAction(nameof(Detalles), new { id = pagoId });
            }
            catch (OperacionException e) { ModelState.AddModelError("", e.Message); }
            catch (Exception e) when (ErroresBaseDatos.Es(e))
            { archivo = null; ModelState.AddModelError("", "No se pudo confirmar. Revisa tus pagos antes de volver a intentar."); }
            finally { archivos.EliminarComprobante(archivo); } // Solo queda en null si el pago se guardó.
        }
        return View(form);
    }
    private Task<Reserva?> ReservaPropia(int id) => db.Reservas.AsNoTracking()
        .Include(r => r.Espacio).Include(r => r.Pago).Include(r => r.ReservaEquipamientos).ThenInclude(e => e.Equipamiento)
        .SingleOrDefaultAsync(r => r.IdReserva == id && r.IdUsuario == UsuarioId);

    public async Task<IActionResult> Detalles(int id)
    {
        var p = await Visibles.AsNoTracking().Include(p => p.Reserva).ThenInclude(r => r.Usuario)
            .SingleOrDefaultAsync(p => p.IdPago == id);
        return p is null ? NotFound() : View(p);
    }
    [Authorize(Policy = "Administracion")]
    public async Task<IActionResult> Crear(int? reservaId)
    {
        var form = new PagoForm { IdReserva = reservaId ?? 0 }; await Opciones(form); return View("Formulario", form);
    }
    [HttpPost, Authorize(Policy = "Administracion")]
    [RequestSizeLimit(ArchivoService.TamanoMaximo + 1024 * 1024)]
    public async Task<IActionResult> Crear(PagoForm form)
    {
        form.Id = 0;
        if (ModelState.IsValid)
        {
            string? archivo = null;
            try
            {
                if (form.Archivo is { Length: > 0 }) archivo = await archivos.GuardarComprobanteAsync(form.Archivo);
                var id = await service.RegistrarAsync(form, comprobante: archivo);
                archivo = null;
                TempData["Exito"] = "Pago registrado y reserva confirmada.";
                return RedirectToAction(nameof(Detalles), new { id });
            }
            catch (OperacionException e) { ModelState.AddModelError("", e.Message); }
            catch (Exception e) when (ErroresBaseDatos.Es(e))
            { archivo = null; ModelState.AddModelError("", "No se guardó el pago. Compruebe si ya existe y vuelva a intentar."); }
            finally { archivos.EliminarComprobante(archivo); } // Solo queda en null si el pago se guardó.
        }
        await Opciones(form); return View("Formulario", form);
    }
    [Authorize(Policy = "Administracion")]
    public async Task<IActionResult> Editar(int id)
    {
        var p = await db.Pagos.FindAsync(id);
        if (p is null) return NotFound();
        var form = new PagoForm { Id = id, IdReserva = p.IdReserva, CodigoTransaccion = p.CodigoTransaccion,
            MetodoPago = p.MetodoPago, TieneComprobante = p.Comprobante is not null };
        await Opciones(form); return View("Formulario", form);
    }
    [HttpPost, Authorize(Policy = "Administracion")]
    [RequestSizeLimit(ArchivoService.TamanoMaximo + 1024 * 1024)]
    public async Task<IActionResult> Editar(int id, PagoForm form)
    {
        form.Id = id;
        if (ModelState.IsValid)
        {
            string? archivo = null;
            try
            {
                if (form.Archivo is { Length: > 0 }) archivo = await archivos.GuardarComprobanteAsync(form.Archivo);
                var reemplazado = await service.EditarAsync(id, form, archivo);
                archivo = null;
                archivos.EliminarComprobante(reemplazado);
                TempData["Exito"] = "Comprobante actualizado."; return RedirectToAction(nameof(Detalles), new { id });
            }
            catch (OperacionException e) { ModelState.AddModelError("", e.Message); }
            catch (Exception e) when (ErroresBaseDatos.Es(e))
            { archivo = null; ModelState.AddModelError("", "No se pudo guardar el comprobante. Revise la referencia."); }
            finally { archivos.EliminarComprobante(archivo); } // Solo queda en null si el pago se guardó.
        }
        form.TieneComprobante = await db.Pagos.AnyAsync(p => p.IdPago == id && p.Comprobante != null);
        await Opciones(form); return View("Formulario", form);
    }
    // Sirve el archivo del comprobante solo a quien puede ver el pago (no está en wwwroot).
    [ResponseCache(NoStore = true, Location = ResponseCacheLocation.None)]
    public async Task<IActionResult> Comprobante(int id)
    {
        Response.Headers["X-Content-Type-Options"] = "nosniff";
        var nombre = await Visibles.Where(p => p.IdPago == id).Select(p => p.Comprobante).SingleOrDefaultAsync();
        return archivos.AbrirComprobante(nombre) is { } archivo
            ? PhysicalFile(archivo.Ruta, archivo.Tipo)
            : NotFound();
    }
    [Authorize(Policy = "Administracion")]
    public async Task<IActionResult> Reembolsar(int id)
    {
        var p = await db.Pagos.Include(p => p.Reserva).SingleOrDefaultAsync(p => p.IdPago == id);
        return p is null ? NotFound() : View(p);
    }
    [HttpPost, ActionName("Reembolsar"), Authorize(Policy = "Administracion")]
    public async Task<IActionResult> ReembolsarConfirmado(int id)
    {
        try { await service.ReembolsarAsync(id); TempData["Exito"] = "Reembolso registrado. La reserva fue cancelada."; }
        catch (OperacionException e) { TempData["Error"] = e.Message; }
        catch (Exception e) when (ErroresBaseDatos.Es(e))
        { TempData["Error"] = "No se pudo registrar el reembolso. Actualice e intente nuevamente."; }
        return RedirectToAction(nameof(Detalles), new { id });
    }
    private async Task Opciones(PagoForm form)
    {
        form.Reservas = await db.Reservas.AsNoTracking()
            .Where(r => (r.Estado == EstadoReserva.Pendiente && r.Pago == null && r.FechaInicio > RelojBolivia.Ahora) || (form.Id > 0 && r.IdReserva == form.IdReserva))
            .OrderByDescending(r => r.FechaInicio)
            .Select(r => new SelectListItem(r.Folio + " · " + r.Usuario.Nombre + " · Bs " + r.CostoTotal, r.IdReserva.ToString())).ToListAsync();
    }
}
