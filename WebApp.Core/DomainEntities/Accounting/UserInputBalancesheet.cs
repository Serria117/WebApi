using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WebApp.Core.DomainEntities.Accounting;

[Table(name: "ACC_UserInputBalancesheet")]
public class UserInputBalancesheet : BaseEntityAuditable<int>
{
    public int Year { get; set; }
    public Guid OrganizationId { get; set; }
    public int Regulation { get; set; }
    public ICollection<BalanceEntry> AccountBalances { get; set; } = [];

    [ForeignKey(nameof(FinancialReportWork))]
    public int? FinancialReportWorkId { get; set; }
    public FinancialReportWork? FinancialReportWork { get; set; }
}