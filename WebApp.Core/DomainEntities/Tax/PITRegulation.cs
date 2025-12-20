using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;
using System.Text.Json.Serialization;

namespace WebApp.Core.DomainEntities.Tax;

[Table("TAX_PIT_Regulation")]
public class PITRegulation : BaseEntity<Guid>
{
    public new Guid Id { get; set; } = Guid.CreateVersion7();

    [MaxLength(200)]
    public string Name { get; set; } = string.Empty;

    [MaxLength(500)]
    public string? Regulation { get; set; } = null;
    public List<PITBracket> MonthBrackets { get; set; } = [];
    public List<PITBracket> YearBrackets { get; set; } = [];
    public DateTime EffectiveDate { get; set; }
    public DateTime? EndDate { get; set; }

    [Column(TypeName = "decimal(18,0)")]
    public decimal SelfDeductionAmount { get; set; }
	[Column(TypeName = "decimal(18,0)")]
	public decimal DependantDeductionAmount { get; set; }
}

public class PITBracket
{
	[Column(TypeName = "decimal(19,0)")]
	public decimal? Limit { get; set; }
    [Column(TypeName = "decimal(5,2)")]
	public decimal Rate { get; set; }
}
