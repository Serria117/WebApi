using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WebApp.Core.DomainEntities.Payroll;

[Table("PR_PayrollPeriod")]
public class PayrollPeriod : BaseEntity<long>
{
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public int TotalWorkDay { get; set; }
    public Weekend Weekend { get; set; } = Weekend.Sunday;
    public int Year { get; set; }
    public int Version { get; set; }

    [MaxLength(255)]
    public string? Name { get; set; }

    public Guid OrganizationId { get; set; }

    [ForeignKey(nameof(OrganizationId))]
    public Organization Organization { get; set; } = null!;

    public ICollection<Timesheet> Timesheets { get; set; } = [];
}

public enum Weekend
{
    Sunday, SundayAndSaturday
}