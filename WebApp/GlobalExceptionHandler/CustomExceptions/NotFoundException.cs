namespace WebApp.GlobalExceptionHandler.CustomExceptions;

public class NotFoundException(string message): Exception(message)
{
    /// <summary>
    /// Thrown when an entity is not found in the database or collection.
    /// </summary>
    /// <param name="obj">The entity to check for null</param>
    /// <param name="message">A message to be sent in the exception</param>
    /// <exception cref="NotFoundException"></exception>
    public static void ThrowIfNull(object? obj, string message)
    {
        if (obj is null)
        {
            throw new NotFoundException(message);
        }
    }
}
