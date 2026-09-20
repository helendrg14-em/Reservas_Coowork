namespace Workhub.Models;

public class Equipamiento
{
    public int IdEquipo { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public string Tipo { get; set; } = string.Empty;
    public decimal PrecioHora { get; set; }

    public List<ReservaEquipamiento> ReservaEquipamientos { get; set; } = new();
}
