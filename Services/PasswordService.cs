using Microsoft.AspNetCore.Identity;
using Workhub.Models;

namespace Workhub.Services;

public class PasswordService : IPasswordService
{
    private readonly PasswordHasher<Usuario> _hasher = new();
    public string Hash(string plainPassword) => _hasher.HashPassword(new Usuario(), plainPassword);
    public bool Verify(string plainPassword, string hash)
    {
        if (string.IsNullOrEmpty(plainPassword) || string.IsNullOrEmpty(hash)) return false;
        try
        {
            return _hasher.VerifyHashedPassword(new Usuario(), hash, plainPassword)
                != PasswordVerificationResult.Failed;
        }
        catch (FormatException) { return false; }
    }
}
