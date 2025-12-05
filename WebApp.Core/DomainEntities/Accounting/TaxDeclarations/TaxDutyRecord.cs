using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WebApp.Core.DomainEntities.Accounting.TaxDeclarations;

[Table("TaxDutyRecords")]
public class TaxDutyRecord : BaseEntity<string>
{
    [MaxLength(30)]
    public new string Id { get; set; } = Ulid.NewUlid().ToString();

    public int TaxDutyId { get; set; }
    public Guid OrganizationId { get; set; }
    public DutyPeriodType DutyPeriodType { get; set; }
    public DutyStatus Status { get; set; } = DutyStatus.Unreported;
    public TaxPaymentStatus PaymentStatus { get; set; } = TaxPaymentStatus.Unpaid;
    public DateTime? DueDate { get; set; }

    [Column(TypeName = "decimal(18,0)")]
    public decimal TaxPayable { get; set; } = 0;

    [MaxLength(20)]
    public string Period { get; set; } = string.Empty;

    [MaxLength(500)]
    public string? Note { get; set; }
    
    //navigation properties
    [ForeignKey(nameof(TaxDutyId))]
    public TaxDuty TaxDuty { get; set; } = null!;
    [ForeignKey(nameof(OrganizationId))]
    public Organization Organization { get; set; } = null!;

    public ICollection<TaxDutyXmlDoc> XmlDocs { get; set; } = [];
}

public enum DutyPeriodType
{
    Monthly,
    Quarterly,
    Annual,
    Other
}

public enum DutyStatus
{
    Unreported,
    Submitted,
    Accepted,
    Rejected
}

public enum TaxPaymentStatus
{
    Unpaid,
    Paid,
    NoAmountDue
}