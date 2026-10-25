using System.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Workhub.Data;
using Workhub.Models;
using Workhub.Services;
using Workhub.ViewModels;

namespace Workhub.Controllers;

public class EquipamientosController(ApplicationContext db, CodigoService codigos) : Controller
{
    public async Task<IActionResult> Index(string? buscar)
    {
        ViewData["Buscar"] = buscar;
        var query = db.Equipamientos.AsNoTracking();
        if (!string.IsNullOrWhiteSpace(buscar))
            query = query.Where(e => e.Nombre.Contains(buscar) || e.Codigo.Contains(buscar));
        return View(await query.OrderBy(e => e.Nombre).ToListAsync());
    }
    public async Task<IActionResult> Detalles(int id)
    {
        var e = await db.Equipamientos.AsNoTracking().SingleOrDefaultAsync(e => e.IdEquipo == id);
        return e is null ? NotFound() : View(e);
    }
    [Authorize(Policy = "Administracion")]
    public async Task<IActionResult> Crear() => await Formulario(new EquipoForm { Codigo = await codigos.EquipoAsync() });
    [HttpPost, Authorize(Policy = "Administracion")]
    public Task<IActionResult> Crear(EquipoForm model) => Guardar(0, model);
    [Authorize(Policy = "Administracion")]
    public async Task<IActionResult> Editar(int id)
    {
        var e = await db.Equipamientos.FindAsync(id);
        return e is null ? NotFound() : await Formulario(new EquipoForm
        { Id = e.IdEquipo, Codigo = e.Codigo, Nombre = e.Nombre, Tipo = e.Tipo,  PrecioHora = e.PrecioHora });
    }
    [HttpPost, Authorize(Policy = "Administracion")]
    public Task<IActionResult> Editar(int id, EquipoForm model) => Guardar(id, model);
    private async Task<IActionResult> Guardar(int id, EquipoForm model)
    {
        model.Id = id;
        if (!ModelState.IsValid) return await Formulario(model);
        int? idIntento = null;
        try
        {
            await Transacciones.Ejecutar(db, async () =>
            {
                await using var tx = await db.Database.BeginTransactionAsync(IsolationLevel.ReadCommitted);
                await Transacciones.Bloquear(db,"WorkHub:Catalogos");
                if (id == 0 && idIntento.HasValue && await db.Equipamientos.AnyAsync(e=>e.IdEquipo==idIntento)) return;
                var e = id == 0 ? new Equipamiento() : await db.Equipamientos.FindAsync(id);
                if (e is null) throw new OperacionException("El equipo ya no existe.");
                e.Codigo = id == 0 ? await codigos.EquipoAsync() : e.Codigo;
                e.Nombre=model.Nombre.Trim(); e.Tipo=model.Tipo.Trim(); e.PrecioHora=model.PrecioHora;
                if (id == 0) db.Equipamientos.Add(e);
                await db.SaveChangesAsync(); idIntento=e.IdEquipo; await tx.CommitAsync();
            });
        }
        catch (OperacionException ex) { ModelState.AddModelError("",ex.Message); return await Formulario(model); }
        catch (Exception ex) when (ErroresBaseDatos.Es(ex))
        { ModelState.AddModelError("", "No se pudo confirmar el guardado. Revise el catálogo antes de volver a intentar."); return await Formulario(model); }
        TempData["Exito"] = "Equipo guardado correctamente.";
        return RedirectToAction(nameof(Index));
    }
    private async Task<IActionResult> Formulario(EquipoForm model)
    {
        model.TiposExistentes = await db.Equipamientos.Select(e => e.Tipo).Distinct().OrderBy(t => t).ToListAsync();
        return View("Formulario", model);
    }
    [Authorize(Policy = "Administracion")]
    public async Task<IActionResult> Eliminar(int id)
    {
        var e = await db.Equipamientos.AsNoTracking().SingleOrDefaultAsync(e => e.IdEquipo == id);
        return e is null ? NotFound() : View(e);
    }
    [HttpPost, ActionName("Eliminar"), Authorize(Policy = "Administracion")]
    public async Task<IActionResult> EliminarConfirmado(int id)
    {
        var e = await db.Equipamientos.FindAsync(id);
        if (e is null) return NotFound();
        if (await db.ReservaEquipamientos.AnyAsync(r => r.IdEquipo == id))
        {
            TempData["Error"] = "Este registro tiene reservas asociadas y no puede eliminarse.";
            return RedirectToAction(nameof(Index));
        }
        db.Equipamientos.Remove(e);
        try { await db.SaveChangesAsync(); }
        catch (DbUpdateException)
        {
            TempData["Error"] = "Otro registro utiliza este elemento; no fue eliminado.";
            return RedirectToAction(nameof(Index));
        }
        TempData["Exito"] = "Registro eliminado.";
        return RedirectToAction(nameof(Index));
    }
}
