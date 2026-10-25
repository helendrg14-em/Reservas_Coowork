using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using Workhub.Models.Enums;
namespace Workhub.ViewModels;

public class EquipoForm
{
    public int Id { get; set; }
    // Lo asigna el sistema (EQ-01, EQ-02...); el valor enviado se ignora.
    [ValidateNever, Display(Name = "Código")]
    public string Codigo { get; set; } = "";
    [Required, StringLength(100), Display(Name = "Nombre")]
    public string Nombre { get; set; } = "";
    [Required, StringLength(50), Display(Name = "Tipo")]
    public string Tipo { get; set; } = "";
    [Range(typeof(decimal), "0.01", "999999.99", ParseLimitsInInvariantCulture = true, ConvertValueInInvariantCulture = true), Display(Name = "Precio por hora (Bs)")]
    public decimal PrecioHora { get; set; }
    // Tipos ya registrados, sugeridos al escribir para mantener nombres consistentes.
    [ValidateNever] public List<string> TiposExistentes { get; set; } = [];
}
