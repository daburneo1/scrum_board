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
                    (from task in dbContext.BoardTasks
                        join column in dbContext.BoardColumns
                            on task.ColumnId equals column.Id
                        join assignedUser in dbContext.Users
                            on task.AssignedUserId equals
                            (Guid?)assignedUser.Id
                            into assignedUsers
                        from assignedUser in
                            assignedUsers.DefaultIfEmpty()
                        where
                            column.ProjectId == project.Id &&
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
                                 searchPattern))
                        orderby
                            column.SortOrder,
                            task.SortOrder,
                            task.Id
                        select
                            new ProjectReportTaskDto(
                                task.Id,
                                task.Title,
                                task.Description,
                                column.Name,
                                column.SortOrder,
                                assignedUser != null
                                    ? assignedUser.Name
                                    : null,
                                assignedUser != null
                                    ? assignedUser.Email
                                    : null,
                                task.Priority,
                                task.SortOrder,
                                task.CreatedAtUtc))
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
