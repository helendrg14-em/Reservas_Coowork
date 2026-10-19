using Workhub.Models;

namespace Workhub.Services;

public class CostoService : ICostoService
{
    public decimal CalcularCostoTotal(Reserva reserva)
    {
        if (reserva.Espacio is null)
            throw new InvalidOperationException("La reserva debe tener el Espacio cargado para calcular el costo.");

        var horas = (decimal)Math.Max(0, (reserva.FechaFin - reserva.FechaInicio).TotalHours);
        var costoEspacio = reserva.Espacio.PrecioHora * horas;

        var costoEquipos = reserva.ReservaEquipamientos
            .Sum(re => re.PrecioHora * re.Cantidad * horas);

        return Math.Round(costoEspacio + costoEquipos, 2);
    }
}
