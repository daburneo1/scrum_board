namespace Application.RealTime.Boards;

public sealed record BoardChangedNotification(
    Guid EventId,
    Guid ProjectId,
    BoardChangeType ChangeType,
    Guid TaskId,
    DateTimeOffset OccurredAtUtc);