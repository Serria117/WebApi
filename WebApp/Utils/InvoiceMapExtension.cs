using System.Xml.Linq;
using WebApp.Mongo.DeserializedModel;
using WebApp.Mongo.DocumentModel;
using WebApp.Mongo.DocumentModel.SoldInvoiceDetails;
using WebApp.Services.CommonService;
using WebApp.Services.InvoiceService.dto;
using WebApp.Services.RestService.Dto;
using Doc_Hdhhdvu = WebApp.Mongo.DocumentModel.Hdhhdvu;
using Doc_Thttltsuat = WebApp.Mongo.DocumentModel.Thttltsuat;

namespace WebApp.Utils;

public static class InvoiceMapExtension
{
    public static InvoiceDisplayDto ToDisplayModel(this InvoiceDetailDoc doc)
    {
        return new InvoiceDisplayDto
        {
            Id = doc.Id ?? string.Empty,
            VerifyCode = doc.Mhdon,
            StatusNumber = doc.Tthai,
            BuyerName = doc.Nmten ?? string.Empty,
            BuyerTaxCode = doc.Nmmst ?? string.Empty,
            BuyerAddress = doc.Nmdchi,
            SellerName = doc.Nbten ?? string.Empty,
            SellerTaxCode = doc.Nbmst ?? string.Empty,
            SellerAddress = doc.Nbdchi,
            InvoiceNotation = doc.Khhdon ?? string.Empty,
            InvoiceGroupNotation = doc.Khmshdon,
            InvoiceNumber = doc.Shdon?.ToString(),
            TotalPrice = doc.Tgtcthue,
            Vat = doc.Tgtthue,
            TotalPriceVat = doc.Tgtttbso,
            TotalInWord = doc.Tgtttbchu,
            ChietKhau = doc.Ttcktmai,
            Phi = doc.Tgtphi,
            CreationDate = doc.Tdlap?.ToLocalTime(),
            SigningDate = doc.Nky?.ToLocalTime(),
            IssueDate = doc.Ncma?.ToLocalTime(),
            Risk = doc.Risk ?? false,
            Status = doc.Tthai switch
            {
                1 => "Hóa đơn mới",
                2 => "Hóa đơn thay thế",
                3 => "Hóa đơn điều chỉnh",
                4 => "Hóa đơn bị thay thế",
                5 => "Hóa đơn bị điều chỉnh",
                6 => "Hóa đơn hủy",
                _ => string.Empty
            },
            InvoiceType = doc.Ttxly switch
            {
                5 => "Hóa đơn cấp mã",
                6 => "Hóa đơn không cấp mã",
                8 => "Hóa đơn từ máy tính tiền",
                _ => null
            },
            InvoiceTypeNumber = doc.Ttxly,
            GoodsDetail = doc.Hdhhdvu == null ? [] : doc.Hdhhdvu.Select(g => new Goods
            {
                Name = g.Ten,
                UnitCount = g.Dvtinh,
                UnitPrice = g.Dgia,
                Quantity = g.Sluong,
                PreTaxPrice = g.Thtien,
                Rate = g.Tsuat,
                Discount = g.Stckhau,
                Tax = g.Thtien is null || g.Tsuat is null
                    ? 0
                    : Math.Round(g.Thtien.Value * g.Tsuat.Value, 0),
                TaxType = g.Ltsuat
            }).ToList(),
            SellerSignature = doc.Nbcks,
        };
    }

