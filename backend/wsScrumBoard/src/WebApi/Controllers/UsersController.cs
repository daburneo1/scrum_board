using Application.Contracts.Boards;
using Application.Services.Boards;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace WebApi.Controllers;

[ApiController]
[Authorize]
[Route("api/users")]
public sealed class UsersController : ControllerBase
{
    private readonly BoardService _boardService;

    public UsersController(BoardService boardService)
    {
        _boardService = boardService;
    }

    [HttpGet]
    public Task<IReadOnlyCollection<UserOptionDto>> Get(
        CancellationToken cancellationToken)
    {
        return _boardService.GetUsersAsync(
            cancellationToken);
    }
}