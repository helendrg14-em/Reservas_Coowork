using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.EntityFrameworkCore;
using Workhub.Data;

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
        // La lista administrativa se revalida: quitar un correo revoca su acceso.
        var principal = CrearPrincipal(usuario, configuration);
        context.ReplacePrincipal(principal);
    }

    public static ClaimsPrincipal CrearPrincipal(Models.Usuario usuario, IConfiguration configuration)
    {
        var admins = configuration.GetSection("Administracion:Correos").Get<string[]>() ?? [];
        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, usuario.IdUsuario.ToString()),
            new(ClaimTypes.Name, usuario.Nombre), new(ClaimTypes.Email, usuario.Correo)
        };
        if (admins.Contains(usuario.Correo, StringComparer.OrdinalIgnoreCase))
            claims.Add(new(ClaimTypes.Role, "Administrador"));
        return new ClaimsPrincipal(new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme));
    }
}
