namespace WebApp.Services.UserService.Dto;

public class UserDisplayDto
{
    public Guid Id { get; set; }
    public string Username { get; set; } = string.Empty;
    public string? Email { get; set; }
    public string? FullName { get; set; }
    public bool? Locked { get; set; }
    public HashSet<RoleDisplayDto> Roles { get; set; } = [];

    public List<OrganizationInUserDto>? Organizations { get; set; } = [];
}

public class OrganizationInUserDto
{
    public Guid Id { get; set; }
    public string TaxId { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
}