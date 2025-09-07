// ReSharper disable InconsistentNaming

namespace WebApp.Payloads.DocumentPayload;

public class DocumentBctc133Payload : BaseDocumentPayload
{
    public ChiTieuToKhaiChinh? ChiTieuChinhCuoiNam { get; set; }
    public ChiTieuToKhaiChinh? ChiTieuChinhDauNam { get; set; }
    public Pl_Kqkd_133? KqkdNamNay { get; set; }
    public Pl_Kqkd_133? KqkdNamTruoc { get; set; }
    public Pl_Cdtk_133? CanDoiTaiKhoan { get; set; }
    public Pl_Lctt_133? Lctt { get; set; }
}

public class ChiTieuToKhaiChinh
{
    public string? ct110 { get; set; }
    public string? ct120 { get; set; }
    public string? ct121 { get; set; }
    public string? ct122 { get; set; }
    public string? ct123 { get; set; }
    public string? ct124 { get; set; }
    public string? ct130 { get; set; }
    public string? ct131 { get; set; }
    public string? ct132 { get; set; }
    public string? ct133 { get; set; }
    public string? ct134 { get; set; }
    public string? ct135 { get; set; }
    public string? ct136 { get; set; }
    public string? ct140 { get; set; }
    public string? ct141 { get; set; }
    public string? ct142 { get; set; }
    public string? ct150 { get; set; }
    public string? ct151 { get; set; }
    public string? ct152 { get; set; }
    public string? ct160 { get; set; }
    public string? ct161 { get; set; }
    public string? ct162 { get; set; }
    public string? ct170 { get; set; }
    public string? ct180 { get; set; }
    public string? ct181 { get; set; }
    public string? ct182 { get; set; }
    public string? ct200 { get; set; }
    public string? ct300 { get; set; }
    public string? ct311 { get; set; }
    public string? ct312 { get; set; }
    public string? ct313 { get; set; }
    public string? ct314 { get; set; }
    public string? ct315 { get; set; }
    public string? ct316 { get; set; }
    public string? ct317 { get; set; }
    public string? ct318 { get; set; }
    public string? ct319 { get; set; }
    public string? ct320 { get; set; }
    public string? ct400 { get; set; }
    public string? ct411 { get; set; }
    public string? ct412 { get; set; }
    public string? ct413 { get; set; }
    public string? ct414 { get; set; }
    public string? ct415 { get; set; }
    public string? ct416 { get; set; }
    public string? ct417 { get; set; }
    public string? ct500 { get; set; }
}

public class ChiTieuChinhDauNam_133
{
    public string? ct110 { get; set; }
    public string? ct120 { get; set; }
    public string? ct121 { get; set; }
    public string? ct122 { get; set; }
    public string? ct123 { get; set; }
    public string? ct124 { get; set; }
    public string? ct130 { get; set; }
    public string? ct131 { get; set; }
    public string? ct132 { get; set; }
    public string? ct133 { get; set; }
    public string? ct134 { get; set; }
    public string? ct135 { get; set; }
    public string? ct136 { get; set; }
    public string? ct140 { get; set; }
    public string? ct141 { get; set; }
    public string? ct142 { get; set; }
    public string? ct150 { get; set; }
    public string? ct151 { get; set; }
    public string? ct152 { get; set; }
    public string? ct160 { get; set; }
    public string? ct161 { get; set; }
    public string? ct162 { get; set; }
    public string? ct170 { get; set; }
    public string? ct180 { get; set; }
    public string? ct181 { get; set; }
    public string? ct182 { get; set; }
    public string? ct200 { get; set; }
    public string? ct300 { get; set; }
    public string? ct311 { get; set; }
    public string? ct312 { get; set; }
    public string? ct313 { get; set; }
    public string? ct314 { get; set; }
    public string? ct315 { get; set; }
    public string? ct316 { get; set; }
    public string? ct317 { get; set; }
    public string? ct318 { get; set; }
    public string? ct319 { get; set; }
    public string? ct320 { get; set; }
    public string? ct400 { get; set; }
    public string? ct411 { get; set; }
    public string? ct412 { get; set; }
    public string? ct413 { get; set; }
    public string? ct414 { get; set; }
    public string? ct415 { get; set; }
    public string? ct416 { get; set; }
    public string? ct417 { get; set; }
    public string? ct500 { get; set; }
}

