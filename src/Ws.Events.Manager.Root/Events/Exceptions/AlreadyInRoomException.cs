namespace Ws.Events.Manager.Root.Events.Exceptions;
public class AlreadyInRoomException(string message = "Client is already in the requested room") : Exception(message) { }
