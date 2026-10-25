using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Workhub.Data;
using Workhub.Models;
using Workhub.Models.Enums;
using Workhub.Services;
using Workhub.ViewModels;
namespace Workhub.Controllers;
public class HomeController(ApplicationContext db) : Controller
{
    public async Task<IActionResult> Index()
    {
        var id = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var admin = User.IsInRole("Administrador");
        var q = db.Reservas.AsNoTracking().Where(r => admin || r.IdUsuario == id);
        return View(new DashboardVm
        {
            Espacios = await db.Espacios.CountAsync(), Equipos = await db.Equipamientos.CountAsync(),
            Pendientes = await q.CountAsync(r => r.Estado == EstadoReserva.Pendiente && r.FechaInicio > RelojBolivia.Ahora),
            Vencidas = await q.CountAsync(r => r.Estado == EstadoReserva.Pendiente && r.FechaInicio <= RelojBolivia.Ahora),
            Destacados = await db.Espacios.AsNoTracking().OrderBy(e => e.Tipo).ThenBy(e => e.IdEspacio).Take(6).ToListAsync(),
            Confirmadas = await q.CountAsync(r => r.Estado == EstadoReserva.Confirmada),
            Proximas = await q.Include(r => r.Espacio).Where(r => r.FechaFin >= RelojBolivia.Ahora &&
                ((r.Estado == EstadoReserva.Pendiente && r.FechaInicio > RelojBolivia.Ahora) || r.Estado == EstadoReserva.Confirmada))
                .OrderBy(r => r.FechaInicio).Take(5).ToListAsync()
        });
    }
    [AllowAnonymous, ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error() => View(new ErrorViewModel { RequestId = HttpContext.TraceIdentifier });
}
