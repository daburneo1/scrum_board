using Domain.Entities;

namespace Application.Ports.Security;

public interface ITokenService
{
    string GenerateToken(AppUser user);
}