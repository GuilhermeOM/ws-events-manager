namespace WsUiManager.Events.Exceptions;

public class UsernameInUseException(string message = "Nome de usuário já está em uso.") : Exception(message) { }
