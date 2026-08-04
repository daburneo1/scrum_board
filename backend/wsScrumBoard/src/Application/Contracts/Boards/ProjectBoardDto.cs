namespace Application.Contracts.Boards;

public sealed record ProjectBoardDto(
    Guid ProjectId,
    string ProjectName,
    IReadOnlyCollection<BoardColumnDto> Columns);