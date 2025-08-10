using Fleck;

namespace Ws.Events.Manager.Root.Events.Base;

[AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
public abstract class BaseFilterAttribute : Attribute
{
    public abstract Task Handle<T>(IWebSocketConnection socket, T eventType) where T : BaseEvent;
}
