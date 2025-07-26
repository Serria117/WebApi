using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WebApp.Core.DomainEntities.Payroll;
public class Department : BaseEntity<string>
{
    [MaxLength(255)]
    public string Name { get; set; } = string.Empty;
    [MaxLength(10)]
    public string? Code { get; set; }
    public Guid? OrganizationId { get; set; } //null if used globally
}
