using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Application.RealTime.Boards;
using Application.Services.Boards;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace WebApi.RealTime;

[Authorize]
public sealed class BoardHub : Hub<IBoardClient>
{
    private readonly BoardSubscriptionService
        _subscriptionService;

    private readonly IBoardPresenceTracker
        _presenceTracker;

    public BoardHub(
        BoardSubscriptionService subscriptionService,
        IBoardPresenceTracker presenceTracker)
    {
        _subscriptionService = subscriptionService;
        _presenceTracker = presenceTracker;
    }

    public async Task JoinBoard(Guid projectId)
    {
        await _subscriptionService
            .EnsureProjectExistsAsync(
                projectId,
                Context.ConnectionAborted);

        await Groups.AddToGroupAsync(
            Context.ConnectionId,
            BoardGroupName.FromProjectId(projectId),
            Context.ConnectionAborted);

        var (userId, displayName) =
            GetCurrentUserIdentity();

        var snapshot = _presenceTracker.Join(
            projectId,
            userId,
            displayName,
            Context.ConnectionId);

        await Clients
            .Group(BoardGroupName.FromProjectId(projectId))
            .BoardPresenceChanged(snapshot);
    }

    public async Task LeaveBoard(Guid projectId)
    {
        if (projectId == Guid.Empty)
        {
            return;
        }

        var snapshot = _presenceTracker.Leave(
            projectId,
            Context.ConnectionId);

        await Groups.RemoveFromGroupAsync(
            Context.ConnectionId,
            BoardGroupName.FromProjectId(projectId));

        await Clients
            .Group(BoardGroupName.FromProjectId(projectId))
            .BoardPresenceChanged(snapshot);
    }

    public override async Task OnDisconnectedAsync(
        Exception? exception)
    {
        var snapshots = _presenceTracker.RemoveConnection(
            Context.ConnectionId);

        foreach (var snapshot in snapshots)
        {
            await Clients
                .Group(BoardGroupName.FromProjectId(snapshot.ProjectId))
                .BoardPresenceChanged(snapshot);
        }

        await base.OnDisconnectedAsync(exception);
    }

    private (Guid UserId, string DisplayName) GetCurrentUserIdentity()
    {
        var userIdClaim = Context.User?.FindFirstValue(
                ClaimTypes.NameIdentifier)
            ?? Context.User?.FindFirstValue(
                JwtRegisteredClaimNames.Sub)
            ?? Context.User?.FindFirstValue("sub");

        if (!Guid.TryParse(userIdClaim, out var userId))
        {
            throw new HubException(
                "No se pudo resolver el identificador del usuario autenticado.");
        }

        var displayName = Context.User?.FindFirstValue(
                ClaimTypes.Name)
            ?? Context.User?.FindFirstValue(
                JwtRegisteredClaimNames.Name)
            ?? Context.User?.FindFirstValue(
                ClaimTypes.Email)
            ?? Context.User?.FindFirstValue(
                JwtRegisteredClaimNames.Email)
            ?? Context.User?.FindFirstValue("email")
            ?? "Usuario conectado";

        return (userId, displayName);
    }
}
