using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using Workhub.Data;
using Workhub.Models;
using Workhub.Services;
using Workhub.ViewModels;

namespace Workhub.Controllers;

public class CuentaController(ApplicationContext db, IPasswordService passwords, IConfiguration config, IWebHostEnvironment environment) : Controller
{
    private int UsuarioId => int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
    [AllowAnonymous]
    public IActionResult Login(string? returnUrl = null)
    {
        ViewData["ReturnUrl"] = returnUrl;
        return View(new LoginViewModel());
    }
    [HttpPost, AllowAnonymous]
    public async Task<IActionResult> Login(LoginViewModel model, string? returnUrl = null)
    {
        ViewData["ReturnUrl"] = returnUrl;
        if (!ModelState.IsValid) return View(model);
        var correo = (model.Correo ?? "").Trim().ToLowerInvariant();
        var usuario = await db.Usuarios.SingleOrDefaultAsync(u => u.Correo == correo);
        if (usuario is null ||
            (!environment.IsDevelopment() && correo.EndsWith(CredencialesDemo.Dominio, StringComparison.OrdinalIgnoreCase)) ||
            !passwords.Verify(model.Password, usuario.PasswordHash))
        {
            ModelState.AddModelError("", "Correo o contraseña incorrectos.");
            return View(model);
        }
        await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme,
            SessionEvents.CrearPrincipal(usuario, config), new AuthenticationProperties { IsPersistent = model.Recordarme });
        return Url.IsLocalUrl(returnUrl) ? LocalRedirect(returnUrl!) : RedirectToAction("Index", "Home");
    }
    [AllowAnonymous]
    public IActionResult Registro() => View(new RegistroViewModel());
    [HttpPost, AllowAnonymous]
    public async Task<IActionResult> Registro(RegistroViewModel model)
    {
        model.Correo = (model.Correo ?? "").Trim().ToLowerInvariant();
        var administradores = config.GetSection("Administracion:Correos").Get<string[]>() ?? [];
        if (model.Correo.EndsWith(CredencialesDemo.Dominio, StringComparison.OrdinalIgnoreCase) ||
            administradores.Contains(model.Correo, StringComparer.OrdinalIgnoreCase))
            ModelState.AddModelError(nameof(model.Correo), "Este correo está reservado para cuentas administradas por el sistema.");
        if (await db.Usuarios.AnyAsync(u => u.Correo == model.Correo))
            ModelState.AddModelError(nameof(model.Correo), "Este correo ya está registrado.");
        if (!ModelState.IsValid) return View(model);
        db.Usuarios.Add(new Usuario
        {
            Nombre = model.Nombre.Trim(), Correo = model.Correo, Telefono = model.Telefono.Trim(),
            PasswordHash = passwords.Hash(model.Password)
        });
        try { await db.SaveChangesAsync(); }
        catch (DbUpdateException)
        {
            ModelState.AddModelError("", "No se pudo registrar la cuenta. Compruebe que el correo no esté registrado.");
            return View(model);
        }
        TempData["Exito"] = "Cuenta creada. Ya puede iniciar sesión.";
        return RedirectToAction(nameof(Login));
    }
    public async Task<IActionResult> Index()
    {
        var u = await db.Usuarios.SingleAsync(u => u.IdUsuario == UsuarioId);
        return View(new PerfilViewModel { Nombre = u.Nombre, Telefono = u.Telefono, Correo = u.Correo });
    }
    [HttpPost]
    public async Task<IActionResult> Index(PerfilViewModel model)
    {
        var u = await db.Usuarios.SingleAsync(u => u.IdUsuario == UsuarioId);
        model.Correo = u.Correo;
        if (!ModelState.IsValid) return View(model);
        u.Nombre = model.Nombre.Trim(); u.Telefono = model.Telefono.Trim();
        await db.SaveChangesAsync();
        TempData["Exito"] = "Perfil actualizado.";
        return RedirectToAction(nameof(Index));
    }
    public IActionResult Eliminar() => View();
    [HttpPost, ActionName("Eliminar")]
    public async Task<IActionResult> EliminarConfirmado()
    {
        if (await db.Reservas.AnyAsync(r => r.IdUsuario == UsuarioId))
        {
            TempData["Error"] = "Su cuenta tiene reservas. Se conserva para mantener el historial.";
            return RedirectToAction(nameof(Index));
        }
        var usuario = await db.Usuarios.SingleAsync(u => u.IdUsuario == UsuarioId);
        db.Usuarios.Remove(usuario);
        try { await db.SaveChangesAsync(); }
        catch (DbUpdateException)
        {
            TempData["Error"] = "La cuenta tiene registros relacionados y no puede eliminarse.";
            return RedirectToAction(nameof(Index));
        }
        await HttpContext.SignOutAsync();
        return RedirectToAction(nameof(Login));
    }
    [HttpPost]
    public async Task<IActionResult> Salir()
    {
        await HttpContext.SignOutAsync();
        return RedirectToAction(nameof(Login));
    }
    [AllowAnonymous]
    public IActionResult AccesoDenegado() => View();
}
