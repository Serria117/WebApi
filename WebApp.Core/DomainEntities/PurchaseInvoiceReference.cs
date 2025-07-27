using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace WebApp.Core.DomainEntities;
[Table(name: "INV_purchaseInvoiceRef")]
[Index(nameof(Nbmst), nameof(Khhdon), nameof(Shdon))]
public class PurchaseInvoiceReference : BaseEntityAuditable<string>
{
    public int? Shdon { get; set; }
    public int? Khmshdon { get; set; }

    [Column(TypeName = "nvarchar(50)")]
    public string? Khhdon { get; set; }
    public DateTime? Tdlap { get; set; }

    [Column(TypeName = "nvarchar(20)")]
    public string? Nmmst { get; set; }

    [Column(TypeName = "nvarchar(20)")]
    public string? Nbmst { get; set; }

    [Column(TypeName = "nvarchar(MAX)")]
    public string? Nbten { get; set; }

    [Column(TypeName = "nvarchar(100)")]
    public string? InvoiceId { get; set; }

    public bool IsDetailRetrived { get; set; } = false;
}
