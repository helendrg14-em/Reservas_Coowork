using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Workhub.Models.Enums;

namespace Workhub.Models;

public class Espacio
{
    public int IdEspacio { get; set; }

    [Required(ErrorMessage = "El código del espacio es obligatorio.")]
    [StringLength(20)]
    [Display(Name = "Código")]
    public string Codigo { get; set; } = string.Empty;

    [Required(ErrorMessage = "El nombre o número del espacio es obligatorio.")]
    [StringLength(100)]
    [Display(Name = "Nombre o número")]
    public string Nombre { get; set; } = string.Empty;

    [Required]
    [Display(Name = "Tipo de espacio")]
    public TipoEspacio Tipo { get; set; }

    [Range(1, 100, ErrorMessage = "La capacidad debe ser mayor a 0.")]
    [Display(Name = "Capacidad máxima")]
    public int Capacidad { get; set; }

    [Column(TypeName = "decimal(10,2)")]
    [Range(0.01, double.MaxValue, ErrorMessage = "La tarifa debe ser mayor a 0.")]
    [Display(Name = "Precio por hora")]
    public decimal PrecioHora { get; set; }

    public List<Reserva> Reservas { get; set; } = new();
}
