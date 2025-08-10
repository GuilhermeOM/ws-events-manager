namespace Ws.Events.Manager.Root.Events.Exceptions;
public class EventFailedException(string message = "Failure executing the event") : Exception(message) { }
