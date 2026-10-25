using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.EntityFrameworkCore;
using Workhub.Data;
using Workhub.Models.Enums;

namespace Workhub.Services;

public class SessionEvents(ApplicationContext db, IConfiguration configuration, IWebHostEnvironment environment) : CookieAuthenticationEvents
{
    public override async Task ValidatePrincipal(CookieValidatePrincipalContext context)
    {
        var id = context.Principal?.FindFirstValue(ClaimTypes.NameIdentifier);
        var usuario = int.TryParse(id, out var value)
            ? await db.Usuarios.AsNoTracking().SingleOrDefaultAsync(u => u.IdUsuario == value) : null;
        if (usuario is null || (!environment.IsDevelopment() &&
            usuario.Correo.EndsWith(CredencialesDemo.Dominio, StringComparison.OrdinalIgnoreCase)))
        {
            context.RejectPrincipal();
            await context.HttpContext.SignOutAsync();
            return;
        }
        // Se relee el rol persistido para aplicar cambios de permisos en la siguiente solicitud.
        var principal = CrearPrincipal(usuario, configuration);
        context.ReplacePrincipal(principal);
    }

    public static ClaimsPrincipal CrearPrincipal(Models.Usuario usuario, IConfiguration configuration)
    {
        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, usuario.IdUsuario.ToString()),
            new(ClaimTypes.Name, usuario.Nombre), new(ClaimTypes.Email, usuario.Correo)
        };
        claims.Add(new(ClaimTypes.Role, usuario.Rol == RolUsuario.Admin ? "Administrador" : "Cliente"));
        return new ClaimsPrincipal(new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme));
    }
}
