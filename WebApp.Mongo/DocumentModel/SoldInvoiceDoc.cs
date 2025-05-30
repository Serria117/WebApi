﻿namespace WebApp.Mongo.DocumentModel;

using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System;
using System.Collections.Generic;

[BsonIgnoreExtraElements]
public class SoldInvoiceDoc
{
    [BsonId]
    [BsonElement("_id")]
    public ObjectId _id { get; set; }
    
    [BsonElement("nbmst")]
    public string? Nbmst { get; set; }

    [BsonElement("khmshdon")]
    public int Khmshdon { get; set; }

    [BsonElement("khhdon")]
    public string? Khhdon { get; set; }

    [BsonElement("shdon")]
    public int Shdon { get; set; }

    [BsonElement("cqt")]
    public string? Cqt { get; set; }

    [BsonElement("cttkhac")]
    public List<object> Cttkhac { get; set; }

    [BsonElement("dvtte")]
    public string? Dvtte { get; set; }

    [BsonElement("hdon")]
    public string? Hdon { get; set; }

    [BsonElement("hsgcma")]
    public string? Hsgcma { get; set; }

    [BsonElement("hsgoc")]
    public string? Hsgoc { get; set; }

    [BsonElement("hthdon")]
    public int Hthdon { get; set; }

    [BsonElement("htttoan")]
    public int Htttoan { get; set; }
    
    [BsonElement("id")]
    public string? Id { get; set; }

    [BsonElement("idtbao")]
    public object? Idtbao { get; set; }

    [BsonElement("khdon")]
    public object? Khdon { get; set; }

    [BsonElement("khhdgoc")]
    public object? Khhdgoc { get; set; }

    [BsonElement("khmshdgoc")]
    public object? Khmshdgoc { get; set; }

    [BsonElement("lhdgoc")]
    public object? Lhdgoc { get; set; }

    [BsonElement("mhdon")]
    public string? Mhdon { get; set; }

    [BsonElement("mtdiep")]
    public object? Mtdiep { get; set; }

    [BsonElement("mtdtchieu")]
    public string? Mtdtchieu { get; set; }

    [BsonElement("nbdchi")]
    public string? Nbdchi { get; set; }

    [BsonElement("nbhdktngay")]
    public object? Nbhdktngay { get; set; }

    [BsonElement("nbhdktso")]
    public object? Nbhdktso { get; set; }

    [BsonElement("nbhdso")]
    public object? Nbhdso { get; set; }

    [BsonElement("nblddnbo")]
    public object? Nblddnbo { get; set; }

    [BsonElement("nbptvchuyen")]
    public object? Nbptvchuyen { get; set; }

    [BsonElement("nbstkhoan")]
    public object? Nbstkhoan { get; set; }

    [BsonElement("nbten")]
    public string? Nbten { get; set; }

    [BsonElement("nbtnhang")]
    public object? Nbtnhang { get; set; }

    [BsonElement("nbtnvchuyen")]
    public object? Nbtnvchuyen { get; set; }

    [BsonElement("nbttkhac")]
    public List<object> Nbttkhac { get; set; }

    [BsonElement("ncma")] [BsonRepresentation(BsonType.String)]
    public DateTime? Ncma { get; set; }

    [BsonElement("ncnhat")] [BsonRepresentation(BsonType.String)]
    public DateTime? Ncnhat { get; set; }

    [BsonElement("ngcnhat")]
    public string? Ngcnhat { get; set; }

    [BsonElement("nky")][BsonRepresentation(BsonType.String)]
    public DateTime? Nky { get; set; }

    [BsonElement("nmdchi")]
    public string? Nmdchi { get; set; }

    [BsonElement("nmmst")]
    public string? Nmmst { get; set; }

    [BsonElement("nmstkhoan")]
    public object? Nmstkhoan { get; set; }

    [BsonElement("nmten")]
    public string? Nmten { get; set; }

    [BsonElement("nmtnhang")]
    public object? Nmtnhang { get; set; }

