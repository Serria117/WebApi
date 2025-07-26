using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WebApp.Core.DomainEntities.Payroll;

[Table("PR_Dependents")]
public class Dependents : BaseEntity<long>
{
    [MaxLength(255)] [Column(Order = 0)]
    public string Name { get; set; } = string.Empty;

    [MaxLength(20)] [Column(Order = 1)]
    public string? PersonalId { get; set; }

    [MaxLength(20)] [Column(Order = 2)]
    public string? TaxId { get; set; }

    [Column(Order = 3)]
    public DateTime DateOfBirth { get; set; }

    [MaxLength(20)]
    public string? Relationship { get; set; }

    [MaxLength(20)]
    public string? OtherDocumentId { get; set; }

    [MaxLength(50)]
    public string? OtherDocument { get; set; }

    public DateTime EffectiveDate { get; set; }
    public DateTime? EndDate { get; set; }
    public long EmployeeId { get; set; }

    [ForeignKey(nameof(EmployeeId))]
    public Employee Employee { get; set; } = null!;
}

public struct DependentRelationship
{
    public const string Children = "Con";
    public const string Parents = "Cha/Mẹ";
    public const string Other = "Khác";
}