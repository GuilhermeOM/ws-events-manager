namespace Ws.Events.Manager.Root.Entities.Feedback;
public class LeaveRoomMessage : FeedbackMessage
{
    public required string RoomName { get; set; }
}
