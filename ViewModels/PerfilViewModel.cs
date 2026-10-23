using System.ComponentModel.DataAnnotations;
using Workhub.Models.Enums;

namespace Workhub.ViewModels;

// El correo y el rol solo se muestran; el usuario edita su nombre y teléfono.
public class PerfilViewModel
{
    [Required(ErrorMessage = "El nombre completo es obligatorio.")]
    [StringLength(100, MinimumLength = 3, ErrorMessage = "El nombre debe tener entre 3 y 100 caracteres.")]
    [Display(Name = "Nombre completo")]
    public string Nombre { get; set; } = string.Empty;

    [Required(ErrorMessage = "El teléfono es obligatorio.")]
    [Phone(ErrorMessage = "Ingrese un teléfono válido.")]
    [StringLength(20)]
    [Display(Name = "Teléfono")]
    public string Telefono { get; set; } = string.Empty;

    [Display(Name = "Correo electrónico")]
    public string Correo { get; set; } = string.Empty;

    [Display(Name = "Rol")]
    public RolUsuario Rol { get; set; }

    [Display(Name = "Reservas realizadas")]
    public int TotalReservas { get; set; }
}
