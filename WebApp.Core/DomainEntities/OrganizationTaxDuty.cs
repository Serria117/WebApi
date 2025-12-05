using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;
using NanoidDotNet;
using WebApp.Core.DomainEntities.Accounting.TaxDeclarations;

namespace WebApp.Core.DomainEntities;
[Table("Organizations_TaxDuties")]
[Index(nameof(OrganizationId), Name = "IX_Organizations_TaxDuties_OrganziationId")]
[Index(nameof(TaxReportDutyId), Name = "IX_Organizations_TaxDuties_TaxReportDutyId")]
public class OrganizationTaxDuty : BaseEntity<string>
{
    [MaxLength(30)]
    public new string Id { get; set; } = Nanoid.Generate(Nanoid.Alphabets.Default, 12);

    public Guid OrganizationId { get; set; }
    public int TaxReportDutyId { get; set; }
    
    public DutyPeriodType DutyPeriodType { get; set; }
}