namespace Ws.Events.Manager.Root.Events.Exceptions;
public class NotInARoomException(string message = "Client is not in the requested room") : Exception(message) { }
