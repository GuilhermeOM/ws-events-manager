using System.Reflection;
using System.Text.Json;
using Fleck;

namespace WsUiManager.Events.Base;
public abstract class BaseHandler<T> where T : BaseEvent
{
    private static readonly JsonSerializerOptions _serializePropertyInCaseInsensitive = new()
    {
        PropertyNameCaseInsensitive = true
    };

    public string EventType => GetType().Name;

    public abstract Task Handle(T eventType, IWebSocketConnection socket);

    public async Task InvokeHandle(string message, IWebSocketConnection socket)
    {
        var @event = JsonSerializer.Deserialize<T>(message, _serializePropertyInCaseInsensitive)
            ?? throw new ArgumentException($"Não foi possível deserializar em {typeof(T).Name} a partir da string: {message}");

        foreach (var baseEventFilterAttribute in GetType().GetCustomAttributes().OfType<BaseFilterAttribute>())
        {
            await baseEventFilterAttribute.Handle(socket, @event);
        }

        await Handle(@event, socket);
    }
}
