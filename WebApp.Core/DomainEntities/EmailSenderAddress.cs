using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace WebApp.Core.DomainEntities;
[Index(nameof(Email))]
public class EmailSenderAddress : BaseEntity<long>
{
    [MaxLength(255)]
    public string Email { get; set; } = string.Empty;

    [MaxLength(255)]
    public string? Name { get; set; }

    public Guid? OrganizationId { get; set; }
}
