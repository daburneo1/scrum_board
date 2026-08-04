using System.Text.Json.Serialization;

namespace Application.RealTime.Boards;

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum BoardChangeType
{
    TaskCreated,
    TaskUpdated,
    TaskDeleted,
    TaskMoved
}