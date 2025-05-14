using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace WebApp.Core.DomainEntities;

[Index(nameof(Code))]
public class TaxOffice : BaseEntity<int>
{
    [MaxLength(255)]
    public string FullName { get; set; } = string.Empty;

    [MaxLength(255)]
    public string? ShortName { get; set; }

    [MaxLength(10)]
    public string Code { get; set; } = string.Empty;

    public Province? Province { get; set; }
    
    public int? ParentId { get; set; }
    
    //navigation properties
    public ICollection<TaxOffice>? Children { get; set; } = new List<TaxOffice>();
    public TaxOffice? Parent { get; set; }
}