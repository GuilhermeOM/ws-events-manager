namespace WsUiManager.Events.Base;
public class BaseEvent
{
    public BaseEvent()
    {
        var eventType = GetType().Name;
        var subString = eventType[^5..];

        if (subString.ToLowerInvariant().Equals("event", StringComparison.Ordinal))
        {
            EventType = eventType[..^5];
        }
        else
        {
            EventType = eventType;
        }
    }

    public string EventType { get; set; }
}
