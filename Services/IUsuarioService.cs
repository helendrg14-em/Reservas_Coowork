using Workhub.Models;

namespace Workhub.Services;

public interface IUsuarioService
{
    // Correo en minúsculas y sin espacios, para comparar y guardar siempre igual.
    string NormalizarCorreo(string correo);

    // Devuelve el usuario si el correo y la contraseña coinciden; null en caso contrario.
    Task<Usuario?> ValidarCredencialesAsync(string correo, string password, CancellationToken ct = default);

    // Excluye al usuario con idUsuarioExcluir (útil al editar sus propios datos).
    Task<bool> CorreoEnUsoAsync(string correo, int? idUsuarioExcluir = null, CancellationToken ct = default);
}
