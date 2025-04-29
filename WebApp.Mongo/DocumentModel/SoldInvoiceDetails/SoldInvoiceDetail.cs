using System.Text.Json.Serialization;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace WebApp.Mongo.DocumentModel.SoldInvoiceDetails;

[BsonIgnoreExtraElements]
public class SoldInvoiceDetail
{
    [BsonElement("nbmst")]
    public string Nbmst { get; set; }

    [BsonElement("khmshdon")]
    public int Khmshdon { get; set; }

    [BsonElement("khhdon")]
    public string Khhdon { get; set; }

    [BsonElement("shdon")]
    public int Shdon { get; set; }

    [BsonElement("cqt")]
    public string Cqt { get; set; }

    [BsonElement("cttkhac")]
    public List<object> Cttkhac { get; set; } = [];

    [BsonElement("dvtte")]
    public string Dvtte { get; set; }

    [BsonElement("hdon")]
    public string Hdon { get; set; }

    [BsonElement("hsgcma")]
    public string Hsgcma { get; set; }

    [BsonElement("hsgoc")]
    public string Hsgoc { get; set; }

    [BsonElement("hthdon")]
    public int Hthdon { get; set; }

    [BsonElement("htttoan")]
    public int Htttoan { get; set; }

    [BsonId]
    [BsonElement("id")]
    public string Id { get; set; }

    [BsonElement("idtbao")]
    public string Idtbao { get; set; }

    [BsonElement("mhdon")]
    public string Mhdon { get; set; }

    [BsonElement("mtdtchieu")]
    public string Mtdtchieu { get; set; }

    [BsonElement("nbdchi")]
    public string Nbdchi { get; set; }

    [BsonElement("nbten")]
    public string Nbten { get; set; }

    [BsonElement("nbttkhac")][JsonIgnore]
    public List<Cttkhac>? Nbttkhac { get; set; } = [];

    [BsonElement("ncma")][BsonRepresentation(BsonType.String)]
    public DateTime? Ncma { get; set; }

    [BsonElement("ncnhat")]
    public DateTime Ncnhat { get; set; }

    [BsonElement("ngcnhat")]
    public string Ngcnhat { get; set; }

    [BsonElement("nky")][BsonRepresentation(BsonType.String)]
    public DateTime? Nky { get; set; }

    [BsonElement("nmdchi")]
    public string Nmdchi { get; set; }

    [BsonElement("nmmst")]
    public string Nmmst { get; set; }

    [BsonElement("nmten")]
    public string Nmten { get; set; }

    [BsonElement("nmttkhac")][JsonIgnore]
    public List<object> Nmttkhac { get; set; }

    [BsonElement("ntao")][BsonRepresentation(BsonType.String)]
    public DateTime Ntao { get; set; }

    [BsonElement("ntnhan")][BsonRepresentation(BsonType.String)]
    public DateTime Ntnhan { get; set; }

    [BsonElement("pban")]
    public string Pban { get; set; }

    [BsonElement("ptgui")]
    public int Ptgui { get; set; }

    [BsonElement("tchat")]
    public int Tchat { get; set; }

    [BsonElement("tdlap")] [BsonRepresentation(BsonType.String)]
    public DateTime Tdlap { get; set; }

    [BsonElement("tgia")]
    public double Tgia { get; set; }

    [BsonElement("tgtcthue")]
    public double Tgtcthue { get; set; }

    [BsonElement("tgtthue")]
    public double Tgtthue { get; set; }

    [BsonElement("tgtttbchu")]
    public string Tgtttbchu { get; set; }

    [BsonElement("tgtttbso")]
    public double Tgtttbso { get; set; }

    [BsonElement("thdon")]
    public string Thdon { get; set; }

    [BsonElement("thlap")]
    public int Thlap { get; set; }

    [BsonElement("thttltsuat")]
    public List<ChitietThueSuat> Thttltsuat { get; set; } = [];

    [BsonElement("tlhdon")]
    public string Tlhdon { get; set; }

    [BsonElement("ttcktmai")]
    public double Ttcktmai { get; set; }

    [BsonElement("tthai")]
    public int Tthai { get; set; }

    [BsonElement("ttkhac")][JsonIgnore]
    public List<object> Ttkhac { get; set; }

    [BsonElement("tttbao")]
    public int Tttbao { get; set; }

    [BsonElement("ttttkhac")][JsonIgnore]
    public List<object> Ttttkhac { get; set; }

    [BsonElement("ttxly")]
    public int Ttxly { get; set; }

    [BsonElement("tvandnkntt")]
    public string Tvandnkntt { get; set; }

    [BsonElement("thtttoan")]
    public string Thtttoan { get; set; }

    [BsonElement("msttcgp")]
    public string Msttcgp { get; set; }

    [BsonElement("nbcks")]
    public string? Nbcks { get; set; }

    [BsonElement("cqtcks")]
    public string? Cqtcks { get; set; }

    [BsonElement("hdhhdvu")]
    public List<ChiTietHangHoa> Hdhhdvu { get; set; } = [];

    [BsonElement("qrcode")]
    public string Qrcode { get; set; }

    [BsonElement("tthdclquan")]
    public bool Tthdclquan { get; set; }
}

public class ChiTietHangHoa
{
    [BsonElement("idhdon")]
    public string Idhdon { get; set; } = string.Empty;

    [BsonElement("id")]
    public string Id { get; set; } = string.Empty;

    [BsonElement("dgia")]
    public double Dgia { get; set; }

    [BsonElement("dvtinh")]
    public string? Dvtinh { get; set; }

    [BsonElement("ltsuat")]
    public string? Ltsuat { get; set; }

    [BsonElement("sluong")]
    public double Sluong { get; set; }

    [BsonElement("ten")]
    public string Ten { get; set; } = string.Empty;

    [BsonElement("thtien")]
    public decimal Thtien { get; set; }

    [BsonElement("tsuat")]
    public decimal Tsuat { get; set; }

    [BsonElement("sxep")]
    public int Sxep { get; set; }

    [BsonElement("tchat")]
    public int Tchat { get; set; }

    [BsonElement("stckhau")]
    public double? Stckhau { get; set; }

    [BsonElement("tlckhau")]
    public double? Tlckhau { get; set; }

    [BsonElement("stt")]
    public int Stt { get; set; }

    [BsonElement("ttkhac")]
    public List<Cttkhac> Ttkhac { get; set; }
}

public class ChitietThueSuat
{
    [BsonElement("tsuat")]
    public string? Tsuat { get; set; }

    [BsonElement("thtien")]
    public double Thtien { get; set; }

    [BsonElement("tthue")]
    public double Tthue { get; set; }

    [BsonElement("gttsuat")]
    public string? Gttsuat { get; set; }
}

[BsonIgnoreExtraElements]
public class Cttkhac
{
    [BsonElement("ttruong")] public string? Ttruong { get; set; }

    [BsonElement("kdlieu")] public string? Kdlieu { get; set; }

    [BsonElement("dlieu")] public string? Dlieu { get; set; }
}