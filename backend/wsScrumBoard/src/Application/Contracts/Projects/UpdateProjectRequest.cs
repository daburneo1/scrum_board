using Domain.Enums;

namespace Application.Contracts.Projects;

public sealed record UpdateProjectRequest(
    string Name,
    string? Description,
    DateOnly StartDate,
    DateOnly ExpectedEndDate,
    ProjectStatus Status);