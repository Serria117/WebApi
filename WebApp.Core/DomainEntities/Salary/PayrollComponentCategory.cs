using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;

namespace WebApp.Core.DomainEntities.Salary;

public class PayrollComponentCategory : BaseEntity<int>
{
    [MaxLength(255)]
    public string Name { get; set; } = string.Empty;

    [MaxLength(255)]
    public string? Description { get; set; }

    public int Order { get; set; } = 0;

    public ICollection<PayrollComponentType> PayrollComponentTypes { get; set; } = null!;
}