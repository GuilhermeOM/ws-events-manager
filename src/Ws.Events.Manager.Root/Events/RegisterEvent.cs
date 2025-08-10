using Fleck;
using Ws.Events.Manager.Root.Entities;
using Ws.Events.Manager.Root.Entities.Feedback;
using Ws.Events.Manager.Root.Events.Base;
using Ws.Events.Manager.Root.Events.Exceptions;

namespace Ws.Events.Manager.Root.Events;
public class RegisterEvent : BaseEvent
{
    public required string Username { get; set; }
}

public class Register : BaseHandler<RegisterEvent>
{
    public override async Task Handle(RegisterEvent eventType, IWebSocketConnection socket)
    {
        if (eventType.Username.Equals("Anonymous", StringComparison.OrdinalIgnoreCase))
        {
            throw new ReservedUsernameException();
        }

        var usernameInUse = StateService
            .Connections
            .Keys
            .Any(connectionId => StateService
                .Connections[connectionId]
                .Username
                .Equals(eventType.Username, StringComparison.Ordinal));

        if (usernameInUse)
        {
            throw new UsernameInUseException();
        }

        StateService.Connections[socket.ConnectionInfo.Id].Username = eventType.Username;

        await socket.Send(new Message<RegisterMessage>()
        {
            ConnectionId = socket.ConnectionInfo.Id,
            Name = "REGISTER_FEEDBACK",
            Data = new RegisterMessage()
            {
                Feedback = "User successfully registered!",
                Username = StateService.Connections[socket.ConnectionInfo.Id].Username
            },
        }.AsJson());
    }
}

