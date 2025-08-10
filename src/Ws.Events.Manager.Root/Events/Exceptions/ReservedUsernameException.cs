namespace Ws.Events.Manager.Root.Events.Exceptions;
public class ReservedUsernameException(string message = "Requested name is a reserved one") : Exception(message) { }