    public static InvoiceDisplayDto ToDisplayModel(this SoldInvoiceDetail doc)
    {
        return new InvoiceDisplayDto
        {
            Id = doc.Id ?? string.Empty,
            StatusNumber = doc.Tthai,
            BuyerName = doc.Nmten ?? string.Empty,
            BuyerNameIndividual = doc.Nmtnmua,
            BuyerTaxCode = doc.Nmmst ?? string.Empty,
            SellerName = doc.Nbten ?? string.Empty,
            SellerTaxCode = doc.Nbmst ?? string.Empty,
            InvoiceNotation = doc.Khhdon ?? string.Empty,
            InvoiceGroupNotation = doc.Khmshdon,
            InvoiceNumber = doc.Shdon.ToString(),
            TotalPrice = doc.Tgtcthue,
            Vat = doc.Tgtthue,
            TotalPriceVat = doc.Tgtttbso,
            CreationDate = doc.Tdlap?.ToLocalTime(),
            SigningDate = doc.Nky?.ToLocalTime(),
            IssueDate = doc.Ncma?.ToLocalTime(),
            Status = doc.Tthai switch
            {
                1 => "Hóa đơn mới",
                2 => "Hóa đơn thay thế",
                3 => "Hóa đơn điều chỉnh",
                4 => "Hóa đơn bị thay thế",
                5 => "Hóa đơn bị điều chỉnh",
                6 => "Hóa đơn hủy",
                _ => string.Empty
            },
            InvoiceType = doc.Ttxly switch
            {
                5 => "Hóa đơn cấp mã",
                6 => "Hóa đơn không cấp mã",
                8 => "Hóa đơn từ máy tính tiền",
                _ => string.Empty
            },
            InvoiceTypeNumber = doc.Ttxly,
            GoodsDetail = doc.Hdhhdvu == null ? [] : doc.Hdhhdvu.Select(h => new Goods
            {
                Name = h.Ten,
                UnitCount = h.Dvtinh,
                UnitPrice = h.Dgia,
                Quantity = h.Sluong,
                PreTaxPrice = h.Thtien,
                Rate = h.Tsuat,
                Discount = h.Stckhau,
                Tax = h is { Thtien: not null, Tsuat: not null } ? Math.Round(h.Thtien.Value * h.Tsuat.Value, 0) : 0,
                TaxType = h.Ltsuat
            }).ToList(),
            SellerSignature = doc.Nbcks,
            VerifyCode = doc.Mhdon,
            TotalInWord = doc.Tgtttbchu,
            BuyerAddress = doc.Nmdchi,
            SellerAddress = doc.Nbdchi
        };
    }

    public static InvoiceDisplayDto ToDisplayModel(this InvoiceModel inv)
    {
        return new InvoiceDisplayDto
        {
            Id = inv.Id ?? string.Empty,
            StatusNumber = inv.Tthai,
            BuyerName = inv.Nmten ?? string.Empty,
            BuyerTaxCode = inv.Nmmst ?? string.Empty,
            SellerName = inv.Nbten ?? string.Empty,
            SellerTaxCode = inv.Nbmst ?? string.Empty,
            InvoiceNotation = inv.Khhdon ?? string.Empty,
            InvoiceGroupNotation = inv.Khmshdon,
            InvoiceNumber = inv.Shdon?.ToString(),
            TotalPrice = inv.Tgtcthue,
            Vat = inv.Tgtthue,
            TotalPriceVat = inv.Tgtttbso,
            CreationDate = inv.Tdlap?.ToLocalTime(),
            SigningDate = inv.Nky?.ToLocalTime(),
            IssueDate = inv.Ncma?.ToLocalTime(),
            Status = inv.Tthai switch
            {
                1 => "Hóa đơn mới",
                2 => "Hóa đơn thay thế",
                3 => "Hóa đơn điều chỉnh",
                4 => "Hóa đơn bị thay thế",
                5 => "Hóa đơn bị điều chỉnh",
                6 => "Hóa đơn hủy",
                _ => string.Empty
            },
            InvoiceType = inv.Ttxly switch
            {
                5 => "Hóa đơn cấp mã",
                6 => "Hóa đơn không cấp mã",
                8 => "Hóa đơn từ máy tính tiền",
                _ => string.Empty
            },
            InvoiceTypeNumber = inv.Ttxly,
            BuyerAddress = inv.Nmdchi,
            SellerAddress = inv.Nbdchi,
            TotalInWord = inv.Tgtttbchu,
            VerifyCode = inv.Mhdon
        };
    }

