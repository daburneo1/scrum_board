using Domain.Enums;

namespace Application.Contracts.Projects;

public sealed record CreateProjectRequest(
    string Name,
    string? Description,
    DateOnly StartDate,
    DateOnly ExpectedEndDate,
    ProjectStatus Status);
