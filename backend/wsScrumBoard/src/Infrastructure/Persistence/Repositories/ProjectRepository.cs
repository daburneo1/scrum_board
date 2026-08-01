using Application.Common.Models;
using Application.Contracts.Projects;
using Application.Ports.Persistence;
using Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistence.Repositories;

internal sealed class ProjectRepository :
    IProjectRepository
{
    private readonly ApplicationDbContext _dbContext;

    public ProjectRepository(
        ApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<PagedResult<ProjectDto>> GetPagedAsync(
        ProjectQueryParameters parameters,
        CancellationToken cancellationToken = default)
    {
        var query = _dbContext.Projects
            .AsNoTracking()
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(parameters.Name))
        {
            var filter = parameters.Name.Trim();

            query = query.Where(project =>
                EF.Functions.ILike(
                    project.Name,
                    $"%{filter}%"));
        }

        var totalCount = await query.CountAsync(
            cancellationToken);

        var items = await query
            .OrderBy(project => project.Name)
            .ThenBy(project => project.Id)
            .Skip(
                (parameters.PageNumber - 1) *
                parameters.PageSize)
            .Take(parameters.PageSize)
            .Select(project => new ProjectDto(
                project.Id,
                project.Name,
                project.Description,
                project.StartDate,
                project.ExpectedEndDate,
                project.Status))
            .ToListAsync(cancellationToken);

        return new PagedResult<ProjectDto>(
            items,
            parameters.PageNumber,
            parameters.PageSize,
            totalCount);
    }

    public Task<Project?> GetByIdAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        return _dbContext.Projects
            .SingleOrDefaultAsync(
                project => project.Id == id,
                cancellationToken);
    }

    public void Add(Project project)
    {
        _dbContext.Projects.Add(project);
    }

    public void Remove(Project project)
    {
        _dbContext.Projects.Remove(project);
    }
}