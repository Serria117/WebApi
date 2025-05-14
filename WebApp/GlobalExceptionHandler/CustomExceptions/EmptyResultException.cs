namespace WebApp.GlobalExceptionHandler.CustomExceptions;

/// <summary>
/// Represents an exception that is thrown when an operation results in an empty or missing result.
/// </summary>
public class EmptyResultException(string message = "No data found") : Exception(message)
{
    
}