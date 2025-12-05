using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WebApp.Core.DomainEntities;

public class UserWorkDiaryComment : BaseEntityAuditable<string>
{
    [MaxLength(26)]
    public new string Id { get; set; } = Ulid.NewUlid().ToString();

    [MaxLength(26)]
    public string WorkDiaryId { get; set; } = string.Empty;
    
    [MaxLength(26)]
    public string? ReplyId { get; set; }
    
    [MaxLength(2000)]
    public string Content { get; set; } = string.Empty;
    
    public Guid UserId { get; set; }
    
    //Navigation properties
    [ForeignKey(nameof(UserId))]
    public User User { get; set; } = null!;

    [ForeignKey(nameof(WorkDiaryId))]
    public UserWorkDiary UserWorkDiary { get; set; } = null!;
}