using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using Microsoft.AspNetCore.Mvc.Rendering;
namespace Workhub.ViewModels;

public class ReservaForm : IValidatableObject
{
    public int Id { get; set; }
    [Display(Name = "Cliente")]
    public int IdUsuario { get; set; }
    [Range(1, int.MaxValue, ErrorMessage = "Seleccione un espacio."), Display(Name = "Espacio")]
    public int IdEspacio { get; set; }
    [Display(Name = "Inicio (hora de Bolivia)")]
    public DateTime FechaInicio { get; set; }
    [Display(Name = "Fin (hora de Bolivia)")]
    public DateTime FechaFin { get; set; }
    public List<EquipoReservaForm> Equipos { get; set; } = [];
    [ValidateNever] public List<SelectListItem> Espacios { get; set; } = [];
    [ValidateNever] public List<SelectListItem> Usuarios { get; set; } = [];
    public IEnumerable<ValidationResult> Validate(ValidationContext context)
    {
        if (FechaInicio < new DateTime(2020, 1, 1) || FechaInicio > new DateTime(2100, 1, 1))
            yield return new("Seleccione una fecha de inicio válida.", [nameof(FechaInicio)]);
        if (FechaFin <= FechaInicio || FechaFin - FechaInicio > TimeSpan.FromDays(7))
            yield return new("La duración debe ser mayor a cero y no superar 7 días.", [nameof(FechaFin)]);
        if (Equipos.Select(e => e.IdEquipo).Distinct().Count() != Equipos.Count)
            yield return new("No se puede repetir un equipo en la reserva.");
    }
}
public class EquipoReservaForm
{
    public int IdEquipo { get; set; }
    [ValidateNever] public string Nombre { get; set; } = "";
    [ValidateNever] public decimal PrecioHora { get; set; }
    [Range(0, 20, ErrorMessage = "La cantidad debe estar entre 0 y 20.")]
    public int Cantidad { get; set; }
}
