using Application.Contracts.Reports;

namespace Application.Ports.Reports;

public interface IProjectReportRepository
{
    Task<ProjectReportDto?> GetAsync(
        Guid projectId,
        DateTimeOffset generatedAtUtc,
        CancellationToken cancellationToken = default);
}