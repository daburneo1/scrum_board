using Application.Contracts.Reports;

using Application.Contracts.Tasks;

namespace Application.Ports.Reports;

public interface IProjectReportRepository
{
    Task<ProjectReportDto?> GetAsync(
        Guid projectId,
        DateTimeOffset generatedAtUtc,
        ProjectTaskFilter filter,
        CancellationToken cancellationToken = default);
}
