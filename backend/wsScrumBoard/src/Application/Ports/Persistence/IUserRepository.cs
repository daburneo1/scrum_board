using Application.Contracts.Boards;
using Domain.Entities;

namespace Application.Ports.Persistence;

public interface IUserRepository
{
    Task<AppUser?> FindByNormalizedEmailAsync(
        string normalizedEmail,
        CancellationToken cancellationToken);
    
    Task<bool> ExistsAsync(
        Guid userId,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyCollection<UserOptionDto>> GetOptionsAsync(
        CancellationToken cancellationToken = default);
}