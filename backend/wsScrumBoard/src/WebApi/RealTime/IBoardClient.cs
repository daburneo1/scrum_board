using Application.RealTime.Boards;

namespace WebApi.RealTime;

public interface IBoardClient
{
    Task BoardChanged(
        BoardChangedNotification notification);

    Task BoardPresenceChanged(
        BoardPresenceSnapshot snapshot);
}
