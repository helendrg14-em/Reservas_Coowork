using System.Security.Claims;
using Workhub.Models.Enums;

namespace Workhub.Extensions;

// Acceso a los datos del usuario que inició sesión, para usar en cualquier controlador:
// User.ObtenerIdUsuario(), User.EsAdmin().
public static class ClaimsPrincipalExtensions
{
    public static int ObtenerIdUsuario(this ClaimsPrincipal usuario)
    {
        var valor = usuario.FindFirstValue(ClaimTypes.NameIdentifier);
        return int.TryParse(valor, out var id)
            ? id
            : throw new InvalidOperationException("No hay un usuario con sesión iniciada.");
    }

    public static bool EsAdmin(this ClaimsPrincipal usuario) =>
        usuario.IsInRole("Administrador");
}
