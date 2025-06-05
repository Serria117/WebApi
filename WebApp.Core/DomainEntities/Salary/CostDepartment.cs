using System.ComponentModel.DataAnnotations;

namespace WebApp.Core.DomainEntities.Salary;

public class CostDepartment : BaseEntityAuditable<int>
{
    [MaxLength(255)]
    public string Name { get; set; } = string.Empty;

    [MaxLength(255)]
    public string? Description { get; set; }

    /// <summary>
    /// Get or set the organization ID that this cost department belongs to. Null if it is not associated with any organization.
    /// </summary>
    public Guid? OrganizationId { get; set; } = null;

    public List<Employee> Employees { get; set; } = []; // Navigation property
}