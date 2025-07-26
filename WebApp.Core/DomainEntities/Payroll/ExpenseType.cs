using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WebApp.Core.DomainEntities.Payroll;
[Table("PR_ExpenseType")]
public class ExpenseType : BaseEntity<string>
{
    [MaxLength(255)]
    public string Name { get; set; } = string.Empty;
    [MaxLength(20)]
    public string? Code { get; set; }
    public Guid? OrganizationId { get; set; }

    [ForeignKey("OrganizationId")]
    public Organization? Organization { get; set; }

    public ICollection<ExpenseTypeHistory> ExpenseTypeHistories { get; set; } = [];
}
