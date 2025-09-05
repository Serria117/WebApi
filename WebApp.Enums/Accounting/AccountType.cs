using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WebApp.Enums.Accounting;
public enum AccountType
{
    Asset = 0,          // Tài sản
    Liability = 1,      // Nợ phải trả
    Equity = 2,         // Vốn chủ sở hữu
    Revenue = 3,        // Doanh thu
    Expense = 4,        // Chi phí
    Other = 5
}
