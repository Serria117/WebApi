using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WebApp.Core.DomainEntities;
public class TemplateFile : BaseEntity<int>
{
    [Column(TypeName = "NVARCHAR(20)")]
    public string Version { get; set; } = string.Empty;

    [Column(TypeName = "NVARCHAR(255)")]
    public string FileName { get; set; } = string.Empty;

    [Column(TypeName = "NVARCHAR(255)")]
    public string FilePath { get; set; } = string.Empty;

    [Column(TypeName = "NVARCHAR(255)")]
    public string? VersionNote { get; set; }
    public DateTime UploadTime { get; set; }

}
