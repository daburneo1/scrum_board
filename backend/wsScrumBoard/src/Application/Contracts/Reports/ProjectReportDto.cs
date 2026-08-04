using Domain.Enums;

namespace Application.Contracts.Reports;

public sealed record ProjectReportDto(
    Guid ProjectId,
    string ProjectName,
    string Description,
    DateOnly StartDate,
    DateOnly ExpectedEndDate,
    ProjectStatus Status,
    DateTimeOffset GeneratedAtUtc,
    ProjectReportAppliedFiltersDto AppliedFilters,
    IReadOnlyCollection<ProjectReportTaskDto> Tasks);
