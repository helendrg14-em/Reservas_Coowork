using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Workhub.Models;

public class Equipamiento
{
    public int IdEquipo { get; set; }

    [Required(ErrorMessage = "El código del equipo es obligatorio.")]
    [StringLength(20)]
    [Display(Name = "Código")]
    public string Codigo { get; set; } = string.Empty;

    [Required(ErrorMessage = "El nombre del recurso es obligatorio.")]
    [StringLength(100)]
    public string Nombre { get; set; } = string.Empty;

    [Required(ErrorMessage = "El tipo del recurso es obligatorio.")]
    [StringLength(50)]
    public string Tipo { get; set; } = string.Empty;

    [Column(TypeName = "decimal(10,2)")]
    [Range(0.01, double.MaxValue, ErrorMessage = "El precio por hora debe ser mayor a 0.")]
    [Display(Name = "Precio por hora")]
    public decimal PrecioHora { get; set; }

    public List<ReservaEquipamiento> ReservaEquipamientos { get; set; } = new();
}
