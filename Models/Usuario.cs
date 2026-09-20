namespace Workhub.Models;

public class Usuario
{
    public int IdUsuario { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public string Correo { get; set; } = string.Empty;
    public string Telefono { get; set; } = string.Empty;

    public List<Reserva> Reservas { get; set; } = new();
}
