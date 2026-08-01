using Application.Common.Models;
using Domain.Entities;

namespace Application.Ports.Persistence;

public interface IProjectRepository
{
    Task<PagedResult<Project>> SearchAsync(
        string? name,
        int pageNumber,
        int pageSize,
        CancellationToken cancellationToken);

    Task<Project?> GetByIdAsync(
        Guid id,
        CancellationToken cancellationToken);

    Task AddAsync(
        Project project,
        CancellationToken cancellationToken);

    void Remove(Project project);

    Task SaveChangesAsync(
        CancellationToken cancellationToken);
}