using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WebApp.Enums;
public struct CapitalOwner
{
    public const string PrivateCompany = "Doanh nghiệp tư nhân";
    public const string LimitedCompany = "Công ty TNHH";
    public const string LimitedCompanySingleOwner = "Công ty TNHH một thành viên";
    public const string JointStockCompany = "Công ty cổ phần";
    public const string HouseholdBusiness = "Hộ kinh doanh";
    public const string Cooperative = "Hợp tác xã";
    public const string Fdi = "Công ty nước ngoài";
    public const string Other = "Khác";
}

public enum CapitalOwnershipType
{
    PrivateCompany = 1,
    LimitedCompany = 2,
    LimitedCompanySingleOwner = 3,
    JointStockCompany = 4,
    HouseholdBusiness = 5,
    Cooperative = 6,
    Fdi = 7,
    Other = 8
}