namespace Application.Contracts.Boards;

public sealed record ReorderColumnsRequest(
    IReadOnlyCollection<Guid> OrderedColumnIds);