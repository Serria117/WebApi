using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WebApp.Core.DomainEntities;

/// <summary>
/// Store tax declaration task for each period
/// </summary>
[Table(name: "TaxReportTask")]
public class TaxDeclarationTask : BaseEntity<int>
{
    [MaxLength(255)]
    public string TaskName { get; set; } = string.Empty;

    [MaxLength(1000)]
    public string? Description { get; set; } = string.Empty;

    [MaxLength(20)]
    public string Period { get; set; } = string.Empty;

    public DateTime StartDate { get; set; }
    public DateTime DueDate { get; set; }
    public DateTime FinishedAt { get; set; }

    public bool Status { get; set; } = false;
    public User? Assignee { get; set; }
}
