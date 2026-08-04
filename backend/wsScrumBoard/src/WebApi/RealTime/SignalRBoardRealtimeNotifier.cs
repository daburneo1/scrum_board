using Application.RealTime.Boards;
using Microsoft.AspNetCore.SignalR;

namespace WebApi.RealTime;

internal sealed class SignalRBoardRealtimeNotifier(
    IHubContext<BoardHub, IBoardClient> hubContext,
    ILogger<SignalRBoardRealtimeNotifier> logger)
    : IBoardRealtimeNotifier
{
    public async Task NotifyBoardChangedAsync(
        BoardChangedNotification notification)
    {
        try
        {
            var groupName =
                BoardGroupName.FromProjectId(
                    notification.ProjectId);

            await hubContext
                .Clients
                .Group(groupName)
                .BoardChanged(notification);
        }
        catch (Exception exception)
        {
            /*
             * El cambio ya fue persistido.
             */
            logger.LogError(
                exception,
                "No se pudo publicar el evento {EventId} " +
                "para el proyecto {ProjectId}.",
                notification.EventId,
                notification.ProjectId);
        }
    }
}