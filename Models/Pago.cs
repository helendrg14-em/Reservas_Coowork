namespace Workhub.Models;

public class Pago
{
    public int IdPago { get; set; }
    public int IdReserva { get; set; }
    public decimal Monto { get; set; }
    public string MetodoPago { get; set; } = string.Empty;
    public DateTime FechaPago { get; set; }

    public Reserva Reserva { get; set; } = null!;
}
