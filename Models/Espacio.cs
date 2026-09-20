namespace Workhub.Models;

public class Espacio
{
    public int IdEspacio { get; set; }
    public string Tipo { get; set; } = string.Empty;
    public int Capacidad { get; set; }
    public decimal PrecioHora { get; set; }

    public List<Reserva> Reservas { get; set; } = new();
}
