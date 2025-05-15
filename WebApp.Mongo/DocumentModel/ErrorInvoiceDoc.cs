using System;
using System.Collections.Generic;
using System.Linq;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson;

namespace WebApp.Mongo.DocumentModel;
public class ErrorInvoiceDoc
{
    [BsonId]
    public ObjectId Id { get; set; }
    [BsonElement("invoiceNumber")]
    public int? InvoiceNumber { get; set; }
    [BsonElement("clientTaxId")]
    public string? ClientId { get; set; }
    [BsonElement("createDate")]
    public DateTime CreateDate { get; set; }
    [BsonElement("period")]
    public string? Period { get; set; }
    [BsonElement("content")]
    public string? Content { get; set; }
    public string? Message { get; set; }
    public int Type { get; set; } = 0; // 0 = sold, 1 = purchase
}
