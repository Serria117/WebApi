using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WebApp.Core.DomainEntities.Accounting;
public class FinancialReportWork : BaseEntityAuditable<int>
{
    public int Regulation { get; set; }
    public int Year { get; set; }
    public Guid? OrganizationId { get; set; }
    [MaxLength(255)]
    public string Name { get; set; } = string.Empty;
    [MaxLength(500)]
    public string? Note { get; set; }
    public DateTime BeginDate { get; set; }
    public DateTime EndDate { get; set; }
    public DateTime ReportDate { get; set; }
    [MaxLength(5)]
    public string FirstFiscalDate { get; set; } = string.Empty;
    [MaxLength(255)]
    public string TaxAgencyName { get; set; } = string.Empty;
    [MaxLength(10)]
    public string TaxAgencyCode { get; set; } = string.Empty;

    public string? Status { get; set; } = ReportStatus.New;

    public Organization? Organization { get; set; }
    public UserInputBalancesheet? UserInput { get; set; }
    public Balancesheet? Balancesheet { get; set; }

}

public struct ReportStatus
{
    public const string New = "New";
    public const string Pending = "Pending";
    public const string Finish = "Finished";
}
