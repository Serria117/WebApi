using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace WebApp.Core.DomainEntities;
[Index(nameof(MessageId), nameof(HashValue), IsUnique = true)]
public class EmailAttachment : BaseEntityAuditable<long>
{
    [MaxLength(255)]
    public string? Email { get; set; }

    [MaxLength(64)]
    public string? MessageId { get; set; }

    [MaxLength(255)]
    public string FileName { get; set; } = string.Empty;

    [MaxLength(255)]
    public string? HashValue { get; set; }

    public Guid? OrganizationId { get; set; }
}
