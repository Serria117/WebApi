using System.ComponentModel.DataAnnotations;
using WebApp.Core.DomainEntities.Tax;
using WebApp.Utils;

namespace WebApp.Services.TaxRegulationServices.Dto;

public class SocialSecurityDto
{
    [MaxLength(255)]
    public string Name { get => field; set => field = value.TrimSpace(); } = string.Empty; 
    public DateTime EffectiveDate { get; set; }
    public DateTime? EndDate { get; set; }
    public List<SocialSecurityRate> EmployeeRate { get; set; } = [];
    public List<SocialSecurityRate> EmployerRate { get; set; } = [];
    public int? Order { get; set; }
}

public class SocialSecurityDisplayDto
{
    public Guid Id { get; set; }
    public string Name { get => field; set => field = value.TrimSpace(); } = string.Empty;
    public DateTime EffectiveDate { get; set; }
    public DateTime? EndDate { get; set; }
    public int? Order { get; set; }
    public List<SocialSecurityRateDto> EmployeeRate { get; set; } = [];
    public List<SocialSecurityRateDto> EmployerRate { get; set; } = [];
}

public class SocialSecurityRateDto
{
    public string Name { get; set; } = string.Empty;
    public SocialSecurityBearer Bearer { get; set; }
    public decimal Rate { get; set; }
}
