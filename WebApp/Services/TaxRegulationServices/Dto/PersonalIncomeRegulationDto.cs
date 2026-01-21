using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using WebApp.Core.DomainEntities.Tax;
using WebApp.Enums;
using WebApp.Utils;

namespace WebApp.Services.TaxRegulationServices.Dto;

public class PersonalIncomeRegulationDto
{
    [MaxLength(255)]
    public string Name { get; set => field = value.TrimSpace(); } = string.Empty;

    public List<PITBracket> MonthBrackets { get; set; } = [];
    public List<PITBracket> YearBrackets { get; set; } = [];
    public List<Deduction> Deductions { get; set; } = [];
    public DateTime EffectiveDate { get; set; }
    public DateTime? EndDate { get; set; }
    public int? Order { get; set; }
    public string? Description { get; set; }

    public void ValidBracketStep()
    {
        foreach (var b in MonthBrackets)
        {
            b.Limit = b.Limit.HasValue ? Math.Max(0, b.Limit.Value) : SystemBoundary.MaxSafeInteger;
        }
        foreach (var b in YearBrackets)
        {
            b.Limit = b.Limit.HasValue ? Math.Max(0, b.Limit.Value) : SystemBoundary.MaxSafeInteger;
        }
    }
}

public class PersonalIncomeRegulationDisplayDto : PersonalIncomeRegulationDto
{
    public Guid Id { get; set; }
}