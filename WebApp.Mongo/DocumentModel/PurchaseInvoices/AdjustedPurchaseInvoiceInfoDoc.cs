using MongoDB.Bson.Serialization.Attributes;
using System;
using System.Collections.Generic;
using System.Text;

namespace WebApp.Mongo.DocumentModel.PurchaseInvoices;

[BsonIgnoreExtraElements]
public class AdjustedPurchaseInvoiceInfoDoc
{
    [BsonElement("sellerTaxCode")]
    public string SellerTaxCode { get; set; } = string.Empty;

    [BsonElement("buyerTaxCode")]	
    public string BuyerTaxCode { get; set; } = string.Empty;

    [BsonElement("adjusted")]
    public AdjustedInfo Adjusted { get; set; } = null!;

    [BsonElement("original")]
	public OriginalInfo Original { get; set; } = null!;
}

public class AdjustedInfo
{
    [BsonElement("shdon")]
    public int? Shdon { get; set; }
    [BsonElement("khhdon")]
	public string? Khhdon { get; set; }
    [BsonElement("khmshdon")]
	public int? Khmshdon { get; set; }
    [BsonElement("tdlap")]
	public DateTime? Tdlap { get; set; }
}

public class OriginalInfo
{
    [BsonElement("shdon")]
    public int? Shdon { get; set; }
    [BsonElement("khhdon")]
    public string? Khhdon { get; set; }
    [BsonElement("khmshdon")]
    public int? Khmshdon { get; set; }
    [BsonElement("tdlap")]
    public DateTime? Tdlap { get; set; }
}