using System.ComponentModel.DataAnnotations.Schema;

namespace WebApp.Core.DomainEntities.Salary;

public class PayrollItem: BaseEntityAuditable<long>
{
    public string? Description { get; set; }

    [ForeignKey(nameof(PayrollRecordId))]
    public required PayrollRecord PayrollRecord { get; set; }
    public long PayrollRecordId { get; set; } // Foreign key

    [ForeignKey(nameof(PayrollComponentTypeId))]
    public required PayrollComponentType PayrollComponentType { get; set; }
    public int PayrollComponentTypeId { get; set; } // Foreign key
}