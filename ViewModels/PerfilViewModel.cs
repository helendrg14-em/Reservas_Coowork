using System.ComponentModel.DataAnnotations;
namespace Workhub.ViewModels;
public class PerfilViewModel
{
    [Required, StringLength(100, MinimumLength = 3), Display(Name = "Nombre completo")]
    public string Nombre { get; set; } = "";
    [Required, Phone, StringLength(20), Display(Name = "Teléfono")]
    public string Telefono { get; set; } = "";
    public string Correo { get; set; } = "";
}
