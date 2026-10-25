using System.ComponentModel.DataAnnotations;

namespace Workhub.Models;

public class Usuario
{
    public int IdUsuario { get; set; }

    [Required(ErrorMessage = "El nombre completo es obligatorio.")]
    [StringLength(100, MinimumLength = 3, ErrorMessage = "El nombre debe tener entre 3 y 100 caracteres.")]
    [Display(Name = "Nombre completo")]
    public string Nombre { get; set; } = string.Empty;

    [Required(ErrorMessage = "El correo es obligatorio.")]
    [EmailAddress(ErrorMessage = "Ingrese un correo válido.")]
    [StringLength(150)]
    [Display(Name = "Correo electrónico")]
    public string Correo { get; set; } = string.Empty;

    [Required(ErrorMessage = "El teléfono es obligatorio.")]
    [Phone(ErrorMessage = "Ingrese un teléfono válido.")]
    [StringLength(20)]
    public string Telefono { get; set; } = string.Empty;

    // Hash ASP.NET Core Identity (PBKDF2); nunca se almacena la contraseña en texto plano.
    [Required]
    public string PasswordHash { get; set; } = string.Empty;

    public List<Reserva> Reservas { get; set; } = new();
}
