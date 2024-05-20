namespace WsUiManager.Events.Exceptions;

public class NotInARoomException(string message = "Cliente não está na sala solicitada.") : Exception(message) { }
