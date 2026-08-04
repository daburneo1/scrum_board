using Application.Services.Boards;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace WebApi.RealTime;

[Authorize]
public sealed class BoardHub : Hub<IBoardClient>
{
    private readonly BoardSubscriptionService
        _subscriptionService;

    public BoardHub(
        BoardSubscriptionService subscriptionService)
    {
        _subscriptionService = subscriptionService;
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
    }

    public Task LeaveBoard(Guid projectId)
    {
        if (projectId == Guid.Empty)
        {
            return Task.CompletedTask;
        }

        return Groups.RemoveFromGroupAsync(
            Context.ConnectionId,
            BoardGroupName.FromProjectId(projectId));
    }
}