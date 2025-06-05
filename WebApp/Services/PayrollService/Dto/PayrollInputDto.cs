using System.ComponentModel.DataAnnotations;

namespace WebApp.Services.PayrollService.Dto;

public class PayrollInputCreateDto
{
    [MaxLength(255)]
    public string? Value { get; set; }

    public long PayrollRecordId { get; set; }
    public int PayrollInputTypeId { get; set; }
}

public class PayrollInputDisplayDto
{
    public int Id { get; set; }
    public string? Value { get; set; }
    public long PayrollRecordId { get; set; }
    public int PayrollInputTypeId { get; set; }
    public string? TypeName { get; set; }
}