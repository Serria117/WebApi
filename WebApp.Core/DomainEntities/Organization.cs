using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;

namespace WebApp.Core.DomainEntities;

[Index(nameof(UnsignName))][Index(nameof(TaxId))]
public class Organization : BaseEntityAuditable<Guid>
{
    [MaxLength(500)] [MinLength(3)] [Required]
    public string FullName { get; set; } = string.Empty;

    [MaxLength(500)] [MinLength(3)] 
    public string UnsignName { get; set; } = string.Empty;

    [MaxLength(50)] [MinLength(3)] 
    public string? ShortName { get; set; }

    [MaxLength(1000)]
    public string? Address { get; set; }

    [MaxLength(1000)]
    public string? ContactAddress { get; set; }
        
    [MaxLength(20)] [MinLength(3)] 
    public string TaxId { get; set; } = string.Empty;
        
    public List<string> Emails { get; set; } = [];
    
    public List<string> Phones { get; set; } = [];
        
    [MaxLength(50)] [MinLength(3)] 
    public string? TaxIdPwd { get; set; }
        
    [MaxLength(50)] [MinLength(3)] 
    public string? InvoicePwd { get; set; }
        
    [MaxLength(50)] [MinLength(3)]
    public string? PinCode { get; set; }

    public TaxOffice? TaxOffice { get; set; }

    public District? District { get; set; }

    [MaxLength(5)]
    public string? FiscalYearFistDate { get; set; } = "01/01";
}