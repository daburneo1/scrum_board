using Application.Common.Models;
using Application.Contracts.Projects;
using Application.Services.Projects;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace WebApi.Controllers;

[ApiController]
[Authorize]
[Route("api/projects")]
public sealed class ProjectsController(ProjectService projectService) : ControllerBase
{
    [HttpGet]
    public Task<PagedResult<ProjectDto>> GetPaged(
        [FromQuery] ProjectQueryParameters parameters,
        CancellationToken cancellationToken)
    {
        return projectService.GetPagedAsync(
            parameters,
            cancellationToken);
    }

    [HttpGet("{id:guid}")]
    public Task<ProjectDto> GetById(
        Guid id,
        CancellationToken cancellationToken)
    {
        return projectService.GetByIdAsync(
            id,
            cancellationToken);
    }

    [HttpPost]
    public async Task<ActionResult<ProjectDto>> Create(
        CreateProjectRequest request,
        CancellationToken cancellationToken)
    {
        var project = await projectService.CreateAsync(
            request,
            cancellationToken);

        return CreatedAtAction(
            nameof(GetById),
            new { id = project.Id },
            project);
    }

    [HttpPut("{id:guid}")]
    public Task<ProjectDto> Update(
        Guid id,
        UpdateProjectRequest request,
        CancellationToken cancellationToken)
    {
        return projectService.UpdateAsync(
            id,
            request,
            cancellationToken);
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(
        Guid id,
        CancellationToken cancellationToken)
    {
        await projectService.DeleteAsync(
            id,
            cancellationToken);

        return NoContent();
    }
}