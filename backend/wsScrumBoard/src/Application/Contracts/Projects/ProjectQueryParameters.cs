namespace Application.Contracts.Projects;

public sealed record ProjectQueryParameters(
    int PageNumber = 1,
    int PageSize = 10,
    string? Name = null);