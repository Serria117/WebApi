using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WebApp.Core.DomainEntities;
public class InvoiceServiceToken : BaseEntityAuditable<string>
{
    public new string Id { get; set; } = string.Empty;
    public string TaxId { get; set; } = string.Empty;
    public string? Token { get; set; }
}
