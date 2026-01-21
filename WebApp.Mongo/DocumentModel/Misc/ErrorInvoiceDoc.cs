using System;
using System.Collections.Generic;
using System.Linq;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson;

namespace WebApp.Mongo.DocumentModel.Misc;

public class ErrorInvoiceDoc
{
    [BsonId]
    public ObjectId Id { get; set; }

    [BsonElement("invoiceNumber")]
    public int? InvoiceNumber { get; set; }

    [BsonElement("clientTaxId")]
    public string? OrgId { get; set; }

    [BsonElement("invoiceId")]
    public string? InvoiceId { get; set; }

    [BsonElement("BuyerTaxId")]
    public string? BuyerTaxId { get; set; }

    [BsonElement("sellerTaxId")]
    public string? SellerTaxId { get; set; }

    [BsonElement("createDate")]
    public DateTime? InvoiceDate { get; set; }

    [BsonElement("period")]
    public string? Period { get; set; }

    [BsonElement("content")]
    public string? Content { get; set; }

    [BsonElement("message")]
    public string? Message { get; set; }

    [BsonElement("type")]
    public int? Type { get; set; } = 0; // 0 = sold, 1 = purchase

    [BsonElement("createdDate")]
    public DateTime CreatedDate { get; set; } = DateTime.Now;

    [BsonElement("isSuccessRetry")]
    public bool? IsSuccessRetry { get; set; } = false;
}