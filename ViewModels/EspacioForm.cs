using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using Workhub.Models.Enums;
namespace Workhub.ViewModels;

public class EspacioForm
{
    public int Id { get; set; }
    // Lo asigna el sistema según el tipo (ESC-, OFI-, SAL-); el valor enviado se ignora.
    [ValidateNever, Display(Name = "Código")]
    public string Codigo { get; set; } = "";
    [Required, StringLength(100), Display(Name = "Nombre")]
    public string Nombre { get; set; } = "";
    [EnumDataType(typeof(TipoEspacio)), Display(Name = "Tipo de espacio")]
    public TipoEspacio Tipo { get; set; } = TipoEspacio.EscritorioFlexible;
    [Range(1, 100), Display(Name = "Capacidad de personas")]
    public int Capacidad { get; set; } = 1;
    [Range(typeof(decimal), "0.01", "999999.99", ParseLimitsInInvariantCulture = true, ConvertValueInInvariantCulture = true), Display(Name = "Precio por hora (Bs)")]
    public decimal PrecioHora { get; set; }
    [Display(Name = "Fotografía del espacio")]
    public IFormFile? Archivo { get; set; }
    [Display(Name = "Quitar la fotografía actual")]
    public bool QuitarFoto { get; set; }
    [ValidateNever] public string? FotoActual { get; set; }
    [ValidateNever] public TipoEspacio? TipoOriginal { get; set; }
    // Próximo código por tipo, para mostrarlo al cambiar el tipo en el formulario.
    [ValidateNever] public Dictionary<TipoEspacio, string> CodigosSugeridos { get; set; } = [];
}
