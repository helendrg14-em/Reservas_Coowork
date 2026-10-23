using Microsoft.EntityFrameworkCore;
using Workhub.Data;
using Workhub.Models;

namespace Workhub.Services;

public class UsuarioService : IUsuarioService
{
    private readonly ApplicationContext _db;
    private readonly IPasswordService _passwords;

    public UsuarioService(ApplicationContext db, IPasswordService passwords)
    {
        _db = db;
        _passwords = passwords;
    }

    public string NormalizarCorreo(string correo) => correo.Trim().ToLowerInvariant();

    public async Task<Usuario?> ValidarCredencialesAsync(string correo, string password, CancellationToken ct = default)
    {
        var correoNormalizado = NormalizarCorreo(correo);
        var usuario = await _db.Usuarios
            .AsNoTracking()
            .FirstOrDefaultAsync(u => u.Correo == correoNormalizado, ct);

        if (usuario is null || !_passwords.Verify(password, usuario.PasswordHash))
            return null;

        return usuario;
    }

    public Task<bool> CorreoEnUsoAsync(string correo, int? idUsuarioExcluir = null, CancellationToken ct = default)
    {
        var correoNormalizado = NormalizarCorreo(correo);
        return _db.Usuarios.AnyAsync(
            u => u.Correo == correoNormalizado
                 && (!idUsuarioExcluir.HasValue || u.IdUsuario != idUsuarioExcluir.Value),
            ct);
    }
}
