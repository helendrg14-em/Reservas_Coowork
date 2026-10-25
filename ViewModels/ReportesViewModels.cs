using System.ComponentModel.DataAnnotations;
using Workhub.Models;
using Workhub.Models.Enums;
using Workhub.Services;
namespace Workhub.ViewModels;
public class FiltroReporte : IValidatableObject
{
    [DataType(DataType.Date), Display(Name = "Desde")]
    public DateTime Desde { get; set; } = new(RelojBolivia.Ahora.Year, RelojBolivia.Ahora.Month, 1);
    [DataType(DataType.Date), Display(Name = "Hasta")]
    public DateTime Hasta { get; set; } = RelojBolivia.Ahora.Date;
    [EnumDataType(typeof(EstadoReserva)), Display(Name = "Estado")]
    public EstadoReserva? Estado { get; set; }
    [EnumDataType(typeof(MetodoPago)), Display(Name = "Método de pago")]
    public MetodoPago? Metodo { get; set; }
    public IEnumerable<ValidationResult> Validate(ValidationContext context)
    {
        if (Desde.Year < 2000 || Hasta.Year > 2100 || Hasta.Date < Desde.Date)
            yield return new("Ingrese un rango de fechas válido entre 2000 y 2100.");
        else if ((Hasta - Desde).TotalDays > 366)
            yield return new("Consulte como máximo 366 días por reporte.");
    }
}
public class ReporteReservasVm
{
    public FiltroReporte Filtro { get; set; } = new();
    public List<Reserva> Filas { get; set; } = [];
    public Paginacion Paginacion { get; set; } = Paginacion.Crear(1, 0, 15);
    public decimal Importe => Filas.Where(r => r.Estado != EstadoReserva.Cancelada).Sum(r => r.CostoTotal);
}
public class ReporteIngresosVm
{
    public FiltroReporte Filtro { get; set; } = new();
    public List<Pago> Filas { get; set; } = [];
    public Paginacion Paginacion { get; set; } = Paginacion.Crear(1, 0, 15);
    public decimal Aprobados => Filas.Where(p => p.Estado == EstadoPago.Aprobado).Sum(p => p.Monto);
    public decimal Reembolsados => Filas.Where(p => p.Estado == EstadoPago.Reembolsado).Sum(p => p.Monto);
}
