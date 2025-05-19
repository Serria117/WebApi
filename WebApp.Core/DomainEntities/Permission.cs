using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace WebApp.Core.DomainEntities;

[Index(nameof(PermissionName))]
public class Permission : BaseEntity<int>
{
    [MaxLength(255)]
    public string PermissionName { get; set; } = string.Empty;
    [MaxLength(255)]
    public string? Description { get; set; }
    [MaxLength(255)]
    public string? FrontEndAccess { get; set; }

    public ISet<Role> Roles { get; set; } = new HashSet<Role>();

    public ICollection<MenuPermission> MenuPermissions { get; set; } = [];
}