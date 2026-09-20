namespace Workhub.Models;

// Relacion "incluye" del MER, con sus propios atributos.
public class ReservaEquipamiento
{
    public int IdReserva { get; set; }
    public int IdEquipo { get; set; }
    public decimal PrecioHora { get; set; }
    public int Cantidad { get; set; }
    public DateTime FechaHora { get; set; }

    public Reserva Reserva { get; set; } = null!;
    public Equipamiento Equipamiento { get; set; } = null!;
}
