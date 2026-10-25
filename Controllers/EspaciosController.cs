using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Workhub.Data;
using System.Data;
using Workhub.Models;
using Workhub.Services;
using Workhub.ViewModels;

namespace Workhub.Controllers;

public class EspaciosController(ApplicationContext db, CodigoService codigos, ArchivoService archivos) : Controller
{
    private const int EspaciosPorPagina = 9;

    public async Task<IActionResult> Index(string? buscar, int pagina = 1)
    {
        ViewData["Buscar"] = buscar;
        var query = db.Espacios.AsNoTracking();
        if (!string.IsNullOrWhiteSpace(buscar))
            query = query.Where(e => e.Nombre.Contains(buscar) || e.Codigo.Contains(buscar));
        var paginacion = Paginacion.Crear(pagina, await query.CountAsync(), EspaciosPorPagina);
        ViewData["Paginacion"] = paginacion;
        return View(await query.OrderBy(e => e.Nombre).ThenBy(e => e.IdEspacio)
            .Skip(paginacion.Saltar).Take(paginacion.TamanoPagina).ToListAsync());
    }
    public async Task<IActionResult> Detalles(int id)
    {
        var e = await db.Espacios.AsNoTracking().SingleOrDefaultAsync(e => e.IdEspacio == id);
        return e is null ? NotFound() : View(e);
    }
    [Authorize(Policy = "Administracion")]
    public async Task<IActionResult> Crear()
    {
        var model = new EspacioForm();
        await Opciones(model);
        return View("Formulario", model);
    }
    [HttpPost, Authorize(Policy = "Administracion")]
    [RequestSizeLimit(ArchivoService.TamanoMaximo + 1024 * 1024)]
    public Task<IActionResult> Crear(EspacioForm model) => Guardar(0, model);
    [Authorize(Policy = "Administracion")]
    public async Task<IActionResult> Editar(int id)
    {
        var e = await db.Espacios.FindAsync(id);
        if (e is null) return NotFound();
        var model = new EspacioForm
        {
            Id = e.IdEspacio, Codigo = e.Codigo, Nombre = e.Nombre, Tipo = e.Tipo, Capacidad = e.Capacidad,
            PrecioHora = e.PrecioHora, FotoActual = e.Foto, TipoOriginal = e.Tipo
        };
        await Opciones(model);
        return View("Formulario", model);
    }
    [HttpPost, Authorize(Policy = "Administracion")]
    [RequestSizeLimit(ArchivoService.TamanoMaximo + 1024 * 1024)]
    public Task<IActionResult> Editar(int id, EspacioForm model) => Guardar(id, model);
    private async Task<IActionResult> Guardar(int id, EspacioForm model)
    {
        model.Id = id;
        var actual = id == 0 ? null : await db.Espacios.AsNoTracking().SingleOrDefaultAsync(e=>e.IdEspacio==id);
        if (id > 0 && actual is null) return NotFound();
        model.FotoActual = actual?.Foto; model.TipoOriginal = actual?.Tipo; model.Codigo = actual?.Codigo ?? "";
        if (!ModelState.IsValid) return await Formulario(model);
        string? fotoNueva = null;
        try
        {
            if (model.Archivo is { Length: > 0 }) fotoNueva = await archivos.GuardarFotoEspacioAsync(model.Archivo);
        }
        catch (OperacionException ex) { ModelState.AddModelError(nameof(model.Archivo),ex.Message); return await Formulario(model); }
        int? idIntento = null;
        string? fotoAnterior = null;
        try
        {
            await Transacciones.Ejecutar(db, async () =>
            {
                await using var tx = await db.Database.BeginTransactionAsync(IsolationLevel.ReadCommitted);
                await Transacciones.Bloquear(db,"WorkHub:Catalogos");
                if (id == 0 && idIntento.HasValue && await db.Espacios.AnyAsync(e=>e.IdEspacio==idIntento)) return;
                var e = id == 0 ? new Espacio() : await db.Espacios.FindAsync(id);
                if (e is null) throw new OperacionException("El espacio ya no existe.");
                if (id == 0 || e.Tipo != model.Tipo) e.Codigo = await codigos.EspacioAsync(model.Tipo);
                fotoAnterior = e.Foto == fotoNueva ? null : e.Foto;
                if (fotoNueva is not null || model.QuitarFoto) e.Foto = fotoNueva;
                e.Nombre = model.Nombre.Trim(); e.Tipo = model.Tipo; e.PrecioHora = model.PrecioHora; e.Capacidad = model.Capacidad;
                if (id == 0) db.Espacios.Add(e);
                await db.SaveChangesAsync(); idIntento=e.IdEspacio; await tx.CommitAsync();
            });
        }
        catch (OperacionException ex) { archivos.EliminarFotoEspacio(fotoNueva); ModelState.AddModelError("",ex.Message); return await Formulario(model); }
        catch (Exception ex) when (ErroresBaseDatos.Es(ex))
        {
            // Si se perdió la conexión al confirmar, se conserva el archivo: podría estar referenciado.
            ModelState.AddModelError("", "No se pudo confirmar el guardado. Revise el catálogo antes de volver a intentar.");
            return await Formulario(model);
        }
        if (fotoNueva is not null || model.QuitarFoto) archivos.EliminarFotoEspacio(fotoAnterior);
        TempData["Exito"] = "Espacio guardado correctamente.";
        return RedirectToAction(nameof(Index));
    }
    private async Task<IActionResult> Formulario(EspacioForm model)
    {
        await Opciones(model);
        return View("Formulario", model);
    }
    private async Task Opciones(EspacioForm model)
    {
        model.CodigosSugeridos = await codigos.EspaciosPorTipoAsync();
        // Al editar, el tipo original conserva su código actual.
        if (model.TipoOriginal is { } original) model.CodigosSugeridos[original] = model.Codigo;
        model.Codigo = model.CodigosSugeridos.GetValueOrDefault(model.Tipo, "Seleccione un tipo válido");
    }
    [Authorize(Policy = "Administracion")]
    public async Task<IActionResult> Eliminar(int id)
    {
        var e = await db.Espacios.AsNoTracking().SingleOrDefaultAsync(e => e.IdEspacio == id);
        return e is null ? NotFound() : View(e);
    }
    [HttpPost, ActionName("Eliminar"), Authorize(Policy = "Administracion")]
    public async Task<IActionResult> EliminarConfirmado(int id)
    {
        var e = await db.Espacios.FindAsync(id);
        if (e is null) return NotFound();
        if (await db.Reservas.AnyAsync(r => r.IdEspacio == id))
        {
            TempData["Error"] = "Este registro tiene reservas asociadas y no puede eliminarse.";
            return RedirectToAction(nameof(Index));
        }
        db.Espacios.Remove(e);
        try { await db.SaveChangesAsync(); }
        catch (DbUpdateException)
        {
            TempData["Error"] = "Otro registro utiliza este elemento; no fue eliminado.";
            return RedirectToAction(nameof(Index));
        }
        archivos.EliminarFotoEspacio(e.Foto);
        TempData["Exito"] = "Registro eliminado.";
        return RedirectToAction(nameof(Index));
    }
}
