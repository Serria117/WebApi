using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WebApp.Core.DomainEntities.Invoice;

[Table("INV_PurchaseInvoiceData")]
public class PurchaseInvoiceData : BaseEntityAuditable<string>
{
	[MaxLength(50)]
	public new string Id { get; set; } = Ulid.NewUlid().ToString();
	public Guid PurchaseInvoiceId { get; set; }

	[ForeignKey("PurchaseInvoiceId")]
	public PurchaseInvoice PurchaseInvoice { get; set; } = null!;

	[Column(TypeName = "jsonb")]
	public PurchaseInvoiceJson JsonContent { get; set; } = null!;
}

public class PurchaseInvoiceJson
{
	public string? Nbmst { get; set; }

	public int? Khmshdon { get; set; }

	public string? Khhdon { get; set; }

	public int? Shdon { get; set; }

	public string? Cqt { get; set; }

	public List<Cttkhac> Cttkhac { get; set; } = [];

	public string? Dvtte { get; set; }

	public string? Hdon { get; set; }

	public string? Hsgcma { get; set; }

	public string? Hsgoc { get; set; }

	public int? Hthdon { get; set; }

	public int? Htttoan { get; set; }

	public string? Id { get; set; }

	public string? Idtbao { get; set; }

	public string? Khdon { get; set; }

	public string? Khhdgoc { get; set; }

	public string? Khmshdgoc { get; set; }

	public string? Lhdgoc { get; set; }

	public string? Mhdon { get; set; }

	public string? Mtdiep { get; set; }

	public string? Mtdtchieu { get; set; }

	public string? Nbdchi { get; set; }

	public string? Nbhdktngay { get; set; }

	public string? Nbhdktso { get; set; }

	public string? Nbhdso { get; set; }

	public string? Nblddnbo { get; set; }

	public string? Nbptvchuyen { get; set; }

	public string? Nbstkhoan { get; set; }

	public string? Nbten { get; set; }

	public string? Nbtnhang { get; set; }

	public string? Nbtnvchuyen { get; set; }

	public List<Nbttkhac>? Nbttkhac { get; set; }

	public DateTime? Ncma { get; set; }

	public DateTime? Ncnhat { get; set; }

	public string? Ngcnhat { get; set; }

	public DateTime? Nky { get; set; }

	public string? Nmdchi { get; set; }

	public string? Nmmst { get; set; }

	public string? Nmstkhoan { get; set; }

	public string? Nmten { get; set; }

	public string? Nmtnhang { get; set; }

	public string? Nmtnmua { get; set; }

	public List<Nmttkhac>? Nmttkhac { get; set; }

	public DateTime? Ntao { get; set; }

	public DateTime? Ntnhan { get; set; }

	public string? Pban { get; set; }

	public int? Ptgui { get; set; }

	public int? Shdgoc { get; set; }

	public int? Tchat { get; set; }

	public DateTime? Tdlap { get; set; }

	public double? Tgia { get; set; }

	public double? Tgtcthue { get; set; }

	public double? Tgtthue { get; set; }

	public string? Tgtttbchu { get; set; }

	public double? Tgtttbso { get; set; }

	public string? Thdon { get; set; }

	public int? Thlap { get; set; }

	public List<Thttlphi> Thttlphi { get; set; } = [];

	public List<Thttltsuat> Thttltsuat { get; set; } = [];

	public string? Tlhdon { get; set; }

	public double? Ttcktmai { get; set; }

	public int? Tthai { get; set; }

	public List<Ttkhac>? Ttkhac { get; set; }

	public int? Tttbao { get; set; }

	public List<Ttttkhac>? Ttttkhac { get; set; }

	public int? Ttxly { get; set; }

	public string? Tvandnkntt { get; set; }

	public string? Mhso { get; set; }

	public int? Ladhddt { get; set; }

	public string? Mkhang { get; set; }

	public string? Nbsdthoai { get; set; }

	public string? Nbdctdtu { get; set; }

	public string? Nbfax { get; set; }

	public string? Nbwebsite { get; set; }

	public string? Nbcks { get; set; }

	public string? Nmsdthoai { get; set; }

	public string? Nmdctdtu { get; set; }

	public string? Nmcmnd { get; set; }

	public string? Nmcks { get; set; }

	public int? Bhphap { get; set; }

