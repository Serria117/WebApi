using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WebApp.Core.DomainEntities.Tax;

[Table("TAX_PIT_Regulation"), Index(nameof(Name))]
public class PITRegulation : BaseEntity<Guid>
{
	public new Guid Id { get; set; } = Guid.CreateVersion7();

	[MaxLength(200)]
	public string Name { get; set; } = string.Empty;

	[MaxLength(500)]
	public string? Description { get; set; } = null;
	public List<PITBracket> MonthBrackets { get; set; } = [];
	public List<PITBracket> YearBrackets { get; set; } = [];
	public List<Deduction> Deductions { get; set; } = [];
	public DateTime EffectiveDate { get; set; }
	public DateTime? EndDate { get; set; }

	public int? Order { get; set; }
}

public class PITBracket
{
	[Column(TypeName = "decimal(18,4)")]
	public decimal? Limit { get; set; }
	
	[Column(TypeName = "decimal(18,4)")]
	public decimal Rate { get; set; }

	[Column(TypeName = "decimal(18,4)")]
	public decimal ProgressiveAmount { get; set; } = 0;// the amount of tax for the previous bracket
}

public class Deduction
{
	[Column(TypeName = "decimal(18,4)")]
	public decimal Amount { get; set; }
	public DeductionType Type { get; set; }
}

public enum DeductionType
{
	Self,
	Dependant
}