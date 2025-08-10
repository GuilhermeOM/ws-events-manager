using System.Text.Encodings.Web;
using System.Text.Json;
using Fleck;
using WsUiManager.Entities;
using WsUiManager.Entities.Enums;
using WsUiManager.Entities.Feedback;
using WsUiManager.Events.Base;
using WsUiManager.Events.Exceptions;

namespace WsUiManager.Events;
public class BroadCastToRoomEvent : BaseEvent
{
    private string _roomName = "";

    public required string Message { get; set; }
    public required string RoomName
    {
        get => _roomName;
        set => _roomName = Enum.GetNames(typeof(Room))
            .Where(room => room.Equals(value, StringComparison.OrdinalIgnoreCase))
            .FirstOrDefault(string.Empty);
    }
}

public class BroadCastToRoomWithUsername
{
    public required string Message { get; set; }
    public required string From { get; set; }
}

public class BroadCastToRoom : BaseHandler<BroadCastToRoomEvent>
{
    private static readonly JsonSerializerOptions _jsonSerializerForMessage = new()
    {
        WriteIndented = true,
        Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
    };

    public override async Task Handle(BroadCastToRoomEvent eventType, IWebSocketConnection socket)
    {
        if (string.IsNullOrEmpty(eventType.RoomName))
        {
            throw new RoomNotExistsException();
        }

        var message = new BroadCastToRoomWithUsername()
        {
            Message = eventType.Message,
            From = StateService.Connections[socket.ConnectionInfo.Id].Username
        };

        var roomAsEnum = Enum.Parse<Room>(eventType.RoomName);

        await StateService.BroadcastToRoom((int)roomAsEnum, JsonSerializer.Serialize(message, _jsonSerializerForMessage));
        await socket.Send(new Message<BroadCastToRoomMessage>()
        {
            ConnectionId = socket.ConnectionInfo.Id,
            Name = "BROADCASTTOROOM_FEEDBACK",
            Data = new BroadCastToRoomMessage()
            {
                Feedback = "Mensagem enviada com sucesso!",
            },
        }.AsJson());
    }
}
