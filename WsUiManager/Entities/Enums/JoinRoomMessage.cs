namespace WsUiManager.Entities.Enums;

using WsUiManager.Entities.Feedback;

public class JoinRoomMessage : FeedbackMessage
{
    public required string RoomName { get; set; }
}
