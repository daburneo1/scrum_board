using Domain.Enums;

namespace Application.Contracts.Boards;

public sealed record CreateBoardTaskRequest(
    string Title,
    string Description,
    WorkItemPriority Priority,
    Guid? AssignedUserId,
    Guid ColumnId);