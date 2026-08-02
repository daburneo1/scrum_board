namespace Application.Tasks.Ordering;

public sealed record TaskMovePlan(
    IReadOnlyCollection<TaskPositionChange> Changes);