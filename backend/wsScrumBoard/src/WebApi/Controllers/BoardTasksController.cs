using Application.Contracts.Boards;
using Application.Services.Boards;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace WebApi.Controllers;

[ApiController]
[Authorize]
[Route("api/projects/{projectId:guid}/tasks")]
public sealed class BoardTasksController : ControllerBase
{
    private readonly BoardService _boardService;

    public BoardTasksController(BoardService boardService)
    {
        _boardService = boardService;
    }

    [HttpPost]
    public Task<BoardTaskDto> Create(
        Guid projectId,
        CreateBoardTaskRequest request,
        CancellationToken cancellationToken)
    {
        return _boardService.CreateTaskAsync(
            projectId,
            request,
            cancellationToken);
    }

    [HttpPut("{taskId:guid}")]
    public Task<BoardTaskDto> Update(
        Guid projectId,
        Guid taskId,
        UpdateBoardTaskRequest request,
        CancellationToken cancellationToken)
    {
        return _boardService.UpdateTaskAsync(
            projectId,
            taskId,
            request,
            cancellationToken);
    }

    [HttpDelete("{taskId:guid}")]
    public async Task<IActionResult> Delete(
        Guid projectId,
        Guid taskId,
        CancellationToken cancellationToken)
    {
        await _boardService.DeleteTaskAsync(
            projectId,
            taskId,
            cancellationToken);

        return NoContent();
    }
}