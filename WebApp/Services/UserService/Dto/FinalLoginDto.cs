namespace WebApp.Services.UserService.Dto;

public class FinalLoginDto
{
    public string AuthCode { get; set; } = string.Empty;
    public string? VerificationCode { get; set; }
    public string? VerificationKey { get; set; }
    public Guid OrganizationId { get; set; }
}