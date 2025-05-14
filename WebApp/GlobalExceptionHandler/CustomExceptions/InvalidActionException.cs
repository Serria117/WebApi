namespace WebApp.GlobalExceptionHandler.CustomExceptions;

public class InvalidActionException(string message) : Exception(message);
