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

        // Hay solapamiento si: (A.inicio < B.fin) && (A.fin > B.inicio)
        // Solo contamos las reservas activas (no canceladas).
        var query = _db.Reservas
            .AsNoTracking()
            .Where(r => r.IdEspacio == idEspacio
                        && r.Estado != EstadoReserva.Cancelada
                        && r.FechaInicio < fechaFin
                        && r.FechaFin > fechaInicio);

        if (idReservaExcluir.HasValue)
            query = query.Where(r => r.IdReserva != idReservaExcluir.Value);

        var existeChoque = await query.AnyAsync(ct);
        return !existeChoque;
    }
}
