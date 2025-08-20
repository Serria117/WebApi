using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WebApp.Core.DomainEntities;

public class Template : BaseEntity<int>
{
    [MaxLength(255)]
    public string Name { get; set; } = string.Empty;
    
    [MaxLength(255)]    
    public string? Description { get; set; }

    [Range(0,10000)]
    public int Order { get; set; }

    public ICollection<TemplateFile> TemplateFiles { get; set; } = [];
}
