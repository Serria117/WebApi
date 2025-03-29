using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;

namespace WebApp.Core.DomainEntities;

[Index(nameof(TaxId), IsUnique = true)]
public class RiskCompany : BaseEntityAuditable<int>
{
    [MaxLength(255)]
    public string Name { get; set; } = string.Empty;
    
    [MaxLength(255)]
    public string TaxId { get; set; } = string.Empty;
    
    [MaxLength(255)]
    public string? TaxOffice { get; set; }

}