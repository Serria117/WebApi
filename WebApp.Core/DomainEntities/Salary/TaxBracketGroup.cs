using System.ComponentModel.DataAnnotations;

namespace WebApp.Core.DomainEntities.Salary;

public class TaxBracketGroup : BaseEntityAuditable<int>
{
    [MaxLength(100)]
    public string Name { get; set; }  = string.Empty;
    [MaxLength(255)]
    public string? Description { get; set; }
    public DateTime EffectiveDate { get; set; }
    public DateTime? EndDate { get; set; } // nullable for future updates
    /// <summary>
    /// Navigational property for the collection of income tax brackets associated with this group.
    /// </summary>
    public ICollection<IncomeTaxBracket> IncomeTaxBrackets { get; set; } = new List<IncomeTaxBracket>();
}