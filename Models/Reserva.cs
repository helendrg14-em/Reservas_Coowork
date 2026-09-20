namespace Workhub.Models;

public class Reserva
{
    public int IdReserva { get; set; }
    public string Estado { get; set; } = string.Empty;
    public DateTime FechaInicio { get; set; }
    public DateTime FechaFin { get; set; }

    public int IdUsuario { get; set; }
    public Usuario Usuario { get; set; } = null!;
    public int IdEspacio { get; set; }
    public Espacio Espacio { get; set; } = null!;
    public Pago? Pago { get; set; }
    public List<ReservaEquipamiento> ReservaEquipamientos { get; set; } = new();
}
