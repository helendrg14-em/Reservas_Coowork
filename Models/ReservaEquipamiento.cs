using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Workhub.Models;

// Relación "incluye" del MER, con atributos propios (cantidad y precio_hora
// congelado al momento de vincular, para conservar el historial financiero).
public class ReservaEquipamiento
{
    public int IdReserva { get; set; }
    public int IdEquipo { get; set; }

    [Range(1, int.MaxValue, ErrorMessage = "La cantidad debe ser mayor a 0.")]
    public int Cantidad { get; set; } = 1;

    [Column(TypeName = "decimal(10,2)")]
    [Display(Name = "Precio por hora al momento de la reserva")]
    public decimal PrecioHora { get; set; }

    public DateTime FechaHora { get; set; } = DateTime.Now;

    public Reserva Reserva { get; set; } = null!;
    public Equipamiento Equipamiento { get; set; } = null!;
}
