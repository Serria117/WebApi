using System.ComponentModel.DataAnnotations;
using WebApp.Core.DomainEntities;

namespace WebApp.Services.TaxDutyServices.Dto;

public class TaxDutyRecordDto
{
    public int TaxDutyId { get; set; }
    public Guid OrganizationId { get; set; }
    public DutyPeriodType DutyPeriodType { get; set; }
    public DutyStatus Status { get; set; } = DutyStatus.Unreported;
    public TaxPaymentStatus PaymentStatus { get; set; } = TaxPaymentStatus.Unpaid;
    public DateTime? DueDate { get; set; }

    public decimal TaxPayable { get; set; } = 0;

    [MaxLength(20)]
    public string Period { get; set; } = string.Empty;

    [MaxLength(500)]
    public string? Note { get; set; }
}

public class TaxDutyRecordDisplayDto : TaxDutyRecordDto
{
    public string Id { get; set; } = string.Empty;
}