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
        var e = id == 0 ? new Equipamiento() : await db.Equipamientos.FindAsync(id);
        if (e is null) return NotFound();
        // El código es correlativo y se asigna al crear; al editar se conserva.
        model.Codigo = id == 0 ? await codigos.EquipoAsync() : e.Codigo;
        if (!ModelState.IsValid) return await Formulario(model);
        e.Codigo = model.Codigo; e.Nombre = model.Nombre.Trim(); e.Tipo = model.Tipo.Trim();
        e.PrecioHora = model.PrecioHora;
        if (id == 0) db.Equipamientos.Add(e);
        try { await db.SaveChangesAsync(); }
        catch (DbUpdateException)
        {
            ModelState.AddModelError("", "No se pudo guardar. Otro administrador registró un equipo al mismo tiempo; intente nuevamente.");
            return await Formulario(model);
        }
        TempData["Exito"] = $"Registro guardado correctamente con el código {e.Codigo}.";
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

