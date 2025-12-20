using System.ComponentModel.DataAnnotations;
using WebApp.Core.DomainEntities;
using WebApp.Enums;
using WebApp.Utils;

namespace WebApp.Services.WorkDailyService.Dto;

public class DiaryDto
{
    [MaxLength(2000)]
    public string Content { get; set; } = string.Empty;

    [MaxLength(255)]
    public string Subject { get; set; } = string.Empty;

    public Guid? OrganizationId { get; set; }

    public DateTime? DueDate
    {
        get;
        set
        {
            if (value.HasValue && value.Value.Date > DateTime.Now) field = null;
        }
    }

    public WorkPriority Priority { get; set; }
}

public class DiaryDisplayDto : DiaryDto
{
    public string Id { get; set; } = string.Empty;
}

public class DiaryEditDto
{
    public string Id { get; set; } = string.Empty;

    [MaxLength(255)]
    public string Subject
    {
        get;
        set
        {
            var trimedValue = value.TrimSpace();
            field = trimedValue.Length > 255
                ? trimedValue[..255]
                : trimedValue;
        }
    } = string.Empty;

    [MaxLength(4000)]
    public string Contents { get; set; } = string.Empty;
}

public class DiaryUpdateStatusDto
{
    public string Id
    {
        get;
        set => field = value.TrimSpace();
    } = string.Empty;

    public WorkStatus WorkStatus { get; set; }
    public WorkPriority Priority { get; set; }
    public DateTime? DueDate { get; set; }
}

/// <summary>
/// Represents the data transfer object used for querying diaries with various filtering,
/// pagination, and sorting criteria.
/// </summary>
/// <remarks>
/// This DTO includes properties for specifying keyword-based search, date range filtering,
/// organization and user-level scoping, page number, and page size. It also allows filtering
/// diaries based on their statuses.
/// </remarks>
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

    public DateTime? FromDate
    {
        get;
        set
        {
            if (value.HasValue && value.Value.Date > DateTime.Now)
            {
                field = DateTime.Now;
            }
            else
            {
                field = value;
            }
        }
    }

    public DateTime? ToDate
    {
        get;
        set
        {
            if (value.HasValue && value.Value.Date > DateTime.Now)
            {
                field = DateTime.Now;
            }
            else
            {
                field = value;
            }
        }
    }

    public int Page { get; set; } = SystemBoundary.MinPageIndex;
    public int Size { get; set; } = SystemBoundary.MaxPageSize;
    public ICollection<WorkStatus> Status { get; set; } = [];
}