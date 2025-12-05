using System.ComponentModel.DataAnnotations;
using WebApp.Core.DomainEntities;
using WebApp.Core.DomainEntities.Accounting.TaxDeclarations;

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

public class TaxDutyRecordUpdateDto
{
    public string Id { get; set; } = string.Empty;
    public DutyStatus Status { get; set; } = DutyStatus.Unreported;
    public TaxPaymentStatus PaymentStatus { get; set; } = TaxPaymentStatus.Unpaid;
    public decimal TaxPayable { get; set; } = 0;
}

/// <summary>
/// Create DTO for tax duty records
/// </summary>
public class TaxDutyRecordsCreateDto
{
    public ICollection<Guid> Organizations { get; set; } = [];
    public DutyPeriodType DutyPeriodType { get; set; }
    public string Period { get; set; } = string.Empty;
}

public class TaxDutyRecordUploadXml
{
    public string Id { get; set; } = string.Empty;
    public IFormFile File { get; set; } = null!;
}

public class TaxDutyRecordUploadMultiXml
{
    public ICollection<IFormFile> Files { get; set; } = [];
    public bool ReplaceExisting { get; set; } = false;
}

public class TaxDutyDocumentUpdateStatus
{
    public string Id { get; set; } = string.Empty;
    public TaxResponseStatus TaxResponseStatus { get; set; }
    
}

public class TaxDutyDocumentRemoveDto
{
    public string DocumentId { get; set; } = string.Empty;
    public bool Permanent { get; set; } = false;
}