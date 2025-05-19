namespace WebApp.Payloads;

/// <summary>
/// Represents a request to reset a user's password.
/// </summary>
/// <remarks>
/// This class is used as a payload in operations requiring a user password update.
/// It contains the necessary information to identify the user and specify the new password.
/// </remarks>
public class ResetPasswordRequest
{
    public Guid UserId { get; set; }
    public string Password { get; set; } = string.Empty;
}