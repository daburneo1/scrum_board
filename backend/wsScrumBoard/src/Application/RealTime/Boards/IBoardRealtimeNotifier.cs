namespace Application.RealTime.Boards;

public interface IBoardRealtimeNotifier
{
    Task NotifyBoardChangedAsync(
        BoardChangedNotification notification);
}