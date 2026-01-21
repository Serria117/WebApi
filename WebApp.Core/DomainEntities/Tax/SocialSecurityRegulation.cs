using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace WebApp.Core.DomainEntities.Tax;

[Table("TAX_SocialSecurityRegulations")]
public class SocialSecurityRegulation : BaseEntity<Guid>
{
	public new Guid Id { get; set; } = Guid.CreateVersion7();

	[MaxLength(255)]
	public string Name { get; set; } = string.Empty;
	public DateTime EffectiveDate { get; set; }
	public DateTime? EndDate { get; set; }
	public List<SocialSecurityRate> EmployeeRate { get; set; } = [];
	public List<SocialSecurityRate> EmployerRate { get; set; } = [];
	public int? Order { get; set; }
}

public class SocialSecurityRate
{
	public SocialSecurityType Type { get; set; }
	public SocialSecurityBearer Bearer { get; set; }

	[Column(TypeName = "decimal(18,4)")]
	public decimal Rate { get; set; }
}

public enum SocialSecurityType
{
	SocialInsurance,
	HealthInsurance,
	UnemploymentInsurance,
	UnionFee
}

public enum SocialSecurityBearer
{
	Employer,
	Employee
}