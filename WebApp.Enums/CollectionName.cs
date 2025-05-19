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
    public const string Organization = "orgId";
    public const string BlacklistedToken = "tokens";
    public const string RefreshToken = "refreshTokens";
    public const string LockedUsers = "lockedUsers";
    
}
