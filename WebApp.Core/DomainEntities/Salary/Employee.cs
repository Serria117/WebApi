using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WebApp.Core.DomainEntities.Salary;

public class Employee : BaseEntityAuditable<Guid>
{
    [MaxLength(255)]
    public string FullName { get; set; } = string.Empty;

    [MaxLength(20)]
    public string? Pid { get; set; } // Personal ID

    [MaxLength(20)]
    public string? TaxId { get; set; }

    public DateTime HireDate { get; set; }
    public DateTime? TerminationDate { get; set; }
    public bool IsActive { get; set; } = true;

    public required Organization Organization { get; set; }

    [ForeignKey(nameof(Organization))]
    public Guid OrganizationId { get; set; } // Foreign key
}