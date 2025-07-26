namespace WebApp.GlobalExceptionHandler.CustomExceptions;

public class RequestCanceledException(string message) : Exception(message)
{
}
