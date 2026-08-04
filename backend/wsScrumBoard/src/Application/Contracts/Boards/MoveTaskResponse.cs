namespace Application.Contracts.Boards;

public sealed record MoveTaskResponse(
    Guid TaskId,
    Guid SourceColumnId,
    Guid TargetColumnId,
    IReadOnlyCollection<BoardColumnDto> AffectedColumns);