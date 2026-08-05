using Application.Contracts.Boards;
using Application.Services.Boards;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace WebApi.Controllers;

[ApiController]
[Authorize]
[Route("api/projects/{projectId:guid}/columns")]
public sealed class ColumnsController : ControllerBase
{
    private readonly BoardService _boardService;

    public ColumnsController(BoardService boardService)
    {
        _boardService = boardService;
    }

    [HttpGet]
    public Task<IReadOnlyCollection<BoardColumnDto>> GetAll(
        Guid projectId,
        CancellationToken cancellationToken)
    {
        return _boardService.GetColumnsAsync(
            projectId,
            cancellationToken);
    }

    [HttpGet("{columnId:guid}")]
    public Task<BoardColumnDto> GetById(
        Guid projectId,
        Guid columnId,
        CancellationToken cancellationToken)
    {
        return _boardService.GetColumnAsync(
            projectId,
            columnId,
            cancellationToken);
    }

    [HttpPost]
    public async Task<ActionResult<BoardColumnDto>> Create(
        Guid projectId,
        CreateColumnRequest request,
        CancellationToken cancellationToken)
    {
        var column = await _boardService.CreateColumnAsync(
            projectId,
            request,
            cancellationToken);

        return Created(
            $"/api/projects/{projectId}/columns/{column.Id}",
            column);
    }

    [HttpPut("{columnId:guid}")]
    public Task<BoardColumnDto> Update(
        Guid projectId,
        Guid columnId,
        UpdateColumnRequest request,
        CancellationToken cancellationToken)
    {
        return _boardService.UpdateColumnAsync(
            projectId,
            columnId,
            request,
            cancellationToken);
    }

    [HttpDelete("{columnId:guid}")]
    public async Task<IActionResult> Delete(
        Guid projectId,
        Guid columnId,
        CancellationToken cancellationToken)
    {
        await _boardService.DeleteColumnAsync(
            projectId,
            columnId,
            cancellationToken);

        return NoContent();
    }

    [HttpPut("order")]
    public async Task<IActionResult> Reorder(
        Guid projectId,
        ReorderColumnsRequest request,
        CancellationToken cancellationToken)
    {
        await _boardService.ReorderColumnsAsync(
            projectId,
            request,
            cancellationToken);

        return NoContent();
    }
}
