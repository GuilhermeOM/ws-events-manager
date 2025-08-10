using Fleck;
using Serilog;
using Ws.Events.Manager.Root.Entities;
using Ws.Events.Manager.Root.Entities.Enums;
using Ws.Events.Manager.Root.Entities.Feedback;
using Ws.Events.Manager.Root.Events.Base;
using Ws.Events.Manager.Root.Events.Exceptions;

namespace Ws.Events.Manager.Root.Events;
public class LeaveRoomEvent : BaseEvent
{
    private string _roomName = "";

    public required string RoomName
    {
        get => _roomName;
        set => _roomName = Enum.GetNames(typeof(Room))
            .Where(room => room.Equals(value, StringComparison.OrdinalIgnoreCase))
            .FirstOrDefault(string.Empty);
    }
}

public class LeaveRoom : BaseHandler<LeaveRoomEvent>
{
    public override async Task Handle(LeaveRoomEvent eventType, IWebSocketConnection socket)
    {
        if (string.IsNullOrEmpty(eventType.RoomName))
        {
            throw new RoomNotExistsException();
        }

        var roomAsEnum = Enum.Parse<Room>(eventType.RoomName);
        var didRemove = StateService.RemoveFromRoomById(socket, (int)roomAsEnum);

        if (!didRemove)
        {
            throw new EventFailedException();
        }

        Log.Information("{Id} - Client successfully removed from the room {Room}!", socket.ConnectionInfo.Id, eventType.RoomName);

        await socket.Send(new Message<LeaveRoomMessage>()
        {
            ConnectionId = socket.ConnectionInfo.Id,
            Name = "LEAVEROOM_FEEDBACK",
            Data = new LeaveRoomMessage()
            {
                Feedback = "Successfully removed from the room!",
                RoomName = eventType.RoomName
            },
        }.AsJson());
    }
}
