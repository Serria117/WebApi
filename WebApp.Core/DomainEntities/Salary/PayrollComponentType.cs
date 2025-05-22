using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using WebApp.Enums;
using WebApp.Enums.Payroll;

namespace WebApp.Core.DomainEntities.Salary;

public class PayrollComponentType : BaseEntity<int>
{
    [MaxLength(255)]
    public required string Name { get; set; }

    [MaxLength(255)]
    public string? Description { get; set; }

    /// <summary>
    /// Specifies the type of data source that the component will receive,
    /// such as Fixed, Calculated, or Input.
    /// </summary>
    /// <remarks>Fixed: Value is fixed from an enum or database table. 
    /// Calculcated: Value is calculated from other values.
    /// Input: Value is input directly by user.</remarks>
    [MaxLength(50)]
    public string? DataSourceType { get; set; } = ComponentType.Fixed;

    public Guid? OrganizationId { get; set; }

    [ForeignKey(nameof(OrganizationId))]
    public Organization? Organization { get; set; }

    public int PayrollComponentCategoryId { get; set; }

    [ForeignKey(nameof(PayrollComponentCategoryId))]
    public PayrollComponentCategory PayrollComponentCategory { get; set; } = null!;

    public bool IsActive { get; set; } = true;
    public bool IsDeductible { get; set; } = false;
    public bool IsTaxable { get; set; } = true;

    // Nullable if ComponentType is not Input
    public int? InputTypeId { get; set; }

    [ForeignKey("InputTypeId")]
    public PayrollInputType? InputType { get; set; }

    /// <summary>
    /// DataType is used to specify the type of data associated with the payroll component.
    /// </summary>
    [MaxLength(50)]
    public string? DataType { get; set; } = InputDataType.Currency;
    
    public ICollection<PayrollItem> PayrollItems { get; set; } = null!;
}