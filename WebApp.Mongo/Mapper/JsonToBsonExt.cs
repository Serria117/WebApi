using System.Text.Json;
using MongoDB.Bson.Serialization;
using WebApp.Mongo.DeserializedModel;
using WebApp.Mongo.DocumentModel;

namespace WebApp.Mongo.Mapper;

public static class JsonToBsonExt
{
    public static InvoiceDetailDoc ToPurchaseInvoiceDetailBson(this InvoiceDetailModel model, JsonSerializerOptions? options = null)
    {
        var serialized = JsonSerializer.Serialize(model, options);
        return BsonSerializer.Deserialize<InvoiceDetailDoc>(serialized);
    }
    
    public static InvoiceDetailDoc ObjectToBson(this object model, JsonSerializerOptions? options = null)
    {
        var serialized = JsonSerializer.Serialize(model, options);
        return BsonSerializer.Deserialize<InvoiceDetailDoc>(serialized);
    }
    
    public static InvoiceDetailDoc ToPurchaseInvoiceDetailBson(this string model, JsonSerializerOptions? options = null)
    {
       // var serialized = JsonSerializer.Serialize(model, options);
        return BsonSerializer.Deserialize<InvoiceDetailDoc>(model);
    }

    public static SoldInvoiceDoc ToSoldInvoiceBson(this string model, JsonSerializerOptions? options = null)
    {
        return BsonSerializer.Deserialize<SoldInvoiceDoc>(model);
    }
}