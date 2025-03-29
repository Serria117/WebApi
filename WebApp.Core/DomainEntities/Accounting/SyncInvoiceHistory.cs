using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;

namespace WebApp.Core.DomainEntities.Accounting;

[Index(nameof(TaxId))]
public class SyncInvoiceHistory : BaseEntityAuditable<long>
{
    [MaxLength(50)]
    public string Username { get; set; } = string.Empty;

    [MaxLength(20)]
    public string TaxId { get; set; } = string.Empty;

    public SyncType SyncType { get; set; }
    public int TotalFound { get; set; }
    public int TotalSuccess { get; set; }
    public DateTime FromDate { get; set; }
    public DateTime ToDateTime { get; set; }
}

public enum SyncType
{
    Sold = 0,
    Purchased = 1
}