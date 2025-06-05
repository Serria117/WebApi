using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;
using WebApp.Enums.Payroll;

namespace WebApp.Core.DomainEntities.Salary;

/// <summary>
/// Represents a payroll period within an organization.
/// </summary>
[Index(nameof(Name))]
public class PayrollPeriod : BaseEntityAuditable<int>
{
    /// <summary>
    /// Gets or sets the name of the payroll period.
    /// </summary>
    [MaxLength(255)]
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the start date of the payroll period.
    /// </summary>
    public DateTime StartDate { get; set; }

    /// <summary>
    /// Gets or sets the end date of the payroll period.
    /// </summary>
    public DateTime EndDate { get; set; }

    /// <summary>
    /// Gets or sets the year of the payroll period.
    /// </summary>
    [Range(1990, 2900)]
    public int Year { get; set; }

    /// <summary>
    /// Gets or sets the month of the payroll period.
    /// </summary>
    [Range(1, 12)]
    public int Month { get; set; }

    /// <summary>
    /// Gets or sets the number of net workdays in the payroll period.
    /// </summary>
    [Column(TypeName = "decimal(18, 2)")]
    public decimal NetWorkDays { get; set; }
    /// <summary>
    /// The version of the payroll period.
    /// </summary>
    public int Version { get; set; }
    /// <summary>
    /// Indicates whether the payroll period is final. Final payroll periods are typically used for year-end processing or final settlements.
    /// </summary>
    public bool IsFinal { get; set; }
    public WeekendType WeekendType { get; set; } = WeekendType.Sunday;

    /// <summary>
    /// Indicates whether the payroll period is closed.
    /// </summary>
    public bool IsClosed { get; set; } = false;

    /// <summary>
    /// Gets or sets the organization associated with the payroll period.
    /// This is a navigation property.
    /// </summary>
    [ForeignKey(nameof(OrganizationId))]
    public Organization Organization { get; set; } = null!;

    /// <summary>
    /// Gets or sets the foreign key for the associated organization.
    /// </summary>
    public Guid OrganizationId { get; set; }

    /// <summary>
    /// Gets or sets the collection of payroll records associated with this payroll period.
    /// </summary>
    public ICollection<PayrollRecord> PayrollRecords { get; set; } = new List<PayrollRecord>();
}