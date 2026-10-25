using System.Security.Claims;
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
public class ReservasController(ApplicationContext db, ReservaGestionService gestion, IConfiguration configuration) : Controller
{
    private int UsuarioId => int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
    private bool Admin => User.IsInRole("Administrador");
    private IQueryable<Reserva> Visibles => db.Reservas.Where(r => Admin || r.IdUsuario == UsuarioId);
    public async Task<IActionResult> Index(string? buscar, EstadoReserva? estado, string? situacion)
    {
        ViewData["Buscar"] = buscar; ViewData["Estado"] = estado; ViewData["Situacion"] = situacion;
        var query = Visibles.AsNoTracking().Include(r => r.Espacio).Include(r => r.Usuario).Include(r => r.Pago).AsQueryable();
        if (!string.IsNullOrWhiteSpace(buscar)) query = query.Where(r => r.Folio.Contains(buscar) || r.Espacio.Nombre.Contains(buscar));
        if (estado.HasValue) query = query.Where(r => r.Estado == estado);
        if (situacion == "vencidas") query = query.Where(r => r.Estado == EstadoReserva.Pendiente && r.FechaInicio <= RelojBolivia.Ahora);
        else if (estado == EstadoReserva.Pendiente) query = query.Where(r => r.FechaInicio > RelojBolivia.Ahora);
        return View(await query.OrderByDescending(r => r.FechaInicio).ToListAsync());
    }
    public async Task<IActionResult> Detalles(int id)
    {
        var r = await Visibles.AsNoTracking().Include(r => r.Usuario).Include(r => r.Espacio)
            .Include(r => r.Pago).Include(r => r.ReservaEquipamientos).ThenInclude(e => e.Equipamiento)
            .SingleOrDefaultAsync(r => r.IdReserva == id);
        return r is null ? NotFound() : View(r);
    }
    public async Task<IActionResult> Crear(int? espacioId)
    {
        var model = new ReservaForm { IdUsuario = UsuarioId, IdEspacio = espacioId ?? 0,
            FechaInicio = RelojBolivia.Ahora.Date.AddDays(1).AddHours(9),
            FechaFin = RelojBolivia.Ahora.Date.AddDays(1).AddHours(10) };
        await Opciones(model); return View("Formulario", model);
    }
    [HttpPost]
    public Task<IActionResult> Crear(ReservaForm model, string? siguiente) { model.Id = 0; return Guardar(model, siguiente == "pago"); }
    public async Task<IActionResult> Editar(int id)
    {
        var r = await Visibles.Include(r => r.ReservaEquipamientos).SingleOrDefaultAsync(r => r.IdReserva == id);
        if (r is null) return NotFound();
        var model = new ReservaForm { Id = id, IdUsuario = r.IdUsuario, IdEspacio = r.IdEspacio,
            FechaInicio = r.FechaInicio, FechaFin = r.FechaFin,
            Equipos = r.ReservaEquipamientos.Select(e => new EquipoReservaForm { IdEquipo = e.IdEquipo, Cantidad = e.Cantidad }).ToList() };
        await Opciones(model); return View("Formulario", model);
    }
    [HttpPost]
    public Task<IActionResult> Editar(int id, ReservaForm model) { model.Id = id; return Guardar(model); }
    private async Task<IActionResult> Guardar(ReservaForm model, bool continuarPago = false)
    {
        if (ModelState.IsValid)
        {
            try
            {
                var id = await gestion.GuardarAsync(model, UsuarioId, Admin);
                TempData["Exito"] = "Reserva guardada. El costo se calculó con las tarifas registradas.";
                if (continuarPago && !Admin) return RedirectToAction("Pagar", "Pagos", new { id });
                return RedirectToAction(nameof(Detalles), new { id });
            }
            catch (OperacionException e) { ModelState.AddModelError("", e.Message); }
            catch (Exception e) when (ErroresBaseDatos.Es(e))
            { ModelState.AddModelError("", "No se pudo confirmar el guardado por una operación simultánea o un fallo temporal. Revise sus reservas antes de volver a intentar."); }
        }
        await Opciones(model); return View("Formulario", model);
    }
    private async Task Opciones(ReservaForm model)
    {
        model.Espacios = await db.Espacios.AsNoTracking().OrderBy(e => e.Nombre)
            .Select(e => new SelectListItem(e.Nombre + " · " + e.Capacidad + " personas · Bs " + e.PrecioHora + "/h", e.IdEspacio.ToString())).ToListAsync();
        var administradores = configuration.GetSection("Administracion:Correos").Get<string[]>() ?? [];
        if (Admin) model.Usuarios = await db.Usuarios.AsNoTracking()
            .Where(u => !administradores.Contains(u.Correo)).OrderBy(u => u.Nombre)
            .Select(u => new SelectListItem(u.Nombre + " — " + u.Correo, u.IdUsuario.ToString())).ToListAsync();
        var equipos = await db.Equipamientos.AsNoTracking().OrderBy(e => e.Nombre).ToListAsync();
        model.Equipos = equipos.Select(e => new EquipoReservaForm { IdEquipo = e.IdEquipo, Nombre = e.Nombre,
            PrecioHora = e.PrecioHora, Cantidad = model.Equipos.FirstOrDefault(x => x.IdEquipo == e.IdEquipo)?.Cantidad ?? 0 }).ToList();
    }
    public async Task<IActionResult> Eliminar(int id)
    {
        var r = await Visibles.AsNoTracking().Include(r => r.Espacio).SingleOrDefaultAsync(r => r.IdReserva == id);
        return r is null ? NotFound() : View(r);
    }
    [HttpPost, ActionName("Eliminar")]
    public Task<IActionResult> EliminarConfirmado(int id) => Operar(id, "eliminar");
    [HttpPost] public Task<IActionResult> Cancelar(int id) => Operar(id, "cancelar");
    [HttpPost] public Task<IActionResult> Completar(int id) => Operar(id, "completar");
    private async Task<IActionResult> Operar(int id, string accion)
    {
        try { await gestion.CambiarAsync(id, UsuarioId, Admin, accion); TempData["Exito"] = "Operación completada."; }
        catch (OperacionException e) { TempData["Error"] = e.Message; }
        catch (Exception e) when (ErroresBaseDatos.Es(e))
        { TempData["Error"] = "No se pudo completar la operación. Actualice la página e intente nuevamente."; }
        return RedirectToAction(nameof(Index));
    }
}
