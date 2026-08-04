using Domain.Enums;

namespace Application.Contracts.Projects;

public sealed record ProjectDto(
    Guid Id,
    string Name,
    string? Description,
    DateOnly StartDate,
    DateOnly ExpectedEndDate,
    ProjectStatus Status);