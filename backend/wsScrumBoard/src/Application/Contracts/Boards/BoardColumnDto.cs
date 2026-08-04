namespace Application.Contracts.Boards;

public sealed record BoardColumnDto(
    Guid Id,
    string Name,
    int SortOrder,
    IReadOnlyCollection<BoardTaskDto> Tasks);