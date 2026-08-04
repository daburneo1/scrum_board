using Domain.Entities;

namespace Application.Ports.Security;

public interface IPasswordService
{
    string HashPassword(AppUser user, string password);

    bool VerifyPassword(
        AppUser user,
        string passwordHash,
        string providedPassword);
}