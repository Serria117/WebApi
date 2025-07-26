using System.ComponentModel.DataAnnotations;
using WebApp.Core.DomainEntities.Payroll;

namespace WebApp.Services.PayrollService.Dto;

public class PayrollPeriodCreateDto
{
    [Range(2000, 9999)]
    public int Year { get; set; }

    public Weekend Weekend { get; set; }

    public PayrollPeriodCreateDto Valid()
    {
        if (Year > DateTime.Now.Year) Year = DateTime.Now.Year;
        return this;
    }
}

public class PayrollPeriodDisplay
{
    public long Id { get; set; }
    public int Year { get; set; }
    public int Version { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public int TotalWorkDay { get; set; }
    public Weekend Weekend { get; set; }
    public string? Name { get; set; }
    public Guid OrganizationId { get; set; }
}

public class PayrollPeriodQuery
{
    public int? Year { get; set; }
    public int? Version { get; set; }
}