public class Pl_Kqkd_133
{
    public string? ct01 { get; set; }
    public string? ct02 { get; set; }
    public string? ct10 { get; set; }
    public string? ct11 { get; set; }
    public string? ct20 { get; set; }
    public string? ct21 { get; set; }
    public string? ct22 { get; set; }
    public string? ct23 { get; set; }
    public string? ct24 { get; set; }
    public string? ct30 { get; set; }
    public string? ct31 { get; set; }
    public string? ct32 { get; set; }
    public string? ct40 { get; set; }
    public string? ct50 { get; set; }
    public string? ct51 { get; set; }
    public string? ct60 { get; set; }
}

public class Pl_KqkdNamTruoc_133
{
    public string? ct01 { get; set; }
    public string? ct02 { get; set; }
    public string? ct10 { get; set; }
    public string? ct11 { get; set; }
    public string? ct20 { get; set; }
    public string? ct21 { get; set; }
    public string? ct22 { get; set; }
    public string? ct23 { get; set; }
    public string? ct24 { get; set; }
    public string? ct30 { get; set; }
    public string? ct31 { get; set; }
    public string? ct32 { get; set; }
    public string? ct40 { get; set; }
    public string? ct50 { get; set; }
    public string? ct51 { get; set; }
    public string? ct60 { get; set; }
}

public class Pl_Cdtk_133
{
    public Cdtk_133_DauKy? SoDuDauKy { get; set; }
    public Cdtk_133_PhatSinh? SoPhatSinhTrongKy { get; set; }
    public Cdtk_133_CuoiKy? SoDuCuoiKy { get; set; }
}

public class Cdtk_133_DauKy
{
    public Cdtk_133_Account? No { get; set; }
    public Cdtk_133_Account? Co { get; set; }
}

public class Cdtk_133_PhatSinh
{
    public Cdtk_133_Account? No { get; set; }
    public Cdtk_133_Account? Co { get; set; }
}

public class Cdtk_133_CuoiKy
{
    public Cdtk_133_Account? No { get; set; }
    public Cdtk_133_Account? Co { get; set; }
}

