using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

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
    /// Indicates whether the payroll period is closed.
    /// </summary>
    public bool IsClosed { get; set; } = false;

    /// <summary>
    /// Gets or sets the organization associated with the payroll period.
    /// This is a navigation property.
    /// </summary>
    [ForeignKey(nameof(OrganizationId))]
    public required Organization Organization { get; set; }

    /// <summary>
    /// Gets or sets the foreign key for the associated organization.
    /// </summary>
    public Guid OrganizationId { get; set; }
}