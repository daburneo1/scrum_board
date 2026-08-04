namespace Application.RealTime.Boards;

public sealed record BoardPresenceSnapshot(
    Guid ProjectId,
    int ConnectedUserCount,
    IReadOnlyCollection<BoardPresenceUser> Users,
    DateTimeOffset OccurredAtUtc);
