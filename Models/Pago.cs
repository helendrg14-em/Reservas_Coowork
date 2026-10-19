using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Workhub.Models.Enums;

namespace Workhub.Models;

public class Pago
{
    public int IdPago { get; set; }

    [Required]
    [StringLength(40)]
    [Display(Name = "Código de transacción")]
    public string CodigoTransaccion { get; set; } = string.Empty;

    public int IdReserva { get; set; }

    [Column(TypeName = "decimal(10,2)")]
    [Range(0.01, double.MaxValue, ErrorMessage = "El monto debe ser mayor a 0.")]
    [Display(Name = "Monto")]
    public decimal Monto { get; set; }

    [Required]
    [Display(Name = "Método de pago")]
    public MetodoPago MetodoPago { get; set; }

    [Required]
    [Display(Name = "Estado del pago")]
    public EstadoPago Estado { get; set; } = EstadoPago.Aprobado;

    [Display(Name = "Fecha y hora de transacción")]
    public DateTime FechaPago { get; set; } = DateTime.Now;

    public Reserva Reserva { get; set; } = null!;
}
