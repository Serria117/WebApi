namespace WebApp.Services.UserService.Dto;

public class RoleDisplayDto
{
    public int Id { get; set; }
    public string RoleName { get; set; } = string.Empty;
    public string? Description { get; set; }
    public HashSet<PermissionDisplayDto> Permissions { get; set; } = [];
    public ISet<UserInfoDto>? Users { get; set; } = new HashSet<UserInfoDto>();
}

public class UserInfoDto
{
    public Guid Id { get; set; }
    public string Username { get; set; } = string.Empty;
}