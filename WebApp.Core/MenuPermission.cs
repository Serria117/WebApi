using System.ComponentModel.DataAnnotations.Schema;
using WebApp.Core.DomainEntities;

namespace WebApp.Core;

public class MenuPermission
{
    public int PermissionId { get; set; }
    public int MenuId { get; set; }

    [ForeignKey("PermissionId")]
    public Permission Permission { get; set; }

    [ForeignKey("MenuId")]
    public MenuItem MenuItem { get; set; }

    public DateTime GrantedAt { get; set; }
}