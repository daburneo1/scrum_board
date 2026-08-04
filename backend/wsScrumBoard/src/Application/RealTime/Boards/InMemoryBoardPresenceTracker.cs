namespace Application.RealTime.Boards;

public sealed class InMemoryBoardPresenceTracker : IBoardPresenceTracker
{
    private readonly object _syncRoot = new();

    private readonly Dictionary<Guid, ProjectPresence> _projects = new();

    private readonly Dictionary<string, HashSet<Guid>> _connectionProjects =
        new(StringComparer.Ordinal);

    public BoardPresenceSnapshot Join(
        Guid projectId,
        Guid userId,
        string displayName,
        string connectionId)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(connectionId);

        if (projectId == Guid.Empty)
        {
            throw new ArgumentException(
                "El identificador del proyecto es obligatorio.",
                nameof(projectId));
        }

        if (userId == Guid.Empty)
        {
            throw new ArgumentException(
                "El identificador del usuario es obligatorio.",
                nameof(userId));
        }

        lock (_syncRoot)
        {
            if (!_projects.TryGetValue(projectId, out var projectPresence))
            {
                projectPresence = new ProjectPresence();
                _projects.Add(projectId, projectPresence);
            }

            if (!projectPresence.Users.TryGetValue(userId, out var userPresence))
            {
                userPresence = new UserPresence(
                    NormalizeDisplayName(displayName, userId));

                projectPresence.Users.Add(userId, userPresence);
            }
            else
            {
                userPresence.DisplayName =
                    NormalizeDisplayName(displayName, userId);
            }

            userPresence.ConnectionIds.Add(connectionId);

            if (!_connectionProjects.TryGetValue(connectionId, out var projects))
            {
                projects = [];
                _connectionProjects.Add(connectionId, projects);
            }

            projects.Add(projectId);

            return CreateSnapshotUnlocked(projectId);
        }
    }

    public BoardPresenceSnapshot Leave(
        Guid projectId,
        string connectionId)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(connectionId);

        lock (_syncRoot)
        {
            RemoveConnectionFromProjectUnlocked(
                projectId,
                connectionId);

            return CreateSnapshotUnlocked(projectId);
        }
    }

    public IReadOnlyCollection<BoardPresenceSnapshot> RemoveConnection(
        string connectionId)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(connectionId);

        lock (_syncRoot)
        {
            if (!_connectionProjects.TryGetValue(
                    connectionId,
                    out var projectIds))
            {
                return [];
            }

            var affectedProjectIds = projectIds.ToArray();

            foreach (var projectId in affectedProjectIds)
            {
                RemoveConnectionFromProjectUnlocked(
                    projectId,
                    connectionId);
            }

            return affectedProjectIds
                .Select(CreateSnapshotUnlocked)
                .ToArray();
        }
    }

    public BoardPresenceSnapshot GetSnapshot(
        Guid projectId)
    {
        lock (_syncRoot)
        {
            return CreateSnapshotUnlocked(projectId);
        }
    }

    private void RemoveConnectionFromProjectUnlocked(
        Guid projectId,
        string connectionId)
    {
        if (_connectionProjects.TryGetValue(
                connectionId,
                out var projectIds))
        {
            projectIds.Remove(projectId);

            if (projectIds.Count == 0)
            {
                _connectionProjects.Remove(connectionId);
            }
        }

        if (!_projects.TryGetValue(projectId, out var projectPresence))
        {
            return;
        }

        var emptyUserIds = new List<Guid>();

        foreach (var (userId, userPresence) in projectPresence.Users)
        {
            userPresence.ConnectionIds.Remove(connectionId);

            if (userPresence.ConnectionIds.Count == 0)
            {
                emptyUserIds.Add(userId);
            }
        }

        foreach (var userId in emptyUserIds)
        {
            projectPresence.Users.Remove(userId);
        }

        if (projectPresence.Users.Count == 0)
        {
            _projects.Remove(projectId);
        }
    }

    private BoardPresenceSnapshot CreateSnapshotUnlocked(
        Guid projectId)
    {
        var users =
            _projects.TryGetValue(projectId, out var projectPresence)
                ? projectPresence.Users
                    .Select(pair =>
                        new BoardPresenceUser(
                            pair.Key,
                            pair.Value.DisplayName))
                    .OrderBy(user => user.DisplayName)
                    .ThenBy(user => user.UserId)
                    .ToArray()
                : [];

        return new BoardPresenceSnapshot(
            projectId,
            users.Length,
            users,
            DateTimeOffset.UtcNow);
    }

    private static string NormalizeDisplayName(
        string displayName,
        Guid userId)
    {
        var normalized = displayName.Trim();

        return string.IsNullOrWhiteSpace(normalized)
            ? $"Usuario {userId:N}"[..16]
            : normalized;
    }

    private sealed class ProjectPresence
    {
        public Dictionary<Guid, UserPresence> Users { get; } = new();
    }

    private sealed class UserPresence(string displayName)
    {
        public string DisplayName { get; set; } = displayName;

        public HashSet<string> ConnectionIds { get; } =
            new(StringComparer.Ordinal);
    }
}
