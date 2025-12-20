using System.ComponentModel.DataAnnotations;
using WebApp.Core.DomainEntities;
using WebApp.Utils;

namespace WebApp.Services.WorkDailyService.Dto;

public class CommentDto
{
    public string WorkDiaryId { get; set; } = string.Empty;
    
    public string? ReplyId { get; set; }
    
    public string Content { get; set => field = value.TrimSpace(); } = string.Empty;
    
}

public class EditCommentDto
{
    public string Id { get; set; } = string.Empty;
    public string Content { get; set => field = value.TrimSpace(); } = string.Empty;
}

public class DeleteCommentDto
{
    public string Id { get; set; } = string.Empty;
    public bool? Permanent { get; set; } = false;
}