public class Cdtk_133_Account
{
    public string? ct111 { get; set; }
    public string? ct1111 { get; set; }
    public string? ct1112 { get; set; }
    public string? ct112 { get; set; }
    public string? ct1121 { get; set; }
    public string? ct1122 { get; set; }
    public string? ct121 { get; set; }
    public string? ct128 { get; set; }
    public string? ct1281 { get; set; }
    public string? ct1288 { get; set; }
    public string? ct131 { get; set; }
    public string? ct133 { get; set; }
    public string? ct1331 { get; set; }
    public string? ct1332 { get; set; }
    public string? ct136 { get; set; }
    public string? ct1361 { get; set; }
    public string? ct1368 { get; set; }
    public string? ct138 { get; set; }
    public string? ct1381 { get; set; }
    public string? ct1386 { get; set; }
    public string? ct1388 { get; set; }
    public string? ct141 { get; set; }
    public string? ct151 { get; set; }
    public string? ct152 { get; set; }
    public string? ct153 { get; set; }
    public string? ct154 { get; set; }
    public string? ct155 { get; set; }
    public string? ct156 { get; set; }
    public string? ct157 { get; set; }
    public string? ct211 { get; set; }
    public string? ct2111 { get; set; }
    public string? ct2112 { get; set; }
    public string? ct2113 { get; set; }
    public string? ct214 { get; set; }
    public string? ct2141 { get; set; }
    public string? ct2142 { get; set; }
    public string? ct2143 { get; set; }
    public string? ct2147 { get; set; }
    public string? ct217 { get; set; }
    public string? ct228 { get; set; }
    public string? ct2281 { get; set; }
    public string? ct2288 { get; set; }
    public string? ct229 { get; set; }
    public string? ct2291 { get; set; }
    public string? ct2292 { get; set; }
    public string? ct2293 { get; set; }
    public string? ct2294 { get; set; }
    public string? ct241 { get; set; }
    public string? ct2411 { get; set; }
    public string? ct2412 { get; set; }
    public string? ct2413 { get; set; }
    public string? ct242 { get; set; }
    public string? ct331 { get; set; }
    public string? ct333 { get; set; }
    public string? ct3331 { get; set; }
    public string? ct33311 { get; set; }
    public string? ct33312 { get; set; }
    public string? ct3332 { get; set; }
    public string? ct3333 { get; set; }
    public string? ct3334 { get; set; }
    public string? ct3335 { get; set; }
    public string? ct3336 { get; set; }
    public string? ct3337 { get; set; }
    public string? ct3338 { get; set; }
    public string? ct33381 { get; set; }
    public string? ct33382 { get; set; }
    public string? ct3339 { get; set; }
    public string? ct334 { get; set; }
    public string? ct335 { get; set; }
    public string? ct336 { get; set; }
    public string? ct3361 { get; set; }
    public string? ct3368 { get; set; }
    public string? ct338 { get; set; }
    public string? ct3381 { get; set; }
    public string? ct3382 { get; set; }
    public string? ct3383 { get; set; }
    public string? ct3384 { get; set; }
    public string? ct3385 { get; set; }
    public string? ct3386 { get; set; }
    public string? ct3387 { get; set; }
    public string? ct3388 { get; set; }
    public string? ct341 { get; set; }
    public string? ct3411 { get; set; }
    public string? ct3412 { get; set; }
    public string? ct352 { get; set; }
    public string? ct3521 { get; set; }
    public string? ct3522 { get; set; }
    public string? ct3524 { get; set; }
    public string? ct353 { get; set; }
    public string? ct3531 { get; set; }
    public string? ct3532 { get; set; }
    public string? ct3533 { get; set; }
    public string? ct3534 { get; set; }
    public string? ct356 { get; set; }
    public string? ct3561 { get; set; }
    public string? ct3562 { get; set; }
    public string? ct411 { get; set; }
    public string? ct4111 { get; set; }
    public string? ct4112 { get; set; }
    public string? ct4118 { get; set; }
    public string? ct413 { get; set; }
    public string? ct418 { get; set; }
    public string? ct419 { get; set; }
    public string? ct421 { get; set; }
    public string? ct4211 { get; set; }
    public string? ct4212 { get; set; }
    public string? ct511 { get; set; }
    public string? ct5111 { get; set; }
    public string? ct5112 { get; set; }
    public string? ct5113 { get; set; }
    public string? ct5118 { get; set; }
    public string? ct515 { get; set; }
    public string? ct611 { get; set; }
    public string? ct631 { get; set; }
    public string? ct632 { get; set; }
    public string? ct635 { get; set; }
    public string? ct642 { get; set; }
    public string? ct6421 { get; set; }
    public string? ct6422 { get; set; }
    public string? ct711 { get; set; }
    public string? ct811 { get; set; }
    public string? ct821 { get; set; }
    public string? ct911 { get; set; }
    public string? tongCong { get; set; }
}

