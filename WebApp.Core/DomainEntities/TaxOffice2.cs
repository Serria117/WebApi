using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace WebApp.Core.DomainEntities;
[Table(name: "TaxOffices_2")]
[Index(nameof(Code))]
public class TaxOffice2 : BaseEntity<int>
{
    [MaxLength(255)]
    public string FullName { get; set; } = string.Empty;

    [MaxLength(255)]
    public string? ShortName { get; set; }

    [MaxLength(10)]
    public string Code { get; set; } = string.Empty;

    public int? ProvinceId { get; set; }

    [ForeignKey(nameof(ProvinceId))]
    public Province? Province { get; set; }
}
