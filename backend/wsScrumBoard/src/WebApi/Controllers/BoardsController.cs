using Application.Contracts.Boards;
using Application.Services.Boards;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace WebApi.Controllers;

[ApiController]
[Authorize]
[Route("api/projects/{projectId:guid}/board")]
public sealed class BoardsController : ControllerBase
{
    private readonly BoardService _boardService;

    public BoardsController(BoardService boardService)
    {
        _boardService = boardService;
    }

    [HttpGet]
    public Task<ProjectBoardDto> Get(
        Guid projectId,
        [FromQuery] ProjectTaskFilterQuery query,
        CancellationToken cancellationToken)
    {
        return _boardService.GetBoardAsync(
            projectId,
            query.ToFilter(),
            cancellationToken);
    }
}
