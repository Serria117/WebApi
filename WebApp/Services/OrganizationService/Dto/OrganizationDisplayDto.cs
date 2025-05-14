using WebApp.Core.DomainEntities;
using WebApp.Services.RegionService.Dto;

namespace WebApp.Services.OrganizationService.Dto;

public class OrganizationDisplayDto
{
    public Guid Id { get; init; }
    public string FullName { get; set; } = string.Empty;
    public string? ShortName { get; set; }
    public string TaxId { get; set; } = string.Empty;
    public List<string> Emails { get; init; } = [];
    public List<string> Phones { get; init; } = [];
    public string? TaxIdPwd { get; set; }
    public string? InvoicePwd { get; set; }
    public string? PinCode { get; set; }
    public string? Address { get; set; }
    public string? ContactAddress { get; set; }
    public DateTime? CreateAt { get; set; }
    public DateTime? LastUpdateAt { get; set; }
    public string? CreateBy { get; set; }
    public TaxOfficeDisplayDto? TaxOffice { get; set; }
    public DistrictDisplayDto? District { get; set; }
    public string? FiscalYearFirstDate { get; set; }
    public string? TypeOfVatPeriod { get; set; }
    
    public HashSet<OrganizationLoginInfoDto> OrganizationLoginInfos { get; set; } = [];
}

public class OrganizationLoginInfoDto 
{
    public int? Id { get; set; }
    public string? AccountName { get; set; }
    public string? Url { get; set; }
    public string? Provider { get; set; }
    public string? Username { get; set; }
    public string? Password { get; set; }
}