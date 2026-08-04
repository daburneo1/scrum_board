namespace Application.Tasks.Ordering;

public sealed record TaskPositionChange(
    Guid TaskId,
    Guid ColumnId,
    int SortOrder);