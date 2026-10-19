using Microsoft.AspNetCore.Identity;
using Workhub.Models;

namespace Workhub.Services;

public class PasswordService : IPasswordService
{
    private readonly PasswordHasher<Usuario> _hasher = new();

    public string Hash(string plainPassword)
    {
        if (string.IsNullOrWhiteSpace(plainPassword))
            throw new ArgumentException("La contraseña no puede estar vacía.", nameof(plainPassword));

        return _hasher.HashPassword(new Usuario(), plainPassword);
    }

    public bool Verify(string plainPassword, string hash)
    {
        if (string.IsNullOrEmpty(plainPassword) || string.IsNullOrEmpty(hash))
            return false;

        var result = _hasher.VerifyHashedPassword(new Usuario(), hash, plainPassword);
        return result != PasswordVerificationResult.Failed;
    }
}
