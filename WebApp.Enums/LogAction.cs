using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WebApp.Enums;
public struct LogAction
{
    public const string Create = "Tạo";
    public const string Read = "Xem/Đọc";
    public const string Update = "Cập nhật";
    public const string Delete = "Xóa";
    public const string Import = "Nhập khẩu";
    public const string Export = "Xuất khẩu";
    public const string Download = "Tải xuống";
    public const string Upload = "Tải lên";
    public const string Login = "Đăng nhập";
    public const string Query = "Truy vấn";
    public const string Sync = "Đồng bộ";

}
