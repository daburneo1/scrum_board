using Domain.Enums;

namespace Application.Contracts.Boards;

public sealed record UpdateBoardTaskRequest(
    string Title,
    string Description,
    WorkItemPriority Priority,
    Guid? AssignedUserId);