public class Cdtk_133_Co
{
    public string? ct111 { get; set; }
    public string? ct1111 { get; set; }
    public string? ct1112 { get; set; }
    public string? ct112 { get; set; }
    public string? ct1121 { get; set; }
    public string? ct1122 { get; set; }
    public string? ct121 { get; set; }
    public string? ct128 { get; set; }
    public string? ct1281 { get; set; }
    public string? ct1288 { get; set; }
    public string? ct131 { get; set; }
    public string? ct133 { get; set; }
    public string? ct1331 { get; set; }
    public string? ct1332 { get; set; }
    public string? ct136 { get; set; }
    public string? ct1361 { get; set; }
    public string? ct1368 { get; set; }
    public string? ct138 { get; set; }
    public string? ct1381 { get; set; }
    public string? ct1386 { get; set; }
    public string? ct1388 { get; set; }
    public string? ct141 { get; set; }
    public string? ct151 { get; set; }
    public string? ct152 { get; set; }
    public string? ct153 { get; set; }
    public string? ct154 { get; set; }
    public string? ct155 { get; set; }
    public string? ct156 { get; set; }
    public string? ct157 { get; set; }
    public string? ct211 { get; set; }
    public string? ct2111 { get; set; }
    public string? ct2112 { get; set; }
    public string? ct2113 { get; set; }
    public string? ct214 { get; set; }
    public string? ct2141 { get; set; }
    public string? ct2142 { get; set; }
    public string? ct2143 { get; set; }
    public string? ct2147 { get; set; }
    public string? ct217 { get; set; }
    public string? ct228 { get; set; }
    public string? ct2281 { get; set; }
    public string? ct2288 { get; set; }
    public string? ct229 { get; set; }
    public string? ct2291 { get; set; }
    public string? ct2292 { get; set; }
    public string? ct2293 { get; set; }
    public string? ct2294 { get; set; }
    public string? ct241 { get; set; }
    public string? ct2411 { get; set; }
    public string? ct2412 { get; set; }
    public string? ct2413 { get; set; }
    public string? ct242 { get; set; }
    public string? ct331 { get; set; }
    public string? ct333 { get; set; }
    public string? ct3331 { get; set; }
    public string? ct33311 { get; set; }
    public string? ct33312 { get; set; }
    public string? ct3332 { get; set; }
    public string? ct3333 { get; set; }
    public string? ct3334 { get; set; }
    public string? ct3335 { get; set; }
    public string? ct3336 { get; set; }
    public string? ct3337 { get; set; }
    public string? ct3338 { get; set; }
    public string? ct33381 { get; set; }
    public string? ct33382 { get; set; }
    public string? ct3339 { get; set; }
    public string? ct334 { get; set; }
    public string? ct335 { get; set; }
    public string? ct336 { get; set; }
    public string? ct3361 { get; set; }
    public string? ct3368 { get; set; }
    public string? ct338 { get; set; }
    public string? ct3381 { get; set; }
    public string? ct3382 { get; set; }
    public string? ct3383 { get; set; }
    public string? ct3384 { get; set; }
    public string? ct3385 { get; set; }
    public string? ct3386 { get; set; }
    public string? ct3387 { get; set; }
    public string? ct3388 { get; set; }
    public string? ct341 { get; set; }
    public string? ct3411 { get; set; }
    public string? ct3412 { get; set; }
    public string? ct352 { get; set; }
    public string? ct3521 { get; set; }
    public string? ct3522 { get; set; }
    public string? ct3524 { get; set; }
    public string? ct353 { get; set; }
    public string? ct3531 { get; set; }
    public string? ct3532 { get; set; }
    public string? ct3533 { get; set; }
    public string? ct3534 { get; set; }
    public string? ct356 { get; set; }
    public string? ct3561 { get; set; }
    public string? ct3562 { get; set; }
    public string? ct411 { get; set; }
    public string? ct4111 { get; set; }
    public string? ct4112 { get; set; }
    public string? ct4118 { get; set; }
    public string? ct413 { get; set; }
    public string? ct418 { get; set; }
    public string? ct419 { get; set; }
    public string? ct421 { get; set; }
    public string? ct4211 { get; set; }
    public string? ct4212 { get; set; }
    public string? ct511 { get; set; }
    public string? ct5111 { get; set; }
    public string? ct5112 { get; set; }
    public string? ct5113 { get; set; }
    public string? ct5118 { get; set; }
    public string? ct515 { get; set; }
    public string? ct611 { get; set; }
    public string? ct631 { get; set; }
    public string? ct632 { get; set; }
    public string? ct635 { get; set; }
    public string? ct642 { get; set; }
    public string? ct6421 { get; set; }
    public string? ct6422 { get; set; }
    public string? ct711 { get; set; }
    public string? ct811 { get; set; }
    public string? ct821 { get; set; }
    public string? ct911 { get; set; }
    public string? tongCong { get; set; }
}

