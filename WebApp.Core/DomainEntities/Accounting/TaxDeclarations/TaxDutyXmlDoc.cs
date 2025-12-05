using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace WebApp.Core.DomainEntities.Accounting.TaxDeclarations;

[Table("TaxDutyXmlDocs")][Index(nameof(TaxId))]
public class TaxDutyXmlDoc : BaseEntityAuditable<string>
{
    [MaxLength(26)]
    public new string Id { get; set; } = Ulid.NewUlid().ToString();

    [MaxLength(255)]
    public string FileName { get; set; } = string.Empty;

    [MaxLength(20)]
    public string? TaxId { get; set; }

    [Column(TypeName = "NVARCHAR(MAX)")]
    public string Content { get; set; } = string.Empty;

    public string? TaxDutyRecordId { get; set; }

    [MaxLength(26)]
    public string TemplateId { get; set; } = null!;

    [Range(0, 9999)]
    public int SubmissionCount { get; set; } = 0;

    /// <summary>
    /// The code of the tax transaction proviđed by the tax authority after submission.
    /// <br/>User should enter this code after submission for further reference.
    /// </summary>
    [MaxLength(255)]
    public string? TaxTransactionCode { get; set; }

    public TaxResponseStatus TaxResponseStatus { get; set; }

    public DateTime? IssueDate { get; set; }

    [MaxLength(10)]
    public string? Period 
    { 
        get;
        set
        {
            field = value;
            Year = GetYearFromPeriod();
        } } = string.Empty;

    public int? Year {get; set;}

    [Column(TypeName = "NVARCHAR(2)")]
    public string? PeriodType { get; set; } = string.Empty;
    
    // Navigation Properties:
    [ForeignKey(nameof(TaxDutyRecordId))]
    public TaxDutyRecord? TaxDutyRecord { get; set; }

    [ForeignKey(nameof(TemplateId))]
    public TaxDeclarationTemplate TaxDeclarationTemplate { get; set; } = null!;

    private int? GetYearFromPeriod()
    {
        if (string.IsNullOrEmpty(Period))
            return null;

        if (Period.Length == 4)
        {
            return int.TryParse(Period, out var year1) && year1 is >= 1900 and <= 2100 ? year1 : null;
        }

        if (!Period.Contains('/')) return null;
        
        var parts = Period.Split('/');
        if (parts.Length < 2)
            return null;
    
        if (int.TryParse(parts[1], out var year) && year is >= 1900 and <= 2100)
            return year;

        return null;
    }
}

public enum TaxResponseStatus
{
    NotSubmitted = 0,
    Submitted,
    Accepted,
    Rejected
}