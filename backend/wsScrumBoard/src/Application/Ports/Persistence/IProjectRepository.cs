using Application.Common.Models;
using Application.Contracts.Projects;
using Domain.Entities;

namespace Application.Ports.Persistence;

public interface IProjectRepository
{
    Task<PagedResult<ProjectDto>> GetPagedAsync(
        ProjectQueryParameters parameters,
        CancellationToken cancellationToken = default);

    Task<Project?> GetByIdAsync(
        Guid id,
        CancellationToken cancellationToken = default);

    void Add(Project project);

    void Remove(Project project);
    
    Task<bool> ExistsAsync(
        Guid projectId,
        CancellationToken cancellationToken = default);
}