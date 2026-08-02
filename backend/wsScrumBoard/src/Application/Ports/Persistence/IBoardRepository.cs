using Application.Contracts.Boards;
using Domain.Entities;

namespace Application.Ports.Persistence;

public interface IBoardRepository
{
    Task<ProjectBoardDto?> GetBoardAsync(
        Guid projectId,
        CancellationToken cancellationToken = default);

    Task<BoardColumn?> GetColumnAsync(
        Guid projectId,
        Guid columnId,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyCollection<BoardColumn>> GetColumnsAsync(
        Guid projectId,
        CancellationToken cancellationToken = default);

    Task<bool> ColumnHasTasksAsync(
        Guid columnId,
        CancellationToken cancellationToken = default);

    Task<int> GetNextColumnSortOrderAsync(
        Guid projectId,
        CancellationToken cancellationToken = default);

    Task<BoardTask?> GetTaskAsync(
        Guid projectId,
        Guid taskId,
        CancellationToken cancellationToken = default);

    Task<int> GetNextTaskSortOrderAsync(
        Guid columnId,
        CancellationToken cancellationToken = default);

    void AddColumn(BoardColumn column);

    void RemoveColumn(BoardColumn column);

    void AddTask(BoardTask task);

    void RemoveTask(BoardTask task);
}