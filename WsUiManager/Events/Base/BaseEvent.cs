namespace WsUiManager.Events.Base;

public class BaseEvent
{
    public BaseEvent()
    {
        var eventType = this.GetType().Name;
        var subString = eventType[^5..];

        if (subString.ToLowerInvariant().Equals("event", StringComparison.Ordinal))
        {
            this.EventType = eventType[..^5];
        }
        else
        {
            this.EventType = eventType;
        }
    }

    public string EventType { get; set; }
}
