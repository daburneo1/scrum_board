using Domain.Entities;

namespace Application.Ports.Persistence;

public interface IUserRepository
{
    Task<AppUser?> FindByNormalizedEmailAsync(
        string normalizedEmail,
        CancellationToken cancellationToken);
}