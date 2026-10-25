using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using Workhub.Models;
using Workhub.Models.Enums;
namespace Workhub.ViewModels;

public class PagoClienteForm : IValidatableObject
{
    [EnumDataType(typeof(MetodoPago), ErrorMessage = "Seleccione un método válido.")]
    [Display(Name = "Método de pago")]
    public MetodoPago MetodoPago { get; set; } = MetodoPago.Tarjeta;
    public bool ConfirmarSimulacion { get; set; }
    // N° de operación del banco o código de verificación del QR (opcional).
    [StringLength(40, ErrorMessage = "El N° de operación admite hasta 40 caracteres.")]
    [Display(Name = "N° de operación / código de verificación")]
    public string? CodigoTransaccion { get; set; }
    [Display(Name = "Comprobante (imagen o PDF)")]
    public IFormFile? Archivo { get; set; }
    [ValidateNever] public Reserva Reserva { get; set; } = null!;
    [ValidateNever] public bool SimulacionHabilitada { get; set; }
    public IEnumerable<ValidationResult> Validate(ValidationContext context)
    {
        if (!ConfirmarSimulacion)
            yield return new("Confirme que desea registrar el importe indicado.", [nameof(ConfirmarSimulacion)]);
    }
}
