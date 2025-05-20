using WebApp.Enums;

namespace WebApp.Core.DomainEntities.Salary;

public class PayrollInputType: BaseEntity<int>
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }

    /// <summary>
    /// Specifies the type of data associated with the payroll input type.
    /// </summary>
    /// <remarks>
    /// This property determines the format or data classification, such as numeric values, textual information, or dates.
    /// It enhances the ability to validate and categorize input data based on its nature.
    /// </remarks>
    public string DataType { get; set; } = PayrollDataType.NUMBER;

    /// <summary>
    /// Represents the unit of measurement associated with the payroll input type.
    /// </summary>
    /// <remarks>
    /// This property can be used to define the unit relevant to the input type, such as hours, days, or currency.
    /// It allows for better context and understanding of the associated numeric or textual data.
    /// </remarks>
    public string? Unit { get; set; }
}