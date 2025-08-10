using Fleck;
using Serilog;
using Ws.Events.Manager.Root.Entities.Enums;
using Ws.Events.Manager.Root.Events.Exceptions;

namespace Ws.Events.Manager.Root;
public class WebSocketMetaData(IWebSocketConnection connection)
{
    public IWebSocketConnection Connection { get; set; } = connection;
    public required string Username { get; set; }
}

public static class StateService
{
    public static readonly Dictionary<Guid, WebSocketMetaData> Connections = [];
    public static readonly Dictionary<int, HashSet<Guid>> Rooms = [];

    public static void AddConnection(IWebSocketConnection ws)
    {
        var websocketMetadata = new WebSocketMetaData(ws) { Username = "Anonymous" };
        var didAdd = Connections.TryAdd(ws.ConnectionInfo.Id, websocketMetadata);

        var status = didAdd
            ? "{Id} - Nova conexão adicionada com sucesso!"
            : "{Id} - Não foi possível adicionar a nova conexão.";

        Log.Write(
            level: didAdd
                ? Serilog.Events.LogEventLevel.Information
                : Serilog.Events.LogEventLevel.Error,
            messageTemplate: status,
            ws.ConnectionInfo.Id);

        Log.Debug("Id da conexão adicionada: {Id}, Info da conexão adicionada: {@ConnectionInfo}.",
            ws.ConnectionInfo.Id, websocketMetadata.Connection.ConnectionInfo);
    }

    public static void RemoveConnection(IWebSocketConnection ws)
    {
        _ = Connections.TryGetValue(ws.ConnectionInfo.Id, out var websocketMetadata);

        if (websocketMetadata != null)
        {
            _ = RemoveFromRooms(ws);
            var didRemove = Connections.Remove(ws.ConnectionInfo.Id);

            var status = didRemove
                ? "{Id} - Conexão do cliente {Username} removida com sucesso!"
                : "{Id} - Não foi possível remover a conexão do cliente {Username}.";

            Log.Write(
                level: didRemove
                    ? Serilog.Events.LogEventLevel.Information
                    : Serilog.Events.LogEventLevel.Error,
                messageTemplate: status,
                ws.ConnectionInfo.Id, websocketMetadata.Username);
        }
    }

    public static bool AddToRoom(IWebSocketConnection ws, int room)
    {
        if (!Enum.IsDefined(typeof(Room), room))
        {
            throw new RoomNotExistsException();
        }

        if (!Rooms.TryGetValue(room, out var connectionIds))
        {
            Log.Debug("Sala com id {Room} existe mas não foi criada, realizando criação.", room);

            connectionIds = new HashSet<Guid>();
            Rooms.Add(room, connectionIds);
        }

        if (connectionIds.TryGetValue(ws.ConnectionInfo.Id, out _))
        {
            throw new AlreadyInRoomException();
        }

        return connectionIds.Add(ws.ConnectionInfo.Id);
    }

    private static bool RemoveFromRooms(IWebSocketConnection ws)
    {
        foreach (var roomId in Rooms.Keys)
        {
            _ = Rooms.TryGetValue(roomId, out var connectionIds);

            if (connectionIds != null && connectionIds.Contains(ws.ConnectionInfo.Id))
            {
                var didRemove = connectionIds.Remove(ws.ConnectionInfo.Id);

                var status = didRemove
                    ? "{Id} - Cliente removido da sala {Room} com sucesso!"
                    : "{Id} - Não foi possível remover cliente da sala {Room}.";

                Log.Write(
                    level: didRemove
                        ? Serilog.Events.LogEventLevel.Information
                        : Serilog.Events.LogEventLevel.Error,
                    messageTemplate: status,
                    ws.ConnectionInfo.Id, Enum.GetName(typeof(Room), roomId));

                return didRemove;
            }
        }

        Log.Debug("{Id} - Cliente não está em uma sala para remove-lo.", ws.ConnectionInfo.Id);

        return false;
    }

    public static bool RemoveFromRoomById(IWebSocketConnection ws, int room)
    {
        if (!Enum.IsDefined(typeof(Room), room))
        {
            Log.Error("Sala com enum id {Room} não existe.", room);

            throw new RoomNotExistsException();
        }

        _ = Rooms.TryGetValue(room, out var connectionIds);

        if (connectionIds != null && connectionIds.Contains(ws.ConnectionInfo.Id))
        {
            var didRemove = connectionIds.Remove(ws.ConnectionInfo.Id);

            return didRemove;
        }

        Log.Debug("{Id} - Cliente não está na sala solicitada para remove-lo.", ws.ConnectionInfo.Id);

        throw new NotInARoomException();
    }

    public static async Task BroadcastToRoom(int room, string message)
    {
        if (!Rooms.TryGetValue(room, out var guids))
        {
            throw new RoomNotExistsException();
        }

        foreach (var guid in guids!)
        {
            if (Connections.TryGetValue(guid, out var ws))
            {
                Log.Debug("Mensagem enviada para {Guid}: {Message}", guid, message);

                await ws.Connection.Send(message);
            }
        }
    }
}
