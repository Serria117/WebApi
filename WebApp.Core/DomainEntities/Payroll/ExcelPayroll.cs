using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WebApp.Core.DomainEntities.Payroll;

[Table("PR_ExcelPayroll")]
public class ExcelPayroll : BaseEntityAuditable<string>
{
    [MaxLength(26)]
    public new string Id { get; set; } = string.Empty;

    public int Year { get; set; }
    public int Version { get; set; }

    [MaxLength(255)]
    public string FileName { get; set; } = string.Empty;

    public bool IsFinal { get; set; } = false;

    public Guid OrganizationId { get; set; }

    [ForeignKey(nameof(OrganizationId))]
    public Organization Organization { get; set; } = null!;
}