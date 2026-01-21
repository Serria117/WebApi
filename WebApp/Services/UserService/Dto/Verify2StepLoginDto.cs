namespace WebApp.Services.UserService.Dto;

public class Verify2StepLoginDto
{
    public string AuthCode { get; set; } = string.Empty;
    public string VerificationKey { get; set; } = string.Empty;
    public string VerificationCode { get; set; } = string.Empty;
    public Guid UserId { get; set; }
}
