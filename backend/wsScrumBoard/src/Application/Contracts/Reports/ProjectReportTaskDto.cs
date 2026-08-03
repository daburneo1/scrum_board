using Domain.Enums;

namespace Application.Contracts.Reports;

public sealed record ProjectReportTaskDto(
    Guid TaskId,
    string Title,
    string Description,
    string ColumnName,
    int ColumnOrder,
    string? ResponsibleName,
    string? ResponsibleEmail,
    WorkItemPriority Priority,
    int TaskOrder,
    DateTimeOffset CreatedAtUtc);