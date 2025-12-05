using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WebApp.Core.DomainEntities;

[Table("UserWorkDiary")]
public class UserWorkDiary : BaseEntityAuditable<string>
{
    [MaxLength(26)]
    public new string Id { get; set; } = new Ulid().ToString();

    [MaxLength(255)]
    public string Subject { get; set; } = string.Empty;

    [MaxLength(4000)]
    public string Content { get; set; } = string.Empty;

    public Guid? OrganizationId { get; set; }

    public Guid? UserId { get; set; }

    public WorkStatus WorkStatus { get; set; } = WorkStatus.InProgress;
    
    //navigation properties
    [ForeignKey(nameof(UserId))]
    public User? User { get; set; }

    [ForeignKey(nameof(OrganizationId))]
    public Organization? Organization { get; set; }

    public ICollection<UserWorkDiaryComment> Comments { get; set; } = [];
}

public enum WorkStatus
{
    InProgress,
    Cancelled,
    Done
}