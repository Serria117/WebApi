using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace WebApp.Mongo.DocumentModel;

public class ClientDoc
{
    [BsonId]
    public ObjectId Id { get; set; }

    public string ClientId { get; set; } = string.Empty;
    public string? Token { get; set; }
    public DateTime TokenIssueTime { get; set; }
}