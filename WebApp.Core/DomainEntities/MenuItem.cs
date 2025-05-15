using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;

namespace WebApp.Core.DomainEntities;

[Index(nameof(Label))]
public class MenuItem: BaseEntity<int>
{
    [MaxLength(255)]
    public string Label { get; set; } = string.Empty;
    [MaxLength(50)]
    public string? Icon { get; set; } = string.Empty;
    [MaxLength(255)]
    public string? To { get; set; }

    public int Order { get; set; } = 0;
    
    public int? ParentId { get; set; }
    
    public ICollection<MenuItem> Items { get; set; } = new List<MenuItem>(); 
    public MenuItem? Parent { get; set; }

    public ICollection<MenuPermission> MenuPermissions { get; set; } = [];
}