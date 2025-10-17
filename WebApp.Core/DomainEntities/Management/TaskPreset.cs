using System.ComponentModel.DataAnnotations.Schema;

namespace WebApp.Core.DomainEntities.Management;
[Table("MGR_TaskPreset")]
public class TaskPreset : BaseEntity<int>
{
    public string PresetName { get; set; } = string.Empty;
    public string? Description { get; set; }

}
