using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace WebApp.Core.DomainEntities.Accounting;

[Index(nameof(AccountNumber), IsUnique = true)]
public class Account : BaseEntity<int>
{
    [MaxLength(10)]
    public string AccountNumber { get; set; } = string.Empty;

    [MaxLength(255)]
    public string Name { get; set; } = string.Empty;

    public int? Parent { get; set; }

    public int Grade { get; set; }

    [MaxLength(5)]
    public string? B01TS { get; set; }

    [MaxLength(5)]
    public string? B01NV { get; set; }

    [MaxLength(5)]
    public string? B02 { get; set; }
}