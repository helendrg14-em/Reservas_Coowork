using Microsoft.EntityFrameworkCore;
using Workhub.Data;
using Workhub.Models.Enums;

namespace Workhub.Services;

public class ReservaService : IReservaService
{
    private readonly ApplicationContext _db;

    public ReservaService(ApplicationContext db)
    {
        _db = db;
    }

    public async Task<bool> EspacioDisponibleAsync(
        int idEspacio,
        DateTime fechaInicio,
        DateTime fechaFin,
        int? idReservaExcluir = null,
        CancellationToken ct = default)
    {
        if (fechaFin <= fechaInicio) return false;

        // dos reservas chocan si una empieza antes de que termine la otra y termina despues de que empieza la otra
        // las pendientes que ya vencieron no bloquean el horario
        var query = _db.Reservas
            .AsNoTracking()
            .Where(r => r.IdEspacio == idEspacio
                        && r.Estado != EstadoReserva.Cancelada
                        && (r.Estado != EstadoReserva.Pendiente || r.FechaInicio > RelojBolivia.Ahora)
                        && r.FechaInicio < fechaFin
                        && r.FechaFin > fechaInicio);

        if (idReservaExcluir.HasValue)
            query = query.Where(r => r.IdReserva != idReservaExcluir.Value);

        var existeChoque = await query.AnyAsync(ct);
        return !existeChoque;
    }
}
