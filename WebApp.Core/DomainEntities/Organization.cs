using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Net.NetworkInformation;
using WebApp.Enums;

namespace WebApp.Core.DomainEntities;

/// <summary>
/// Represents an organization entity in the system.
/// This class contains properties that define the organization, such as its name, tax information,
/// contact details, and relationships with other entities like users and tax offices.
/// </summary>
[Index(nameof(UnsignName))]
[Index(nameof(TaxId))]
public class Organization : BaseEntityAuditable<Guid>
{
	[MaxLength(500), MinLength(3), Required]
	public string FullName { get; set; } = string.Empty;

	[MaxLength(500), MinLength(3)]
	public string UnsignName { get; set; } = string.Empty;

	[MaxLength(50), MinLength(3)]
	public string? ShortName { get; set; }

	public CapitalOwnershipType? CapitalOwnershipType { get; set; }

	[MaxLength(1000)]
	public string? Address { get; set; }

	[MaxLength(1000)]
	public string? ContactAddress { get; set; }

	[MaxLength(20), MinLength(3)]
	public string TaxId { get; set; } = string.Empty;

	public List<string> Emails { get; set; } = [];

	public List<string> Phones { get; set; } = [];

	[MaxLength(50)]
	public string? TaxIdPwd { get; set; }

	[MaxLength(50)]
	public string? InvoicePwd { get; set; }

	[MaxLength(50)]
	public string? PinCode { get; set; }

	public TaxOffice? TaxOffice { get; set; }

	public int? TaxOffice2Id { get; set; }

	[MaxLength(255)]
	public string? Representative { get; set; }

	[ForeignKey(nameof(TaxOffice2Id))]
	public TaxOffice2? TaxOffice2 { get; set; }

    public int? DistrictId { get; set; }

	[ForeignKey(nameof(DistrictId))]
    public District? District { get; set; }

    public OrganizationStatus Status { get; set; } = OrganizationStatus.Active;

	[MaxLength(5)]
	public string? FiscalYearFirstDate { get; set; } = "01/01";

	public virtual ISet<User> Users { get; set; } = new HashSet<User>();

	[MaxLength(3)]
	public string? TypeOfVatPeriod { get; set; } = "Q";

	public HashSet<OrganizationLoginInfo> OrganizationLoginInfos { get; set; } = [];

	public ICollection<Contract> Contracts { get; set; } = [];
}

public enum OrganizationStatus
{
	Active,
	Suspended, 
	Terminated
}