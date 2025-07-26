using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace WebApp.Core.DomainEntities.Accounting;

[Index(nameof(OrganizationId))]
[Table("INV_InvoiceHistory")]
public class InvoiceHistory : BaseEntityAuditable<long>
{
    public Guid? OrganizationId { get; set; }
    public SyncType SyncType { get; set; }
    public int TotalFound { get; set; }
    public int TotalSuccess { get; set; }
    public DateTime FromDate { get; set; }
    public DateTime ToDate { get; set; }
    public bool? IsRetried { get; set; } = false;
    public int RetryTime { get; set; } = 0;
    public bool Completed { get; set; } = true;
}

public enum SyncType
{
    Sold = 0,
    Purchased = 1
}