using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace WebApp.Mongo.DocumentModel;

public class BlacklistedTokenDoc
{
    [BsonId]
    public ObjectId Id { get; set; }

    public string Token { get; set; } = string.Empty;
    public DateTime ExpTime { get; set; }
}