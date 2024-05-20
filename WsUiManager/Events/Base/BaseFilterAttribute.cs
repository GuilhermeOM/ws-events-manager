namespace WsUiManager.Events.Base;

using Fleck;

[AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
public abstract class BaseFilterAttribute : Attribute
{
    public abstract Task Handle<T>(IWebSocketConnection socket, T eventType) where T : BaseEvent;
}
