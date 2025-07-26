namespace WebApp.Services.PayrollService.Dto;

public class AllowanceTypeCreate
{
    public string Name { get; set; } = string.Empty;
    public string? Code { get; set; }
    public bool IsTaxable { get; set; } = false;
    public bool IsInsurance { get; set; } = false;
    public bool IsOrganization { get; set; } = false;
    public decimal MaxAmount { get; set; } = 999_999_999M;
    public decimal DefaultAmount { get; set; } = 0;
    public string? Unit { get; set; }
    
}

public class AllowanceTypeDisplay    
{
    public long Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Code { get; set; }
    public bool IsTaxable { get; set; } = false;
    public bool IsInsurance { get; set; } = false;
    public Guid? OrganizationId { get; set; }
    public decimal MaxAmount { get; set; }
    public decimal DefaultAmount { get; set; }

    public string? Unit { get; set; }
}