namespace Application.RealTime.Boards;

public interface IBoardPresenceTracker
{
    BoardPresenceSnapshot Join(
        Guid projectId,
        Guid userId,
        string displayName,
        string connectionId);

    BoardPresenceSnapshot Leave(
        Guid projectId,
        string connectionId);

    IReadOnlyCollection<BoardPresenceSnapshot> RemoveConnection(
        string connectionId);

    BoardPresenceSnapshot GetSnapshot(
        Guid projectId);
}
