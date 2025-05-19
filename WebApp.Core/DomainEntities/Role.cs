using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;

namespace WebApp.Core.DomainEntities;

[Index(nameof(RoleName))]
public class Role : BaseEntity<int>
{
    [MaxLength(255)]
    public string RoleName { get; set; } = string.Empty;
    [MaxLength(255)]
    public string? Description { get; set; }
    public HashSet<Permission> Permissions { get; set; } = [];
    public HashSet<User> Users { get; set; } = [];
}