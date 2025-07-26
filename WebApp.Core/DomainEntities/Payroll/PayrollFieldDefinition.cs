using System.ComponentModel.DataAnnotations.Schema;

namespace WebApp.Core.DomainEntities.Payroll;

/// <summary>
/// Define the custom field for the payroll.
/// </summary>
[Table("PR_PayrollFieldDefinition")]
public class PayrollFieldDefinition : BaseEntity<long>
{
    public string Name { get; set; } = string.Empty;
    public string DataType { get; set; } = FieldDataType.Number;
    public int Order { get; set; }
    public Guid? OrganizationId { get; set; } //null if field is used globally.
    
    [ForeignKey(nameof(OrganizationId))]
    public Organization? Organization { get; set; }
}

public struct FieldDataType
{
    public const string Number = "NUMBER";
    public const string Date = "DATE";
    public const string String = "STRING";
}