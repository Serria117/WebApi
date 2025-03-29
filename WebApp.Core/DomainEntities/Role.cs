using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WebApp.Core.DomainEntities;

public class Role : BaseEntity<int>
{
    [MaxLength(255)]
    public string RoleName { get; set; } = string.Empty;
    [MaxLength(255)]
    public string? Description { get; set; }
    public HashSet<Permission> Permissions { get; set; } = [];
    public virtual HashSet<User> Users { get; set; } = [];
}