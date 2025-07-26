namespace WebApp.GlobalExceptionHandler.CustomExceptions;

public class RequestFailedException(string message): Exception(message)
{
}
