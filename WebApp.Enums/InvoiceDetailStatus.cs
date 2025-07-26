using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WebApp.Enums;
public enum InvoiceDetailStatus
{
    Success = 200,
    Failed = 400,
    Undeserializable = 99,
    TooManyRequest = 429
}
