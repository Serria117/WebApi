using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;
using System.Text.Json;
using WebApp.Core.DomainEntities;

namespace WebApp.Core.DomainEntities;

[Table("INV_PurchaseInvoice")]
[
	Index(nameof(OrganizationId)),
	Index(nameof(SellerTaxId)),
	Index(nameof(SellerName)),
	Index(nameof(OrganizationId), nameof(SellerTaxId), nameof(InvoiceNumber))
]
public class PurchaseInvoice : BaseEntityAuditable<Guid>
{
	public int InvoiceNumber { get; set; }
	public string InvoiceNotation { get; set; } = string.Empty;
	public string InvoiceGroupNotation { get; set; } = string.Empty;

	public Guid OrganizationId { get; set; }

	[MaxLength(20)]
	public string BuyerTaxId { get; set; } = string.Empty;
	[MaxLength(500)]
	public string BuyerName { get; set; } = string.Empty;
	[MaxLength(500)]
	public string BuyerNameIndividual { get; set; } = string.Empty;
	[MaxLength(500)]
	public string? BuyerAddress { get; set; } = string.Empty;

	[MaxLength(20)]
	public string SellerTaxId { get; set; } = string.Empty;
	[MaxLength(500)]
	public string SellerName { get; set; } = string.Empty;

	[Column(TypeName = "decimal(18, 2)")]
	public decimal TotalBeforeTax { get; set; }

	[Column(TypeName = "decimal(18, 2)")]
	public decimal TotalTax { get; set; }

	[Column(TypeName = "decimal(18, 2)")]
	public decimal TotalFee { get; set; }

	[Column(TypeName = "decimal(18, 2)")]
	public decimal TotalWithTax { get; set; }

	public DateTime IssueDate { get; set; }
	public DateTime? SignDate { get; set; }

	public InvoiceType InvoiceType { get; set; }
	public InvoiceStatus InvoiceStatus { get; set; }

	public string? VerifyCode { get; set; } //mã CQT

	public bool RiskSeller { get; set; } = false; //Mark seller as potential risk
	public bool SuccessRetrieveData { get; set; } = true; //Successfully retrieve data from tax authority

	[Column(TypeName = "jsonb")]
	public ReferencePurchaseInvoice? ReferencePurchaseInvoice { get; set; } //Thông tin hóa đơn tham chiếu (nếu có)


}

public enum InvoiceStatus
{
	HoaDonMoi = 1,
	HoaDonThayThe = 2,
	HoaDonDieuChinh = 3,
	HoaDonBiThayThe = 4,
	HoaDonBiDieuChinh = 5,
	HoaDonBiHuy = 6,
}

public enum InvoiceType
{
	HaveAuthorityCode = 5,
	NoAuthorityCode = 6,
	FromCashRegister = 8,
}

public class ReferencePurchaseInvoice
{
	public Guid PurchaseInvoiceId { get; set; }
	public string InvoiceNotation { get; set; } = string.Empty;
	public string InvoiceGroupNotation { get; set; } = string.Empty;
	public int InvoiceNumber { get; set; }
	public string SellerTaxCode { get; set; } = string.Empty;
	public InvoiceStatus InvoiceStatus { get; set; }
}
