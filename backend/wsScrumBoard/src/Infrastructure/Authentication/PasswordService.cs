using Application.Ports.Security;
using Domain.Entities;
using Microsoft.AspNetCore.Identity;

namespace Infrastructure.Authentication;

internal sealed class PasswordService : IPasswordService
{
    private readonly PasswordHasher<AppUser> _passwordHasher = new();

    public string HashPassword(
        AppUser user,
        string password)
    {
        if (string.IsNullOrWhiteSpace(password))
        {
            throw new ArgumentException(
                "La contraseña es obligatoria.",
                nameof(password));
        }

        return _passwordHasher.HashPassword(user, password);
    }

    public bool VerifyPassword(
        AppUser user,
        string passwordHash,
        string providedPassword)
    {
        var result = _passwordHasher.VerifyHashedPassword(
            user,
            passwordHash,
            providedPassword);

        return result is
            PasswordVerificationResult.Success or
            PasswordVerificationResult.SuccessRehashNeeded;
    }
}
