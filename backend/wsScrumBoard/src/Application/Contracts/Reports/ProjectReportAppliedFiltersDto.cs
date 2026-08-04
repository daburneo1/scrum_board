namespace Application.Contracts.Reports;

public sealed record ProjectReportAppliedFiltersDto(
    string Assignee,
    string Priority,
    string Search,
    bool HasActiveFilters);
