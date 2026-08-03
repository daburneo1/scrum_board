using Application.Contracts.Reports;
using Application.Ports.Reports;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistence.Repositories;

internal sealed class ProjectReportRepository(ApplicationDbContext dbContext) : IProjectReportRepository
{
    public Task<ProjectReportDto?> GetAsync(
        Guid projectId,
        DateTimeOffset generatedAtUtc,
        CancellationToken cancellationToken = default)
    {
        return dbContext.Projects
            .AsNoTracking()
            .Where(project =>
                project.Id == projectId)
            .Select(project =>
                new ProjectReportDto(
                    project.Id,
                    project.Name,
                    project.Description,
                    project.StartDate,
                    project.ExpectedEndDate,
                    project.Status,
                    generatedAtUtc,
                    project.Columns
                        .SelectMany(column =>
                            column.Tasks.Select(task =>
                                new
                                {
                                    Column = column,
                                    Task = task
                                }))
                        .OrderBy(item =>
                            item.Column.SortOrder)
                        .ThenBy(item =>
                            item.Task.SortOrder)
                        .ThenBy(item =>
                            item.Task.Id)
                        .Select(item =>
                            new ProjectReportTaskDto(
                                item.Task.Id,
                                item.Task.Title,
                                item.Task.Description,
                                item.Column.Name,
                                item.Column.SortOrder,
                                item.Task.AssignedUser != null
                                    ? item.Task
                                        .AssignedUser.Name
                                    : null,
                                item.Task.AssignedUser != null
                                    ? item.Task
                                        .AssignedUser.Email
                                    : null,
                                item.Task.Priority,
                                item.Task.SortOrder,
                                item.Task.CreatedAtUtc))
                        .ToList()))
            .SingleOrDefaultAsync(
                cancellationToken);
    }
}