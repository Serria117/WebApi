using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WebApp.Core.DomainEntities.Management;

[Table("MGR_UserTask")]
public class UserTask : BaseEntityAuditable<string>
{
    [MaxLength(64)]
    public new string Id { get; set; } = Ulid.NewUlid().ToString();

    [MaxLength(255)]
    public string TaskName { get; set; } = string.Empty;

    public User? Assignee { get; set; }
    public Organization Organization { get; set; } = null!;
    
    [MaxLength(500)]
    public string? Note { get; set; }

    public TaskStatus TaskStatus { get; set; } = TaskStatus.TODO;
    public DateTime AssignedDate { get; set; }
    public DateTime ResolvedDate { get; set; }
    public DateTime? DueDate { get; set; }
}

public enum TaskStatus
{
    TODO,
    INPROGRESS,
    RESOLVED,
    CANCELED
}