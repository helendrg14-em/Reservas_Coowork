using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Workhub.Data;
using Workhub.ViewModels;
namespace Workhub.Controllers;

[Authorize(Policy = "Administracion")]
public class ReportesController(ApplicationContext db) : Controller
{
    // Totales y resúmenes usan todas las filas; la tabla de detalle se pagina en pantalla.
    private const int FilasPorPagina = 15;
    public IActionResult Index() => View();
    public async Task<IActionResult> Reservas([Bind(Prefix = "Filtro")] FiltroReporte filtro, int pagina = 1)
    {
        var model = new ReporteReservasVm { Filtro = filtro };
        if (ModelState.IsValid)
        {
            var fin = filtro.Hasta.Date.AddDays(1);
            var q = db.Reservas.AsNoTracking().Include(r => r.Espacio).Include(r => r.Usuario)
                .Where(r => r.FechaInicio >= filtro.Desde.Date && r.FechaInicio < fin);
            if (filtro.Estado.HasValue) q = q.Where(r => r.Estado == filtro.Estado);
            model.Filas = await q.OrderBy(r => r.FechaInicio).ToListAsync();
            model.Paginacion = Paginacion.Crear(pagina, model.Filas.Count, FilasPorPagina);
        }
        return View(model);
    }
    public async Task<IActionResult> Ingresos([Bind(Prefix = "Filtro")] FiltroReporte filtro, int pagina = 1)
    {
        var model = new ReporteIngresosVm { Filtro = filtro };
        if (ModelState.IsValid)
        {
            var fin = filtro.Hasta.Date.AddDays(1);
            var q = db.Pagos.AsNoTracking().Include(p => p.Reserva).ThenInclude(r => r.Usuario)
                .Where(p => p.FechaPago >= filtro.Desde.Date && p.FechaPago < fin);
            if (filtro.Metodo.HasValue) q = q.Where(p => p.MetodoPago == filtro.Metodo);
            model.Filas = await q.OrderBy(p => p.FechaPago).ToListAsync();
            model.Paginacion = Paginacion.Crear(pagina, model.Filas.Count, FilasPorPagina);
        }
        return View(model);
    }
}
