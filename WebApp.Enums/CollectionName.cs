using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WebApp.Enums;

public struct CollectionName
{
    public const string User = "users";
    public const string Invoice = "invoices";
    public const string SoldInvoice = "soldInvoices";
    public const string Client = "client";
    public const string RevokedToken = "tokens";
}