    public static InvoiceDisplayDto ToDisplayModel(this SoldInvoiceDoc doc)
    {
        return new InvoiceDisplayDto()
        {
            Id = doc.Id ?? string.Empty,
            VerifyCode = doc.Mhdon,
            StatusNumber = doc.Tthai,
            BuyerName = doc.Nmten ?? string.Empty,
            BuyerTaxCode = doc.Nmmst ?? string.Empty,
            BuyerAddress = doc.Nmdchi,
            SellerName = doc.Nbten ?? string.Empty,
            SellerTaxCode = doc.Nbmst ?? string.Empty,
            SellerAddress = doc.Nbdchi,
            InvoiceNotation = doc.Khhdon ?? string.Empty,
            InvoiceGroupNotation = doc.Khmshdon,
            InvoiceNumber = doc.Shdon.ToString(),
            TotalPrice = doc.Tgtcthue,
            Vat = doc.Tgtthue,
            TotalPriceVat = doc.Tgtttbso,
            TotalInWord = doc.Tgtttbchu,
            CreationDate = doc.Tdlap?.ToLocalTime(),
            SigningDate = doc.Nky?.ToLocalTime(),
            IssueDate = doc.Ncma?.ToLocalTime(),
            Status = doc.Tthai switch
            {
                1 => "Hóa đơn mới",
                2 => "Hóa đơn thay thế",
                3 => "Hóa đơn điều chỉnh",
                4 => "Hóa đơn bị thay thế",
                5 => "Hóa đơn bị điều chỉnh",
                6 => "Hóa đơn hủy",
                _ => string.Empty
            },
            InvoiceType = doc.Ttxly switch
            {
                5 => "Hóa đơn cấp mã",
                6 => "Hóa đơn không cấp mã",
                8 => "Hóa đơn từ máy tính tiền",
                _ => string.Empty
            },
            InvoiceTypeNumber = doc.Ttxly,
        };
    }

    public static InvoiceDetailDoc ToInvoiceModel(this XDocument doc)
    {
        return new InvoiceDetailDoc
        {
            Khhdon = doc.GetXmlNodeValue("KHHDon"),
            Khmshdon = doc.GetXmlNodeValue("KHMShDon").ToInt(),
            Shdon = doc.GetXmlNodeValue("SHDon").ToInt(),
            Tdlap = doc.GetXmlNodeValue("NLap")?.ToDateTime(),
            Nky = doc.GetElementValueByPath("DSCKS", "NBan", "Signature", "Object", "SignatureProperties",
                                            "SignatureProperty", "SigningTime")?.ToDateTime(),
            Nbcks = doc.GetElementValueByPath("DSCKS", "NBan", "Signature", "KeyInfo", "X509Data",
                                              "X509SubjectName"),
            Mhdon = doc.GetElementValueByPath("MCCQT"),
            Htttoan = doc.GetXmlNodeValue("Htttoan") switch
            {
                "CK/TM" => 9,
                "CK" => 1,
                "TM" => 2,
                _ => 0
            },
            Ttxly = GetInvoiceType(doc),
            Dvtte = doc.GetXmlNodeValue("DVTTe"),
            Tgia = doc.GetXmlNodeValue("TGia").ToInt(),
            Msttcgp = doc.GetXmlNodeValue("Msttcgp"),
            Nbten = doc.GetElementValueByPath("DLHDon", "NDHDon", "NBan", "Ten"),
            Nbmst = doc.GetElementValueByPath("DLHDon", "NDHDon", "NBan", "MST"),
            Nbdchi = doc.GetElementValueByPath("DLHDon", "NDHDon", "NBan", "DChi"),
            Nbsdthoai = doc.GetElementValueByPath("DLHDon", "NDHDon", "NBan", "SDThoai"),
            Nbdctdtu = doc.GetElementValueByPath("DLHDon", "NDHDon", "NBan", "DCTDTu"),
            Nbstkhoan = doc.GetElementValueByPath("DLHDon", "NDHDon", "NBan", "STKNHang"),
            Nbtnhang = doc.GetElementValueByPath("DLHDon", "NDHDon", "NBan", "TNHang"),
            Nbwebsite = doc.GetElementValueByPath("DLHDon", "NDHDon", "NBan", "Website"),
            Nmten = doc.GetElementValueByPath("DLHDon", "NDHDon", "NMua", "Ten"),
            Nmmst = doc.GetElementValueByPath("DLHDon", "NDHDon", "NMua", "MST"),
            Nmdchi = doc.GetElementValueByPath("DLHDon", "NDHDon", "NMua", "DChi"),
            Nmdctdtu = doc.GetElementValueByPath("DLHDon", "NDHDon", "NMua", "DCTDTu"),
            Hdhhdvu = doc.GetChildElementsByPath(childName: "HHDVu",
                                                 path: ["DLHDon", "NDHDon", "DSHHDVu"])
                         .Select(el => new Doc_Hdhhdvu
                         {
                             Stt = el.GetXmlChildValueByPath("STT").ToInt(),
                             Ten = el.GetXmlChildValueByPath("THHDVu"),
                             Sluong = el.GetXmlChildValueByPath("SLuong").ToDouble(),
                             Dvtinh = el.GetXmlChildValueByPath("DVTinh"),
                             Dgia = el.GetXmlChildValueByPath("DGia").ToDouble(),
                             Thtien = el.GetXmlChildValueByPath("THTien").ToDecimal(),
                             Tsuat = el.GetXmlChildValueByPath("TSuat").ToDecimal(),
                             Tthue = el.GetXmlChildValueByPath("TTHue").ToDecimal(),
                             Tlckhau = el.GetXmlChildValueByPath("TLCKhau").ToDouble(),
                             Stckhau = el.GetXmlChildValueByPath("STCKhau").ToDouble(),
                             Ltsuat = el.GetXmlChildValueByPath("LTSuat"),
                         }).ToList(),
            Thttltsuat = doc.GetChildElementsByPath(childName: "THTTLTSuat",
                                                    path: ["DLHDon", "NDHDon", "TToan", "THTTLTSuat"])
                            .Select(el => new Doc_Thttltsuat
                            {
                                Tsuat = el.GetXmlChildValueByPath("TSuat"),
                                Tthue = el.GetXmlChildValueByPath("TTHue").ToDouble(),
                                Thtien = el.GetXmlChildValueByPath("THTien").ToDouble(),
                            }).ToList(),
            Tgtcthue = doc.GetElementValueByPath("DLHdon", "NDHDon", "TToan", "TgTCThue").ToDouble(),
            Tgtthue = doc.GetElementValueByPath("DLHdon", "NDHDon", "TToan", "TgTThue").ToDouble(),
            Tgtttbso = doc.GetElementValueByPath("DLHdon", "NDHDon", "TToan", "TgTTTBSo").ToDouble(),
            Tgtttbchu = doc.GetElementValueByPath("DLHdon", "NDHDon", "TToan", "TgTTTBChu"),
            Ttcktmai = doc.GetElementValueByPath("DLHdon", "NDHDon", "TToan", "TTCKTMai").ToDouble(),
            Tgtphi = doc.GetElementValueByPath("DLHdon", "NDHDon", "TToan", "TgTPhi").ToDouble(),
        };
    }

