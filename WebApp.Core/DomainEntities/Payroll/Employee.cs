using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;
using Microsoft.EntityFrameworkCore;

namespace WebApp.Core.DomainEntities.Payroll;

[Table("PR_Employee")]
public class Employee : BaseEntityAuditable<long>
{
    [MaxLength(255)] [Column(Order = 1)]
    public string Name { get; set; } = string.Empty;
    [MaxLength(20)]
    public string? Code { get; set; }

    [MaxLength(20)] [Column(Order = 2)]
    public string? TaxId { get; set; }

    [MaxLength(20)] [Column(Order = 3)]
    public string? PersonalId { get; set; }

    [MaxLength(20)]
    public string? OtherDocumentId { get; set; }

    [MaxLength(50)]
    public string? OtherDocument { get; set; }

    public DateTime JoinDate { get; set; }
    public DateTime? EndDate { get; set; }
    public Guid OrganizationId { get; set; }

    [ForeignKey("OrganizationId")]
    public Organization Organization { get; set; } = null!;
    
    //Navigation properties:
    public ICollection<Salary> Salaries { get; set; } = [];
    public ICollection<Dependents> Dependents { get; set; } = [];
    public ICollection<Allowance> Allowances { get; set; } = [];
    public ICollection<Bonus> Bonus { get; set; } = [];
    [JsonIgnore]
    public ICollection<Timesheet> Timesheets { get; set; } = [];
    [JsonIgnore]
    public ICollection<ExpenseTypeHistory> ExpenseTypeHistories { get; set; } = [];
}