public class Pl_Lctt_133
{
    public Lctt_133_NamNay? NamNay { get; set; }
    public Lctt_133_NamTruoc? NamTruoc { get; set; }
}



public class ThuyetMinh { 
	public string? ct01 { get; set; } 
	public string? ct02 { get; set; } 
	public string? ct03 { get; set; } 
	public string? ct04 { get; set; } 
	public string? ct05 { get; set; } 
	public string? ct06 { get; set; } 
	public string? ct07 { get; set; } 
	public string? ct20 { get; set; } 
	public string? ct21 { get; set; } 
	public string? ct22 { get; set; } 
	public string? ct23 { get; set; } 
	public string? ct24 { get; set; } 
	public string? ct25 { get; set; } 
	public string? ct30 { get; set; } 
	public string? ct31 { get; set; } 
	public string? ct32 { get; set; } 
	public string? ct33 { get; set; } 
	public string? ct34 { get; set; } 
	public string? ct35 { get; set; } 
	public string? ct40 { get; set; } 
	public string? ct50 { get; set; } 
	public string? ct60 { get; set; } 
	public string? ct61 { get; set; } 
	public string? ct70 { get; set; } 
}

public class Lctt_133_NamNay { 
	public string? ct01 { get; set; } 
	public string? ct02 { get; set; } 
	public string? ct03 { get; set; } 
	public string? ct04 { get; set; } 
	public string? ct05 { get; set; } 
	public string? ct06 { get; set; } 
	public string? ct07 { get; set; } 
	public string? ct20 { get; set; } 
	public string? ct21 { get; set; } 
	public string? ct22 { get; set; } 
	public string? ct23 { get; set; } 
	public string? ct24 { get; set; } 
	public string? ct25 { get; set; } 
	public string? ct30 { get; set; } 
	public string? ct31 { get; set; } 
	public string? ct32 { get; set; } 
	public string? ct33 { get; set; } 
	public string? ct34 { get; set; } 
	public string? ct35 { get; set; } 
	public string? ct40 { get; set; } 
	public string? ct50 { get; set; } 
	public string? ct60 { get; set; } 
	public string? ct61 { get; set; } 
	public string? ct70 { get; set; } 
}

public class Lctt_133_NamTruoc { 
	public string? ct01 { get; set; } 
	public string? ct02 { get; set; } 
	public string? ct03 { get; set; } 
	public string? ct04 { get; set; } 
	public string? ct05 { get; set; } 
	public string? ct06 { get; set; } 
	public string? ct07 { get; set; } 
	public string? ct20 { get; set; } 
	public string? ct21 { get; set; } 
	public string? ct22 { get; set; } 
	public string? ct23 { get; set; } 
	public string? ct24 { get; set; } 
	public string? ct25 { get; set; } 
	public string? ct30 { get; set; } 
	public string? ct31 { get; set; } 
	public string? ct32 { get; set; } 
	public string? ct33 { get; set; } 
	public string? ct34 { get; set; } 
	public string? ct35 { get; set; } 
	public string? ct40 { get; set; } 
	public string? ct50 { get; set; } 
	public string? ct60 { get; set; } 
	public string? ct61 { get; set; } 
	public string? ct70 { get; set; } 
}