namespace WsUiManager.Events.Exceptions;

public class RoomNotExistsException(string message = "Sala não existe.") : Exception(message) { }
