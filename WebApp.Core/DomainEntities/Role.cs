using System.ComponentModel.DataAnnotations;

namespace WebApp.Core.DomainEntities;

public class Role : BaseEntity<int>
{
    [MaxLength(255)]
    public string RoleName { get; set; } = string.Empty;
    [MaxLength(255)]
    public string? Description { get; set; }
    public HashSet<Permission> Permissions { get; set; } = [];
    public HashSet<User> Users { get; set; } = [];
}