    public static SoldInvoiceDetail ToSoldInvoiceDetailModel(XDocument doc)
    {
        return new SoldInvoiceDetail
        {
            Khhdon = doc.GetXmlNodeValue("KHHDon"),
            Khmshdon = doc.GetXmlNodeValue("KHMShDon").ToInt(),
            Shdon = doc.GetXmlNodeValue("SHDon").ToInt(),
            Tdlap = doc.GetXmlNodeValue("NLap")?.ToDateTime(),
            Nky = doc.GetElementValueByPath("DSCKS", "NBan", "Signature", "Object", "SignatureProperties",
                                            "SignatureProperty", "SigningTime")?.ToDateTime(),
            Nbcks = doc.GetElementValueByPath("DSCKS", "NBan", "Signature", "KeyInfo", "X509Data",
                                              "X509SubjectName"),
            Mhdon = doc.GetElementValueByPath("MCCQT"),
            Htttoan = doc.GetXmlNodeValue("Htttoan") switch
            {
                "CK/TM" => 9,
                "CK" => 1,
                "TM" => 2,
                _ => 0
            },
            Ttxly = GetInvoiceType(doc),
            Dvtte = doc.GetXmlNodeValue("DVTTe"),
            Tgia = doc.GetXmlNodeValue("TGia").ToInt(),
            Msttcgp = doc.GetXmlNodeValue("Msttcgp"),
            Nbten = doc.GetElementValueByPath("DLHDon", "NDHDon", "NBan", "Ten"),
            Nbmst = doc.GetElementValueByPath("DLHDon", "NDHDon", "NBan", "MST"),
            Nbdchi = doc.GetElementValueByPath("DLHDon", "NDHDon", "NBan", "DChi"),
            Nmten = doc.GetElementValueByPath("DLHDon", "NDHDon", "NMua", "Ten"),
            Nmmst = doc.GetElementValueByPath("DLHDon", "NDHDon", "NMua", "MST"),
            Nmdchi = doc.GetElementValueByPath("DLHDon", "NDHDon", "NMua", "DChi"),
            Hdhhdvu = doc.GetChildElementsByPath(childName: "HHDVu",
                                                 path: ["DLHDon", "NDHDon", "DSHHDVu"])
                         .Select(el => new ChiTietHangHoa
                         {
                             Stt = el.GetXmlChildValueByPath("STT").ToInt(),
                             Ten = el.GetXmlChildValueByPath("THHDVu"),
                             Sluong = el.GetXmlChildValueByPath("SLuong").ToDouble(),
                             Dvtinh = el.GetXmlChildValueByPath("DVTinh"),
                             Dgia = el.GetXmlChildValueByPath("DGia").ToDouble(),
                             Thtien = el.GetXmlChildValueByPath("THTien").ToDecimal(),
                             Tsuat = el.GetXmlChildValueByPath("TSuat").ToDecimal(),
                             //Tthue = el.GetXmlChildValueByPath("TTHue").ToDecimal(),
                             Tlckhau = el.GetXmlChildValueByPath("TLCKhau").ToDouble(),
                             Stckhau = el.GetXmlChildValueByPath("STCKhau").ToDouble(),
                             Ltsuat = el.GetXmlChildValueByPath("LTSuat"),
                         }).ToList(),
            Thttltsuat = doc.GetChildElementsByPath(childName: "THTTLTSuat",
                                                    path: ["DLHDon", "NDHDon", "TToan", "THTTLTSuat"])
                            .Select(el => new ChitietThueSuat
                            {
                                Tsuat = el.GetXmlChildValueByPath("TSuat"),
                                Tthue = el.GetXmlChildValueByPath("TTHue").ToDouble(),
                                Thtien = el.GetXmlChildValueByPath("THTien").ToDouble(),
                            }).ToList(),
            Tgtcthue = doc.GetElementValueByPath("DLHdon", "NDHDon", "TToan", "TgTCThue").ToDouble(),
            Tgtthue = doc.GetElementValueByPath("DLHdon", "NDHDon", "TToan", "TgTThue").ToDouble(),
            Tgtttbso = doc.GetElementValueByPath("DLHdon", "NDHDon", "TToan", "TgTTTBSo").ToDouble(),
            Tgtttbchu = doc.GetElementValueByPath("DLHdon", "NDHDon", "TToan", "TgTTTBChu"),
            Ttcktmai = doc.GetElementValueByPath("DLHdon", "NDHDon", "TToan", "TTCKTMai").ToDouble(),
            //Tgtphi = doc.GetElementValueByPath("DLHdon", "NDHDon", "TToan", "TgTPhi").ToDouble(),
        };
    }

    /// <summary>
    /// Lấy dữ liệu về loại hóa đơn từ mã KHHDon.
    /// </summary>
    /// <param name="doc">The parsing XML document of the invoice</param>
    /// <returns>Loại hóa đơn: 5 - Hóa đơn cấp mã, 6 - Hóa đơn không cấp mã, 8 - Hóa đơn từ máy tính tiền</returns>
    private static int GetInvoiceType(this XDocument doc)
    {
        var khhdon = doc.GetXmlNodeValue("KHHDon");
        if (khhdon.IsNullOrEmpty() || khhdon?.Length < 2)
        {
            return 0;
        }

        if (khhdon?[1] == 'C' && khhdon[4] == 'T')
        {
            return 5; //Hóa đơn cấp mã
        }

        if (khhdon?[1] == 'C' && khhdon[4] == 'M')
        {
            return 8; //Hóa đơn từ máy tính tiền
        }

        if (khhdon?[1] == 'K')
        {
            return 6; //Hóa đơn không cấp mã
        }

        return 0;
    }
}