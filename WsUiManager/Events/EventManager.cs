using Fleck;
using System.Reflection;
using System.Text.Json;
using WsUiManager.Events.Base;

namespace WsUiManager.Events;
public static class EventManager
{
    private static readonly JsonSerializerOptions _serializePropertyInCaseInsensitive = new()
    {
        PropertyNameCaseInsensitive = true
    };

    public static HashSet<Type> FindAndInjectClientEventHandlers(this WebApplicationBuilder builder,
        Assembly assemblyReference)
    {
        var clientEventHandlers = new HashSet<Type>();

        foreach (var type in assemblyReference.GetTypes())
        {
            if (type.BaseType != null &&
                type.BaseType.IsGenericType &&
                type.BaseType.GetGenericTypeDefinition() == typeof(BaseHandler<>))
            {
                _ = builder.Services.AddSingleton(type);
                _ = clientEventHandlers.Add(type);
            }
        }

        return clientEventHandlers;
    }

    public static async Task InvokeClientEventHandler(this WebApplication app, HashSet<Type> types,
        IWebSocketConnection ws, string message)
    {
        var @event = JsonSerializer.Deserialize<BaseEvent>(message, _serializePropertyInCaseInsensitive)
            ?? throw new ArgumentException($"Não foi possível deserializar string: {message} para {nameof(BaseEvent)}");

        var eventType = @event.EventType.EndsWith("Event", StringComparison.OrdinalIgnoreCase)
            ? @event.EventType[..^5]
            : @event.EventType;

        var handlerType = types.FirstOrDefault(type => type.Name.Equals(eventType, StringComparison.OrdinalIgnoreCase) ||
                                                       type.Name.Equals(eventType + "Event",
                                                            StringComparison.OrdinalIgnoreCase));

        if (handlerType == null)
        {
            var eventTypeName = @event.GetType().Name;

            handlerType = types.FirstOrDefault(type =>
                type.BaseType != null &&
                type.BaseType.IsGenericType &&
                type.BaseType.GetGenericTypeDefinition() == typeof(BaseHandler<>) &&
                (type.BaseType.GetGenericArguments()[0].Name.Equals(eventType, StringComparison.OrdinalIgnoreCase) ||
                 type.BaseType.GetGenericArguments()[0].Name
                     .Equals(eventType + "Event", StringComparison.OrdinalIgnoreCase)));
        }

        if (handlerType == null)
        {
            throw new InvalidOperationException($"Não foi possível encontrar o handler para o evento do tipo: {@event.EventType}");
        }

        dynamic clientEventServiceClass = app.Services.GetService(handlerType)!
            ?? throw new InvalidOperationException($"Não foi possível resolver o serviço do evento para o tipo: {handlerType}");

        await clientEventServiceClass.InvokeHandle(message, ws);
    }
}
