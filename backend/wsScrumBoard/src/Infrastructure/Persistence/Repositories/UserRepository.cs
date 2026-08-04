using Application.Contracts.Boards;
using Application.Ports.Persistence;
using Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistence.Repositories;

internal sealed class UserRepository : IUserRepository
{
    private readonly ApplicationDbContext _dbContext;

    public UserRepository(ApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public Task<AppUser?> FindByNormalizedEmailAsync(
        string normalizedEmail,
        CancellationToken cancellationToken = default)
    {
        return _dbContext.Users
            .AsNoTracking()
            .SingleOrDefaultAsync(
                user => user.NormalizedEmail == normalizedEmail,
                cancellationToken);
    }
    
    public Task<bool> ExistsAsync(
        Guid userId,
        CancellationToken cancellationToken = default)
    {
        return _dbContext.Users
            .AnyAsync(
                user => user.Id == userId,
                cancellationToken);
    }

    public async Task<IReadOnlyCollection<UserOptionDto>>
        GetOptionsAsync(
            CancellationToken cancellationToken = default)
    {
        return await _dbContext.Users
            .AsNoTracking()
            .OrderBy(user => user.Name)
            .ThenBy(user => user.Id)
            .Select(user => new UserOptionDto(
                user.Id,
                user.Name,
                user.Email))
            .ToListAsync(cancellationToken);
    }
}