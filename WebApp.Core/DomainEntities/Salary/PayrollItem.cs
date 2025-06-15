using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WebApp.Core.DomainEntities.Salary;

public class PayrollItem: BaseEntityAuditable<long>
{
    [MaxLength(255)]
    public string? Description { get; set; }

    [Column(TypeName = "decimal(18,2)")]
    public decimal Value { get; set; }

    [ForeignKey(nameof(PayrollRecordId))]
    public PayrollRecord PayrollRecord { get; set; } = null!;
    public long PayrollRecordId { get; set; } // Foreign key

    [ForeignKey(nameof(PayrollComponentTypeId))]
    public PayrollComponentType PayrollComponentType { get; set; } = null!;
    public int PayrollComponentTypeId { get; set; } // Foreign key
}