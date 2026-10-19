using Workhub.Models;

namespace Workhub.Services;

public interface ICostoService
{
    // Calcula: (PrecioHora_Espacio * horas) + sum(PrecioHora_Equipo * cantidad * horas)
    decimal CalcularCostoTotal(Reserva reserva);
}
