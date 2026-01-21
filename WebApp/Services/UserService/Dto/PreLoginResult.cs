namespace WebApp.Services.UserService.Dto;

public class PreLoginResult
{
    public Guid? Id { get; set; }
    public string? Username { get; set; }
    public bool Success { get; set; }
    public string? AuthCode { get; set; }
    public string? VerificationKey { get; set; }
    public List<OrganizationSelectDto>? OrganizationList { get; set; } = null;
}
