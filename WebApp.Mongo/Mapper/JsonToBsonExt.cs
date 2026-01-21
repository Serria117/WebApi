using System.Text.Json;
using System.Text.RegularExpressions;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using WebApp.Mongo.DeserializedModel;
using WebApp.Mongo.DocumentModel;
using WebApp.Mongo.DocumentModel.PurchaseInvoices;

namespace WebApp.Mongo.Mapper;

public static class JsonToBsonExt
{
    public static InvoiceDetailDoc ToPurchaseInvoiceDetailBson(this InvoiceDetailModel invoiceValue, 
                                                               JsonSerializerOptions? options = null)
    {
        var serialized = JsonSerializer.Serialize(invoiceValue, options);
        return BsonSerializer.Deserialize<InvoiceDetailDoc>(serialized);
    }
    
    public static InvoiceDetailDoc ObjectToBson(this object model, JsonSerializerOptions? options = null)
    {
        var serialized = JsonSerializer.Serialize(model, options);
        return BsonSerializer.Deserialize<InvoiceDetailDoc>(serialized);
    }
    
    public static InvoiceDetailDoc ToPurchaseInvoiceDetailBson(this string stringValue)
    {
        //var json = Regex.Unescape(stringValue);
        var json = JsonSerializer.Deserialize<string>(stringValue);
        //Console.WriteLine("Print the string from converter to check:");
        //Console.WriteLine($"{json}");
        return BsonSerializer.Deserialize<InvoiceDetailDoc>(json);
    }

    public static SoldInvoiceDoc ToSoldInvoiceBson(this string model, JsonSerializerOptions? options = null)
    {
        return BsonSerializer.Deserialize<SoldInvoiceDoc>(model);
    }
}