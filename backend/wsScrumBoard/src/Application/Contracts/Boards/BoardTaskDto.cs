using Domain.Enums;

namespace Application.Contracts.Boards;

public sealed record BoardTaskDto(
    Guid Id,
    string Title,
    string Description,
    WorkItemPriority Priority,
    Guid? AssignedUserId,
    string? AssignedUserName,
    Guid ColumnId,
    int SortOrder,
    DateTimeOffset CreatedAtUtc);