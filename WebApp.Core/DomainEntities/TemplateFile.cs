using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WebApp.Core.DomainEntities;
public class TemplateFile : BaseEntity<int>
{
    [MaxLength(20)]
    public string Version { get; set; } = string.Empty;

    [MaxLength(255)]
    public string FileName { get; set; } = string.Empty;

	[MaxLength(255)]
	public string FilePath { get; set; } = string.Empty;

	[MaxLength(255)]
	public string? VersionNote { get; set; }
    public DateTime UploadTime { get; set; }

	public int TemplateId { get; set; }

    [ForeignKey("TemplateId")]
	public Template Template { get; set; } = null!;

}
