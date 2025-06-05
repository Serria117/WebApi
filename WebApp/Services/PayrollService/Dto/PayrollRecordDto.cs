using WebApp.Core.DomainEntities.Salary;

namespace WebApp.Services.PayrollService.Dto;

public class PayrollRecordCreateDto
{
    public List<Guid> EmployeeId { get; set; } = [];
    public decimal TotalGrossPay { get; set; }
    public decimal TotalDeduction { get; set; }
    public decimal TotalNetPay { get; set; }
    public int PeriodId { get; set; }
    
    public bool IsClosed { get; set; } = false;

    public TaxType TaxType { get; set; } = TaxType.ResidentProgressive;
}

public class PayrollRecordDisplayDto
{
    public long Id { get; set; }
    public Guid EmployeeId { get; set; }
    public string? EmployeeName { get; set; }
    public int PeriodId { get; set; }
    public string PeriodName { get; set; } = string.Empty;
    public bool IsClosed { get; set; }
    public decimal TotalGrossPay { get; set; }
    public decimal TotalDeduction { get; set; }
    public decimal TotalNetPay { get; set; }
    
    public TaxType TaxType { get; set; } = TaxType.ResidentProgressive;
}
