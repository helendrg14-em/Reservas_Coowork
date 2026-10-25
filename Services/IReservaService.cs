namespace Workhub.Services;

public interface IReservaService
{
    Task<bool> EspacioDisponibleAsync(
        int idEspacio,
        DateTime fechaInicio,
        DateTime fechaFin,
        int? idReservaExcluir = null,
        CancellationToken ct = default);
}
