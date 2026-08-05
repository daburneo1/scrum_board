using Application.Contracts.Boards;
using Application.Services.Boards;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace WebApi.Controllers;

[ApiController]
[Authorize]
[Route("api/projects/{projectId:guid}/tasks")]
public sealed class BoardTasksController(BoardService boardService) : ControllerBase
{
    [HttpGet]
    public Task<IReadOnlyCollection<BoardTaskDto>> GetAll(
        Guid projectId,
        CancellationToken cancellationToken)
    {
        return boardService.GetTasksAsync(
            projectId,
            cancellationToken);
    }

    [HttpGet("{taskId:guid}")]
    public Task<BoardTaskDto> GetById(
        Guid projectId,
        Guid taskId,
        CancellationToken cancellationToken)
    {
        return boardService.GetTaskAsync(
            projectId,
            taskId,
            cancellationToken);
    }

    [HttpPost]
    public Task<BoardTaskDto> Create(
        Guid projectId,
        CreateBoardTaskRequest request,
        CancellationToken cancellationToken)
    {
        return boardService.CreateTaskAsync(
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
        return boardService.UpdateTaskAsync(
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
        await boardService.DeleteTaskAsync(
            projectId,
            taskId,
            cancellationToken);

        return NoContent();
    }
    
    [HttpPut("{taskId:guid}/position")]
    public Task<MoveTaskResponse> Move(
        Guid projectId,
        Guid taskId,
        MoveTaskRequest request,
        CancellationToken cancellationToken)
    {
        return boardService.MoveTaskAsync(
            projectId,
            taskId,
            request,
            cancellationToken);
    }
}
