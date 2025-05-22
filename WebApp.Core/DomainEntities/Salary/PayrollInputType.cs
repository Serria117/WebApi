using System.ComponentModel.DataAnnotations;
using WebApp.Enums;
using WebApp.Enums.Payroll;

namespace WebApp.Core.DomainEntities.Salary;

public class PayrollInputType : BaseEntity<int>
{
    [MaxLength(255)]
    public string Name { get; set; } = string.Empty;

    [MaxLength(255)]
    public string? Description { get; set; }

    /// <summary>
    /// Specifies the type of data associated with the payroll input type.
    /// </summary>
    /// <remarks>
    /// This property determines the format or data classification, such as numeric values, textual information, or dates.
    /// It enhances the ability to validate and categorize input data based on its nature.
    /// </remarks>
    [MaxLength(50)]
    public string DataType { get; set; } = InputDataType.Currency;

    /// <summary>
    /// Represents the unit of measurement associated with the payroll input type.
    /// </summary>
    /// <remarks>
    /// This property can be used to define the unit relevant to the input type, such as hours, days, or currency.
    /// It allows for better context and understanding of the associated numeric or textual data.
    /// </remarks>
    [MaxLength(50)]
    public string? Unit { get; set; }
    /// <summary>
    /// Specifies the organization ID associated with the payroll input type.
    /// If null, the input type is not tied to any specific organization and has global applicability.
    /// </summary>
    public Guid? OrganizationId { get; set; } = null;
    
    public ICollection<PayrollInput> PayrollInputs { get; set; } = null!;
    public ICollection<PayrollComponentType> PayrollComponentTypes { get; set; } = null!;
}