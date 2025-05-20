using System.ComponentModel.DataAnnotations.Schema;

namespace WebApp.Core.DomainEntities.Salary;

public class PayrollComponentType : BaseEntity<int>
{
    public required string Name { get; set; }
    public string? Description { get; set; }
    public Guid? OrganizationId { get; set; }

    [ForeignKey(nameof(OrganizationId))]
    public Organization? Organization { get; set; }

    public int PayrollComponentCategoryId { get; set; }

    [ForeignKey(nameof(PayrollComponentCategoryId))]
    public required PayrollComponentCategory PayrollComponentCategory { get; set; }

    public bool IsActive { get; set; } = true;
}