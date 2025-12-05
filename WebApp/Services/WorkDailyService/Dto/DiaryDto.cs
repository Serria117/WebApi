using System.ComponentModel.DataAnnotations;
using WebApp.Core.DomainEntities;

namespace WebApp.Services.WorkDailyService.Dto;

public class DiaryDto
{
    [MaxLength(2000)]
    public string Contents { get; set; } = string.Empty;

    [MaxLength(255)]
    public string Subject { get; set; } = string.Empty;

    public Guid? OrganizationId { get; set; }
}

public class DiaryDisplayDto : DiaryDto
{
    public string Id { get; set; } = string.Empty;
}

public class DiaryEditDto
{
    public string Id { get; set; } = string.Empty;

    [MaxLength(255)]
    public string Subject { get; set; } = string.Empty;
    
    [MaxLength(4000)]
    public string Contents { get; set; } = string.Empty;
}

public class DiaryUpdateStatusDto
{
    public string Id { get; set; } = string.Empty;

    public WorkStatus WorkStatus { get; set; }
}

public class DiaryQueryDto
{
    public string? Keyword
    {
        get;
        set
        {
            if (value?.Length > 255)
            {
                field = value[..255];
            }
        }
    } = string.Empty;

    public Guid? OrganizationId { get; set; }
    public Guid? UserId { get; set; }
    public DateTime? FromDate { get; set; }
    public DateTime? ToDate { get; set; }
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 1000;
    public ICollection<WorkStatus> Status { get; set; } = [];
}