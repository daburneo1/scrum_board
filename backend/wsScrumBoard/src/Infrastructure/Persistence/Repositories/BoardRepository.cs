using Application.Contracts.Boards;
using Application.Ports.Persistence;
using Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistence.Repositories;

internal sealed class BoardRepository : IBoardRepository
{
    private const int OrderSpacing = 1000;

    private readonly ApplicationDbContext _dbContext;

    public BoardRepository(
        ApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public Task<ProjectBoardDto?> GetBoardAsync(
        Guid projectId,
        CancellationToken cancellationToken = default)
    {
        return _dbContext.Projects
            .AsNoTracking()
            .Where(project => project.Id == projectId)
            .Select(project => new ProjectBoardDto(
                project.Id,
                project.Name,
                project.Columns
                    .OrderBy(column => column.SortOrder)
                    .ThenBy(column => column.Id)
                    .Select(column => new BoardColumnDto(
                        column.Id,
                        column.Name,
                        column.SortOrder,
                        column.Tasks
                            .OrderBy(task => task.SortOrder)
                            .ThenBy(task => task.Id)
                            .Select(task => new BoardTaskDto(
                                task.Id,
                                task.Title,
                                task.Description,
                                task.Priority,
                                task.AssignedUserId,
                                task.AssignedUser != null
                                    ? task.AssignedUser.Name
                                    : null,
                                task.ColumnId,
                                task.SortOrder,
                                task.CreatedAtUtc))
                            .ToList()))
                    .ToList()))
            .SingleOrDefaultAsync(cancellationToken);
    }

    public Task<BoardColumn?> GetColumnAsync(
        Guid projectId,
        Guid columnId,
        CancellationToken cancellationToken = default)
    {
        return _dbContext.BoardColumns
            .SingleOrDefaultAsync(
                column =>
                    column.Id == columnId &&
                    column.ProjectId == projectId,
                cancellationToken);
    }

    public async Task<IReadOnlyCollection<BoardColumn>>
        GetColumnsAsync(
            Guid projectId,
            CancellationToken cancellationToken = default)
    {
        return await _dbContext.BoardColumns
            .Where(column => column.ProjectId == projectId)
            .OrderBy(column => column.SortOrder)
            .ThenBy(column => column.Id)
            .ToListAsync(cancellationToken);
    }

    public Task<bool> ColumnHasTasksAsync(
        Guid columnId,
        CancellationToken cancellationToken = default)
    {
        return _dbContext.BoardTasks
            .AnyAsync(
                task => task.ColumnId == columnId,
                cancellationToken);
    }

    public async Task<int> GetNextColumnSortOrderAsync(
        Guid projectId,
        CancellationToken cancellationToken = default)
    {
        var maximumOrder = await _dbContext.BoardColumns
            .Where(column => column.ProjectId == projectId)
            .MaxAsync(
                column => (int?)column.SortOrder,
                cancellationToken)
            ?? 0;

        return checked(maximumOrder + OrderSpacing);
    }

    public Task<BoardTask?> GetTaskAsync(
        Guid projectId,
        Guid taskId,
        CancellationToken cancellationToken = default)
    {
        return _dbContext.BoardTasks
            .SingleOrDefaultAsync(
                task =>
                    task.Id == taskId &&
                    task.Column.ProjectId == projectId,
                cancellationToken);
    }

    public async Task<int> GetNextTaskSortOrderAsync(
        Guid columnId,
        CancellationToken cancellationToken = default)
    {
        var maximumOrder = await _dbContext.BoardTasks
            .Where(task => task.ColumnId == columnId)
            .MaxAsync(
                task => (int?)task.SortOrder,
                cancellationToken)
            ?? 0;

        return checked(maximumOrder + OrderSpacing);
    }

    public void AddColumn(BoardColumn column)
    {
        _dbContext.BoardColumns.Add(column);
    }

    public void RemoveColumn(BoardColumn column)
    {
        _dbContext.BoardColumns.Remove(column);
    }

    public void AddTask(BoardTask task)
    {
        _dbContext.BoardTasks.Add(task);
    }

    public void RemoveTask(BoardTask task)
    {
        _dbContext.BoardTasks.Remove(task);
    }
}