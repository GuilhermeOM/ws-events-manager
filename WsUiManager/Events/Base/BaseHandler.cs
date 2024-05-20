namespace WsUiManager.Events.Base;

using System.Reflection;
using System.Text.Json;
using Fleck;

// @Warning - Tudo que herdar BaseHandler torna-se um Singleton na aplicação.
public abstract class BaseHandler<T> where T : BaseEvent
{
    public string EventType => this.GetType().Name;

    public abstract Task Handle(T eventType, IWebSocketConnection socket);

    public async Task InvokeHandle(string message, IWebSocketConnection socket)
    {
        var @event = JsonSerializer.Deserialize<T>(message, SerializePropertyInCaseInsensitive)
            ?? throw new ArgumentException($"Não foi possível deserializar em {typeof(T).Name} a partir da string: {message}");

        foreach (var baseEventFilterAttribute in this.GetType().GetCustomAttributes().OfType<BaseFilterAttribute>())
        {
            await baseEventFilterAttribute.Handle(socket, @event);
        }

        await this.Handle(@event, socket);
    }

    private static readonly JsonSerializerOptions SerializePropertyInCaseInsensitive = new()
    {
        PropertyNameCaseInsensitive = true
    };
}
