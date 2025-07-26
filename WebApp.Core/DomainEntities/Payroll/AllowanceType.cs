using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WebApp.Core.DomainEntities.Payroll;

[Table("PR_AllowanceType")]
public class AllowanceType : BaseEntity<long>
{
    [MaxLength(255)]
    public string Name { get; set; } = string.Empty;

    [MaxLength(50)]
    public string? Code { get; set; }

    [Column(TypeName = "decimal(18,2)")]
    public decimal MaxAmount { get; set; } = 0; //The maximum amount for this allowance type, if applicable

    [Column(TypeName = "decimal(18,2)")]
    public decimal DefaultAmount { get; set; } = 0; //The default amount for this allowance type, if applicable
    public bool IsTaxable { get; set; } = false;
    public bool IsInsurance { get; set; } = false;

    [MaxLength(50)]
    public string? Unit { get; set; }
    public Guid? OrganizationId { get; set; }
    
    [ForeignKey(nameof(OrganizationId))]
    public Organization? Organization { get; set; } //null if apply on global

    public ICollection<Allowance> Allowances { get; set; } = [];

}