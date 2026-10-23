using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Workhub.Data;
using Workhub.Extensions;
using Workhub.Models;
using Workhub.Models.Enums;
using Workhub.Services;
using Workhub.ViewModels;

namespace Workhub.Controllers;

// Registro, inicio y cierre de sesión, y datos de la cuenta del usuario actual.
public class CuentaController : Controller
{
    private readonly ApplicationContext _db;
    private readonly IPasswordService _passwords;
    private readonly IUsuarioService _usuarios;

    public CuentaController(ApplicationContext db, IPasswordService passwords, IUsuarioService usuarios)
    {
        _db = db;
        _passwords = passwords;
        _usuarios = usuarios;
    }

    [AllowAnonymous]
    [HttpGet]
    public IActionResult Login(string? returnUrl = null)
    {
        if (User.Identity?.IsAuthenticated == true)
            return RedirectToAction("Index", "Home");

        ViewData["ReturnUrl"] = returnUrl;
        return View(new LoginViewModel());
    }

    [AllowAnonymous]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Login(LoginViewModel model, string? returnUrl = null)
    {
        ViewData["ReturnUrl"] = returnUrl;
        if (!ModelState.IsValid)
            return View(model);

        var usuario = await _usuarios.ValidarCredencialesAsync(model.Correo, model.Password);
        if (usuario is null)
        {
            ModelState.AddModelError(string.Empty, "El correo o la contraseña son incorrectos.");
            return View(model);
        }

        await IniciarSesionAsync(usuario, model.Recordarme);
        TempData["Mensaje"] = $"Bienvenido(a), {usuario.Nombre}.";

        if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
            return LocalRedirect(returnUrl);

        return RedirectToAction("Index", "Home");
    }

    [AllowAnonymous]
    [HttpGet]
    public IActionResult Registro()
    {
        if (User.Identity?.IsAuthenticated == true)
            return RedirectToAction("Index", "Home");

        return View(new RegistroViewModel());
    }

    [AllowAnonymous]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Registro(RegistroViewModel model)
    {
        if (ModelState.IsValid && await _usuarios.CorreoEnUsoAsync(model.Correo))
            ModelState.AddModelError(nameof(model.Correo), "Ya existe una cuenta registrada con este correo.");

        if (!ModelState.IsValid)
            return View(model);

        // El registro público siempre crea clientes; solo un administrador asigna otros roles.
        var usuario = new Usuario
        {
            Nombre = model.Nombre.Trim(),
            Correo = _usuarios.NormalizarCorreo(model.Correo),
            Telefono = model.Telefono.Trim(),
            PasswordHash = _passwords.Hash(model.Password),
            Rol = RolUsuario.Cliente
        };

        _db.Usuarios.Add(usuario);
        await _db.SaveChangesAsync();

        await IniciarSesionAsync(usuario, recordarme: false);
        TempData["Mensaje"] = "Tu cuenta fue creada correctamente.";
        return RedirectToAction("Index", "Home");
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Logout()
    {
        await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        TempData["Mensaje"] = "Cerraste sesión correctamente.";
        return RedirectToAction(nameof(Login));
    }

    [HttpGet]
    public async Task<IActionResult> Perfil()
    {
        var usuario = await BuscarUsuarioActualAsync();
        if (usuario is null)
            return await CerrarSesionInvalidaAsync();

        return View(await CrearPerfilAsync(usuario));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Perfil(PerfilViewModel model)
    {
        var usuario = await BuscarUsuarioActualAsync();
        if (usuario is null)
            return await CerrarSesionInvalidaAsync();

        if (!ModelState.IsValid)
        {
            var perfil = await CrearPerfilAsync(usuario);
            perfil.Nombre = model.Nombre;
            perfil.Telefono = model.Telefono;
            return View(perfil);
        }

        usuario.Nombre = model.Nombre.Trim();
        usuario.Telefono = model.Telefono.Trim();
        await _db.SaveChangesAsync();

        // Se renueva la cookie para que el menú muestre el nombre actualizado.
        await IniciarSesionAsync(usuario, recordarme: false);
        TempData["Mensaje"] = "Tus datos fueron actualizados.";
        return RedirectToAction(nameof(Perfil));
    }

    [HttpGet]
    public IActionResult CambiarPassword() => View(new CambiarPasswordViewModel());

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CambiarPassword(CambiarPasswordViewModel model)
    {
        if (!ModelState.IsValid)
            return View(model);

        var usuario = await BuscarUsuarioActualAsync();
        if (usuario is null)
            return await CerrarSesionInvalidaAsync();

        if (!_passwords.Verify(model.PasswordActual, usuario.PasswordHash))
        {
            ModelState.AddModelError(nameof(model.PasswordActual), "La contraseña actual no es correcta.");
            return View(model);
        }

        usuario.PasswordHash = _passwords.Hash(model.PasswordNueva);
        await _db.SaveChangesAsync();

        TempData["Mensaje"] = "Tu contraseña fue actualizada.";
        return RedirectToAction(nameof(Perfil));
    }

    [AllowAnonymous]
    [HttpGet]
    public IActionResult AccesoDenegado() => View();

    private async Task IniciarSesionAsync(Usuario usuario, bool recordarme)
    {
        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, usuario.IdUsuario.ToString()),
            new(ClaimTypes.Name, usuario.Nombre),
            new(ClaimTypes.Email, usuario.Correo),
            new(ClaimTypes.Role, usuario.Rol.ToString())
        };

        var identidad = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
        await HttpContext.SignInAsync(
            CookieAuthenticationDefaults.AuthenticationScheme,
            new ClaimsPrincipal(identidad),
            new AuthenticationProperties { IsPersistent = recordarme });
    }

    private Task<Usuario?> BuscarUsuarioActualAsync()
    {
        var idUsuario = User.ObtenerIdUsuario();
        return _db.Usuarios.FirstOrDefaultAsync(u => u.IdUsuario == idUsuario);
    }

    private async Task<PerfilViewModel> CrearPerfilAsync(Usuario usuario) => new()
    {
        Nombre = usuario.Nombre,
        Telefono = usuario.Telefono,
        Correo = usuario.Correo,
        Rol = usuario.Rol,
        TotalReservas = await _db.Reservas.CountAsync(r => r.IdUsuario == usuario.IdUsuario)
    };

    // La cookie apunta a un usuario que ya no existe (por ejemplo, fue eliminado por un administrador).
    private async Task<IActionResult> CerrarSesionInvalidaAsync()
    {
        await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        TempData["Error"] = "Tu cuenta ya no está disponible. Inicia sesión nuevamente.";
        return RedirectToAction(nameof(Login));
    }
}
