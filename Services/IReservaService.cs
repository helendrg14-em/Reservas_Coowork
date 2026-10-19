namespace Workhub.Services;

public interface IReservaService
{
    // Verifica si un espacio está disponible en el rango dado.
    // Excluye la reserva con idReservaExcluir (útil al editar una reserva existente).
    Task<bool> EspacioDisponibleAsync(
        int idEspacio,
        DateTime fechaInicio,
        DateTime fechaFin,
        int? idReservaExcluir = null,
        CancellationToken ct = default);
}
