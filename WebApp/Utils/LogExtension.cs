using System.Runtime.CompilerServices;

namespace WebApp.Utils;

public static class LogExtension
{
    /// <param name="logger">The logger instance.</param>
    extension(ILogger logger)
    {
        /// <summary>
        /// Extension method to log a formatted error message with optional method name and exception details.
        /// </summary>
        /// <param name="message">The error message to log.</param>
        /// <param name="methodName">The name of the method where the error occurred (optional).</param>
        /// <param name="exception">The exception to log (optional).</param>
        public void LogErrorFormatted(string? message = "An error occurred",
                                      [CallerMemberName] string? methodName = default,
                                      Exception? exception = null)
        {
            var logMessage = $"Error: {message}";

            if (!string.IsNullOrEmpty(methodName))
            {
                logMessage += $" while executing method [{methodName}]";
            }

            if (exception != null)
            {
                logMessage += $"\nException: {exception.GetType().Name}.\n" +
                              $"Message: {exception.Message}\n" +
                              $"StackTrace: {exception.StackTrace}";
            }

            logger.LogError("{message}", logMessage);
        }

        /// <summary>
        /// Extension method to log a formatted informational message with optional method name details.
        /// </summary>
        /// <param name="message">The informational message to log.</param>
        /// <param name="methodName">The name of the method where the log is generated (optional).</param>
        public void LogInfoFormatted(string message = "Operation completed successfully",
                                     [CallerMemberName] string? methodName = default)
        {
            var logMessage = $"Info: {message}.";

            if (!string.IsNullOrEmpty(methodName))
            {
                logMessage += $" Executed method [{methodName}]";
            }

            logger.LogInformation("{message}", logMessage);
        }
    }
}