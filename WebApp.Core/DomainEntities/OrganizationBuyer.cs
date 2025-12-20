using System.ComponentModel.DataAnnotations;

namespace WebApp.Core.DomainEntities;

/// <summary>
/// Represent the buyer of the organization
/// </summary>
public class OrganizationBuyer : BaseEntity<string>
{
    [MaxLength(26)]
    public new string Id { get; set; } = Ulid.NewUlid().ToString();

    [MaxLength(255)]
    public string Name { get; set; } = string.Empty;

    [MaxLength(20)]
    public string TaxId { get; set; } = string.Empty;
}