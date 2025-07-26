using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WebApp.Core.DomainEntities.Payroll;

[Table("PR_PayrollField")]
public class PayrollField : BaseEntity<long>
{
    [MaxLength(255)]
    public string Name { get; set; } = string.Empty;

    [Column(TypeName = "NVARCHAR(MAX)")]
    public string FieldValue { get; set; } = string.Empty;

    public long EmployeeId { get; set; }
    public long PayrollFieldDefinitionId { get; set; }


    [ForeignKey(nameof(EmployeeId))]
    public Employee Employee { get; set; } = null!;

    [ForeignKey(nameof(PayrollFieldDefinitionId))]
    public PayrollFieldDefinition PayrollFieldDefinition { get; set; } = null!;

}