    [BsonElement("nmtnmua")]
    public object? Nmtnmua { get; set; }

    [BsonElement("nmttkhac")]
    public List<object> Nmttkhac { get; set; }

    [BsonElement("ntao")] [BsonRepresentation(BsonType.String)]
    public DateTime? Ntao { get; set; }

    [BsonElement("ntnhan")] [BsonRepresentation(BsonType.String)]
    public DateTime? Ntnhan { get; set; }

    [BsonElement("pban")]
    public string? Pban { get; set; }

    [BsonElement("ptgui")]
    public int Ptgui { get; set; }

    [BsonElement("shdgoc")]
    public object? Shdgoc { get; set; }

    [BsonElement("tchat")]
    public int Tchat { get; set; }

    [BsonElement("tdlap")] [BsonRepresentation(BsonType.String)]
    public DateTime? Tdlap { get; set; }

    [BsonElement("tgia")]
    public double? Tgia { get; set; }

    [BsonElement("tgtcthue")]
    public double? Tgtcthue { get; set; }

    [BsonElement("tgtthue")]
    public double? Tgtthue { get; set; }

    [BsonElement("tgtttbchu")]
    public string? Tgtttbchu { get; set; }

    [BsonElement("tgtttbso")]
    public double? Tgtttbso { get; set; }

    [BsonElement("thdon")]
    public string? Thdon { get; set; }

    [BsonElement("thlap")]
    public int Thlap { get; set; }

    [BsonElement("thttlphi")]
    public List<object> Thttlphi { get; set; }

    [BsonElement("thttltsuat")]
    public List<Thttltsuat> Thttltsuat { get; set; }

    [BsonElement("tlhdon")]
    public string? Tlhdon { get; set; }

    [BsonElement("ttcktmai")]
    public double? Ttcktmai { get; set; }

    [BsonElement("tthai")]
    public int Tthai { get; set; }

    [BsonElement("ttkhac")]
    public List<object> Ttkhac { get; set; }

    [BsonElement("tttbao")]
    public int Tttbao { get; set; }

    [BsonElement("ttttkhac")]
    public List<object> Ttttkhac { get; set; }

    [BsonElement("ttxly")]
    public int Ttxly { get; set; }

    [BsonElement("tvandnkntt")]
    public string? Tvandnkntt { get; set; }

    [BsonElement("mhso")]
    public object? Mhso { get; set; }

    [BsonElement("ladhddt")]
    public int Ladhddt { get; set; }

    [BsonElement("mkhang")]
    public object? Mkhang { get; set; }

    [BsonElement("nbsdthoai")]
    public object? Nbsdthoai { get; set; }

    [BsonElement("nbdctdtu")]
    public object? Nbdctdtu { get; set; }

    [BsonElement("nbfax")]
    public object? Nbfax { get; set; }

    [BsonElement("nbwebsite")]
    public object? Nbwebsite { get; set; }

    [BsonElement("nbcks")]
    public string? Nbcks { get; set; }

    [BsonElement("nmsdthoai")]
    public object? Nmsdthoai { get; set; }

    [BsonElement("nmdctdtu")]
    public object? Nmdctdtu { get; set; }

    [BsonElement("nmcmnd")]
    public object? Nmcmnd { get; set; }

    [BsonElement("nmcks")]
    public object? Nmcks { get; set; }

    [BsonElement("bhphap")]
    public int Bhphap { get; set; }

    [BsonElement("hddunlap")]
    public object? Hddunlap { get; set; }

    [BsonElement("gchdgoc")]
    public object? Gchdgoc { get; set; }

    [BsonElement("tbhgtngay")]
    public object? Tbhgtngay { get; set; }

    [BsonElement("bhpldo")]
    public object? Bhpldo { get; set; }

    [BsonElement("bhpcbo")]
    public object? Bhpcbo { get; set; }

    [BsonElement("bhpngay")]
    public object? Bhpngay { get; set; }

