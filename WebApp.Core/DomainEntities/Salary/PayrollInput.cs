using System.ComponentModel.DataAnnotations.Schema;

namespace WebApp.Core.DomainEntities.Salary;

public class PayrollInput : BaseEntity<int>
{
    public string? Value { get; set; }

    [ForeignKey(nameof(PayrollRecordId))]
    public required PayrollRecord PayrollRecord { get; set; }
    public long PayrollRecordId { get; set; } // Foreign key

    [ForeignKey(nameof(PayrollInputTypeId))]
    public required PayrollInputType PayrollInputType { get; set; }
    public int PayrollInputTypeId { get; set; } // Foreign key
}