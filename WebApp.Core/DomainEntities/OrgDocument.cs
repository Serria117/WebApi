using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WebApp.Enums;

namespace WebApp.Core.DomainEntities;
public class OrgDocument : BaseEntityAuditable<int>
{
    public required Organization Organization { get; set; }

    public required DocumentType DocumentType { get; set; }
    
    [StringLength(255)]
    public string? DocumentName { get; set; }

    public int? NumberOfAdjustment { get; set; }

    [StringLength(1)]
    public string? AdjustmentType { get; set; }
    
    [StringLength(10)]
    public string? Period { get; set; }
    
    [StringLength(1)]
    public string? PeriodType { get; set; }
    
    [StringLength(10)]
    public DateTime? DocumentDate { get; set; }

    [StringLength(255)]
    public string FileName { get; set; } = string.Empty;

    [StringLength(255)]
    public string FilePath { get; set; } = string.Empty;

    [Required]
    public DateTime UploadTime { get; set; }
    public long FileSize { get; set; }
}