    [BsonElement("tdlhdgoc")]
    public object? Tdlhdgoc { get; set; }

    [BsonElement("tgtphi")]
    public object? Tgtphi { get; set; }

    [BsonElement("unhiem")]
    public object? Unhiem { get; set; }

    [BsonElement("mstdvnunlhdon")]
    public object? Mstdvnunlhdon { get; set; }

    [BsonElement("tdvnunlhdon")]
    public object? Tdvnunlhdon { get; set; }

    [BsonElement("nbmdvqhnsach")]
    public object? Nbmdvqhnsach { get; set; }

    [BsonElement("nbsqdinh")]
    public object? Nbsqdinh { get; set; }

    [BsonElement("nbncqdinh")]
    public object? Nbncqdinh { get; set; }

    [BsonElement("nbcqcqdinh")]
    public object? Nbcqcqdinh { get; set; }

    [BsonElement("nbhtban")]
    public object? Nbhtban { get; set; }

    [BsonElement("nmmdvqhnsach")]
    public object? Nmmdvqhnsach { get; set; }

    [BsonElement("nmddvchden")]
    public object? Nmddvchden { get; set; }

    [BsonElement("nmtgvchdtu")]
    public object? Nmtgvchdtu { get; set; }

    [BsonElement("nmtgvchdden")]
    public object? Nmtgvchdden { get; set; }

    [BsonElement("nbtnban")]
    public object? Nbtnban { get; set; }

    [BsonElement("dcdvnunlhdon")]
    public object? Dcdvnunlhdon { get; set; }

    [BsonElement("dksbke")]
    public object? Dksbke { get; set; }

    [BsonElement("dknlbke")]
    public object? Dknlbke { get; set; }

    [BsonElement("thtttoan")]
    public string? Thtttoan { get; set; }

    [BsonElement("msttcgp")]
    public string? Msttcgp { get; set; }

    [BsonElement("cqtcks")]
    public string? Cqtcks { get; set; }

    [BsonElement("gchu")]
    public string? Gchu { get; set; }

    [BsonElement("kqcht")]
    public object? Kqcht { get; set; }

    [BsonElement("hdntgia")]
    public object? Hdntgia { get; set; }

    [BsonElement("tgtkcthue")]
    public object? Tgtkcthue { get; set; }

    [BsonElement("tgtkhac")]
    public object? Tgtkhac { get; set; }

    [BsonElement("nmshchieu")]
    public object? Nmshchieu { get; set; }

    [BsonElement("nmnchchieu")]
    public object? Nmnchchieu { get; set; }

    [BsonElement("nmnhhhchieu")]
    public object? Nmnhhhchieu { get; set; }

    [BsonElement("nmqtich")]
    public object? Nmqtich { get; set; }

    [BsonElement("ktkhthue")]
    public object? Ktkhthue { get; set; }

    [BsonElement("hdhhdvu")]
    public object? Hdhhdvu { get; set; }

    [BsonElement("qrcode")]
    public object? Qrcode { get; set; }

    [BsonElement("ttmstten")]
    public object? Ttmstten { get; set; }

    [BsonElement("ladhddtten")]
    public object? Ladhddtten { get; set; }

    [BsonElement("hdxkhau")]
    public object? Hdxkhau { get; set; }

    [BsonElement("hdxkptquan")]
    public object? Hdxkptquan { get; set; }

    [BsonElement("hdgktkhthue")]
    public object? Hdgktkhthue { get; set; }

    [BsonElement("hdonLquans")]
    public object? HdonLquans { get; set; }

    [BsonElement("tthdclquan")]
    public bool? Tthdclquan { get; set; }

    [BsonElement("pdndungs")]
    public object? Pdndungs { get; set; }

    [BsonElement("hdtbssrses")]
    public object? Hdtbssrses { get; set; }

    [BsonElement("hdTrung")]
    public object? HdTrung { get; set; }

    [BsonElement("isHDTrung")]
    public object? IsHDTrung { get; set; }
}

