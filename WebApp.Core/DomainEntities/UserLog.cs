using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WebApp.Core.DomainEntities;
public class UserLog : BaseEntity<Guid>
{
    [MaxLength(255)]
    public string? UserName { get; set; }
    public Guid? UserId { get; set; }
    [MaxLength(100)]
    public string? Action { get; set; }
    [MaxLength(255)]
    public string? Objective { get; set; }
    [MaxLength(255)]
    public string? Description { get; set; }
    public DateTime ActionTime { get; set; }
    public bool Success { get; set; } = true;

    [MaxLength(100)]
    public string? Ip { get; set; }
}
