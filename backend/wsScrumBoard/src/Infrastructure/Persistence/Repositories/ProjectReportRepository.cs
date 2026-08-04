using Application.Contracts.Reports;
using Application.Contracts.Tasks;
using Application.Ports.Reports;
using Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistence.Repositories;

internal sealed class ProjectReportRepository(ApplicationDbContext dbContext) : IProjectReportRepository
{
    public Task<ProjectReportDto?> GetAsync(
        Guid projectId,
        DateTimeOffset generatedAtUtc,
        ProjectTaskFilter filter,
        CancellationToken cancellationToken = default)
    {
        var assigneeId = filter.AssigneeId;
        var priority = filter.Priority;
        var search = filter.Search;
        var searchPattern = search is null
            ? null
            : $"%{search}%";
        var hasActiveFilters = filter.HasActiveFilters;
        var priorityDisplay = priority.HasValue
            ? FormatPriority(priority.Value)
            : "Todas";
        var searchDisplay = search ?? "Sin búsqueda";

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
                    new ProjectReportAppliedFiltersDto(
                        assigneeId.HasValue
                            ? dbContext.Users
                                  .Where(user => user.Id == assigneeId.Value)
                                  .Select(user => user.Name)
                                  .SingleOrDefault() ??
                              assigneeId.Value.ToString()
                            : "Todos",
                        priorityDisplay,
                        searchDisplay,
                        hasActiveFilters),
                    project.Columns
                        .SelectMany(column =>
                            column.Tasks
                                .Where(task =>
                                    (!assigneeId.HasValue ||
                                     task.AssignedUserId == assigneeId.Value) &&
                                    (!priority.HasValue ||
                                     task.Priority == priority.Value) &&
                                    (searchPattern == null ||
                                     EF.Functions.ILike(
                                         task.Title,
                                         searchPattern) ||
                                     EF.Functions.ILike(
                                         task.Description ?? string.Empty,
                                         searchPattern)))
                                .Select(task =>
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

    private static string FormatPriority(
        WorkItemPriority priority)
    {
        return priority switch
        {
            WorkItemPriority.Low => "Low",
            WorkItemPriority.Medium => "Medium",
            WorkItemPriority.High => "High",
            WorkItemPriority.Critical => "Critical",
            _ => priority.ToString()
        };
    }
}
