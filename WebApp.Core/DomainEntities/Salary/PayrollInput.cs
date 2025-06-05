using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WebApp.Core.DomainEntities.Salary;

/// <summary>
/// Represents an input for payroll processing.
/// </summary>
public class PayrollInput : BaseEntity<int>
{
    /// <summary>
    /// The value of the payroll input.
    /// </summary>
    [MaxLength(255)]
    public string? Value { get; set; }

    /// <summary>
    /// The associated payroll record.
    /// </summary>
    [ForeignKey(nameof(PayrollRecordId))]
    public PayrollRecord PayrollRecord { get; set; } = null!;

    /// <summary>
    /// The foreign key for the associated payroll record.
    /// </summary>
    public long PayrollRecordId { get; set; } // Foreign key

    /// <summary>
    /// The type of the payroll input.
    /// </summary>
    [ForeignKey(nameof(PayrollInputTypeId))]
    public PayrollInputType PayrollInputType { get; set; } = null!;

    /// <summary>
    /// The foreign key for the payroll input type.
    /// </summary>
    public int PayrollInputTypeId { get; set; } // Foreign key
}