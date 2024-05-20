namespace WsUiManager.Events;

using System.Text.Encodings.Web;
using System.Text.Json;
using System.Threading.Tasks;
using Fleck;
using WsUiManager.Entities;
using WsUiManager.Entities.Enums;
using WsUiManager.Entities.Feedback;
using WsUiManager.Events.Base;
using WsUiManager.Events.Exceptions;

public class BroadCastToRoomEvent : BaseEvent
{
    private string roomName = "";

    public required string Message { get; set; }
    public required string RoomName
    {
        get => this.roomName;
        set => this.roomName = Enum.GetNames(typeof(Room))
            .Where(room => room.Equals(value, StringComparison.OrdinalIgnoreCase))
            .FirstOrDefault("");
    }
}

public class BroadCastToRoomWithUsername
{
    public required string Message { get; set; }
    public required string From { get; set; }
}

public class BroadCastToRoom : BaseHandler<BroadCastToRoomEvent>
{
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

        await StateService.BroadcastToRoom((int)roomAsEnum, JsonSerializer.Serialize(message, JsonSerializerForMessage));
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

    private static readonly JsonSerializerOptions JsonSerializerForMessage = new()
    {
        WriteIndented = true,
        Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
    };
}
