using System.ComponentModel.DataAnnotations;

namespace WebApp.Core.DomainEntities;

public class OrganizationInfo : BaseEntityAuditable<int>
{
    [MaxLength(255)]
    public string FullName { get; set; } = string.Empty;

    [MaxLength(255)]
    public string Address { get; set; } = string.Empty;

    public int District { get; set; }
    
    public int TaxOfficeId { get; set; }
    
    public Guid Organization { get; set; }
    
    public bool IsCurrent { get; set; } = true;

    [MaxLength(3)]
    public string? TypeOfVatPeriod { get; set; } = "Q";
}