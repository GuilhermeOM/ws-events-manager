namespace WsUiManager.Events;

using Fleck;
using Serilog;
using WsUiManager.Entities;
using WsUiManager.Entities.Enums;
using WsUiManager.Entities.Feedback;
using WsUiManager.Events.Base;
using WsUiManager.Events.Exceptions;

public class LeaveRoomEvent : BaseEvent
{
    private string roomName = "";

    public required string RoomName
    {
        get => this.roomName;
        set => this.roomName = Enum.GetNames(typeof(Room))
            .Where(room => room.Equals(value, StringComparison.OrdinalIgnoreCase))
            .FirstOrDefault("");
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

        Log.Information("{@Id} - Cliente removido da sala {@Room} com sucesso!",
            socket.ConnectionInfo.Id, eventType.RoomName);

        await socket.Send(new Message<LeaveRoomMessage>()
        {
            ConnectionId = socket.ConnectionInfo.Id,
            Name = "LEAVEROOM_FEEDBACK",
            Data = new LeaveRoomMessage()
            {
                Feedback = "Removido da sala com sucesso!",
                RoomName = eventType.RoomName
            },
        }.AsJson());
    }
}
