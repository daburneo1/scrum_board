using Application.Services.Reports;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace WebApi.Controllers;

[ApiController]
[Authorize]
[Route(
    "api/projects/{projectId:guid}/reports")]
public sealed class ProjectReportsController
    : ControllerBase
{
    private readonly ProjectReportService
        _reportService;

    public ProjectReportsController(
        ProjectReportService reportService)
    {
        _reportService = reportService;
    }

    [HttpGet("{format}")]
    public async Task<IActionResult> Download(
        Guid projectId,
        string format,
        [FromQuery] ProjectTaskFilterQuery query,
        CancellationToken cancellationToken)
    {
        var report =
            await _reportService.GenerateAsync(
                projectId,
                format,
                query.ToFilter(),
                cancellationToken);

        return File(
            report.Content,
            report.ContentType,
            report.FileName);
    }
}
