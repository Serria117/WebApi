namespace WebApp.GlobalExceptionHandler.CustomExceptions;

public class NotFoundException(string message): Exception(message)
{
}
