using Ws.Events.Manager.Root.Entities.Feedback;

namespace Ws.Events.Manager.Root.Entities.Enums;
public class JoinRoomMessage : FeedbackMessage
{
    public required string RoomName { get; set; }
}
