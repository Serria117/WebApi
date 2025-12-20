using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WebApp.Core.DomainEntities;

[Table("UserWorkDiary"), Index(nameof(WorkStatus))]
public class UserWorkDiary : BaseEntityAuditable<string>
{
    [MaxLength(26)]
    public new string Id { get; set; } = Ulid.NewUlid().ToString();

    [MaxLength(255)]
    public string Subject { get; set; } = string.Empty;

    [MaxLength(4000)]
    public string Content { get; set; } = string.Empty;

    public Guid? OrganizationId { get; set; }

    public Guid? UserId { get; set; }

    public WorkStatus WorkStatus { get; set; } = WorkStatus.InProgress;

    public DateTime? DueDate { get; set; }

    public WorkPriority Priority { get; set; } = WorkPriority.Medium;

    //navigation properties
    [ForeignKey(nameof(UserId))]
    public User? User { get; set; }

    [ForeignKey(nameof(OrganizationId))]
    public Organization? Organization { get; set; }

    public ICollection<UserWorkDiaryComment> Comments { get; set; } = [];
}

public enum WorkStatus
{
    InProgress = 0,
    Done = 1,
    Cancelled = 2

}

public enum WorkPriority
{
    Low = 0,
    Medium = 1,
    High = 2,
    Urgent = 3
}