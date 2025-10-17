namespace WebApp.GlobalExceptionHandler.CustomExceptions;

public class EmptyInputException(string message = "The input has no data") : Exception(message)
{
}