using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace WebApp.Core.DomainEntities.Payroll;
[Table("PR_ExpenseTypeHistory")]
public class ExpenseTypeHistory: BaseEntity<string>
{
    public string ExpenseTypeId { get; set; } = null!;

    public DateTime EffectiveDate { get; set; }
    public DateTime? EndDate { get; set; }

    public Guid OrganizationId { get; set; }

    public ICollection<Employee> Employees { get; set; } = [];

    [ForeignKey("OrganizationId")]
    public Organization Organization { get; set; } = null!;

    [ForeignKey("ExpenseTypeId")][DeleteBehavior(DeleteBehavior.NoAction)]
    public ExpenseType ExpenseType { get; set; } = null!;
}
