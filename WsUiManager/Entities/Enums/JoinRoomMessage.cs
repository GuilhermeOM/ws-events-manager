using WsUiManager.Entities.Feedback;

namespace WsUiManager.Entities.Enums;
public class JoinRoomMessage : FeedbackMessage
{
    public required string RoomName { get; set; }
}
