namespace Application.Contracts.Boards;

public sealed record MoveTaskRequest(
    Guid TargetColumnId,
    int TargetIndex);