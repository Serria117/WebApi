using System.ComponentModel.DataAnnotations;

namespace WebApp.Core.DomainEntities;

public class EmailConfig : BaseEntity<int>
{
    [MaxLength(255)]
    public string Email { get; set; } = string.Empty;

    [MaxLength(255)]
    public string AppPassword { get; set; } = string.Empty;
}