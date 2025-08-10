namespace Ws.Events.Manager.Root.Events.Exceptions;
public class UsernameInUseException(string message = "User name is already in use") : Exception(message) { }
