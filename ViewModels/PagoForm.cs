using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using Microsoft.AspNetCore.Mvc.Rendering;
using Workhub.Models.Enums;
namespace Workhub.ViewModels;
public class PagoForm
{
    public int Id { get; set; }
    [Range(1, int.MaxValue, ErrorMessage = "Seleccione una reserva."), Display(Name = "Reserva")]
    public int IdReserva { get; set; }
    // N° de operación del banco, código de la transacción QR o N° de recibo. Si se deja vacío
    // (por ejemplo, pago en efectivo) el sistema genera uno para identificar el comprobante.
    [StringLength(40), Display(Name = "N° de operación / código de verificación")]
    public string? CodigoTransaccion { get; set; }
    [EnumDataType(typeof(MetodoPago)), Display(Name = "Método de pago")]
    public MetodoPago MetodoPago { get; set; } = MetodoPago.Efectivo;
    [Display(Name = "Comprobante (imagen o PDF)")]
    public IFormFile? Archivo { get; set; }
    [ValidateNever] public bool TieneComprobante { get; set; }
    [ValidateNever] public List<SelectListItem> Reservas { get; set; } = [];
}
