using System.ComponentModel.DataAnnotations;
using Workhub.Models.Enums;

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
    [Display(Name = "Teléfono")]
    public string Telefono { get; set; } = string.Empty;

    // Hash generado con PasswordHasher de ASP.NET Core; nunca se almacena la contraseña en texto plano.
    [Required]
    public string PasswordHash { get; set; } = string.Empty;

    // Atributo agregado al DER: distingue al personal que administra el sistema de los clientes.
    [Required]
    [Display(Name = "Rol")]
    public RolUsuario Rol { get; set; } = RolUsuario.Cliente;

    public List<Reserva> Reservas { get; set; } = new();
}
