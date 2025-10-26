namespace WebApp.Services.UserService.Dto;

public class AuthenticatedUser
{
    public Guid UserId { get; set; }
    public List<string> Permissions { get; set; } = [];
    public string Username { get; set; }
    
}