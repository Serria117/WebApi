using System.ComponentModel.DataAnnotations;

namespace WebApp.Services.UserService.Dto;

public class UserInputDto
{
    [MinLength(3), MaxLength(255)]
    public string Username { get; set; } = string.Empty;

    [MinLength(6), MaxLength(100)]
    public string Password { get; set; } = string.Empty;

    [MaxLength(255)] [EmailAddress]
    public string? Email { get; set; }

    [MaxLength(255)]
    public string? FullName { get; set; }

    public bool? Locked { get; set; } = false;
    public ISet<int> Roles { get; set; } = new HashSet<int>();
    public List<Guid> Organizations { get; set; } = [];
}