	public string? Hddunlap { get; set; }

	public string? Gchdgoc { get; set; }

	public string? Tbhgtngay { get; set; }

	public string? Bhpldo { get; set; }

	public string? Bhpcbo { get; set; }

	public string? Bhpngay { get; set; }

	public DateTime? Tdlhdgoc { get; set; }

	public double? Tgtphi { get; set; }

	public string? Unhiem { get; set; }

	public string? Mstdvnunlhdon { get; set; }

	public string? Tdvnunlhdon { get; set; }

	public string? Nbmdvqhnsach { get; set; }

	public string? Nbsqdinh { get; set; }

	public string? Nbncqdinh { get; set; }

	public string? Nbcqcqdinh { get; set; }

	public string? Nbhtban { get; set; }

	public string? Nmmdvqhnsach { get; set; }

	public string? Nmddvchden { get; set; }

	public string? Nmtgvchdtu { get; set; }

	public string? Nmtgvchdden { get; set; }

	public string? Nbtnban { get; set; }

	public string? Dcdvnunlhdon { get; set; }

	public string? Dcdsbke { get; set; }

	public string? Dknlbke { get; set; }

	public string? Thtttoan { get; set; }

	public string? Msttcgp { get; set; }

	public string? Cqtcks { get; set; }

	public string? Gchu { get; set; }

	public string? Kqcht { get; set; }

	public string? Hdntgia { get; set; }

	public double? Tgtkcthue { get; set; }

	public double? Tgtkhac { get; set; }

	public string? Nmshchieu { get; set; }

	public string? Nmnchchieu { get; set; }

	public string? Nmnhhhchieu { get; set; }

	public string? Nmqtich { get; set; }

	public string? Ktkhthue { get; set; }

	public List<Hdhhdvu>? Hdhhdvu { get; set; } = [];

	public string? Qrcode { get; set; }

	public string? Ttmstten { get; set; }

	public string? Ladhddtten { get; set; }

	public string? Hdxkhau { get; set; }

	public string? Hdxkptquan { get; set; }

	public string? Hdgktkhthue { get; set; }

	public string? HdonLquans { get; set; }

	public bool? Tthdclquan { get; set; }

	public string? Pdndungs { get; set; }

	public string? Hdtbssrses { get; set; }

	public string? HdTrung { get; set; }

	public string? IsHDTrung { get; set; }
}

public class Thttltsuat
{
	public string? Tsuat { get; set; }

	public double? Thtien { get; set; }

	public double? Tthue { get; set; }

	public string? Gttsuat { get; set; }
}

public class Hdhhdvu
{
	public string? Idhdon { get; set; }

	public string? Id { get; set; }

	public double? Dgia { get; set; }

	public string? Dvtinh { get; set; }

	public string? Ltsuat { get; set; }

	public double? Sluong { get; set; }

	public string? Stbchu { get; set; }

	public double? Stckhau { get; set; }

	public int? Stt { get; set; }

	public int? Tchat { get; set; }

	public string? Ten { get; set; }

	public string? Thtcthue { get; set; }

	public decimal? Thtien { get; set; }

	public double? Tlckhau { get; set; }

	public decimal? Tsuat { get; set; }

	public string? Tthue { get; set; }

	public int? Sxep { get; set; }

	public List<Ttkhac>? Ttkhac { get; set; }

	public string? Dvtte { get; set; }

	public string? Tgia { get; set; }
}

public class Cttkhac
{
	public string? Ttruong { get; set; }

	public string? Kdlieu { get; set; }

	public string? Dlieu { get; set; }
}

public class Ttkhac
{
	public string? Ttruong { get; set; }

	public string? Kdlieu { get; set; }

	public string? Dlieu { get; set; }
}

public class Nbttkhac
{
	public string? Ttruong { get; set; }

	public string? Kdlieu { get; set; }

	public string? Dlieu { get; set; }
}

public class Nmttkhac
{
	public string? Ttruong { get; set; }

	public string? Kdlieu { get; set; }

	public string? Dlieu { get; set; }
}

public class Ttttkhac
{
	public string? Ttruong { get; set; }

	public string? Kdlieu { get; set; }

	public string? Dlieu { get; set; }
}

public class Thttlphi
{
	public string? Tlphi { get; set; } = string.Empty;
	public decimal? Tphi { get; set; } = 0;
}