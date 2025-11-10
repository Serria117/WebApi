using WebApp.Core.DomainEntities;

namespace WebApp.Services.TaxDutyServices.Dto;

public class OrganizationTaxDutyDto
{
    public string? Id { get; set; }
    public int TaxReportDutyId { get; set; }
    public DutyPeriodType DutyPeriodType { get; set; }
}

public class OrganizationTaxDutyDisplayDto : OrganizationTaxDutyDto
{
    public string Name { get; set; } = string.Empty;
    public Guid OrganizationId { get; set; }
}

public class OrganizationTaxDutiesDto
{
    public Guid OrganizationId { get; set; }
    public List<OrganizationTaxDutyDto> Duties { get; set; } = [];
}