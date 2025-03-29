using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace WebApp.Mongo.DocumentModel;

public class UserDoc
{
    [BsonId]
    public ObjectId Id { get; set; }
    public string UserId { get; set; } = string.Empty;
    public HashSet<string> Permissions { get; set; } = [];
}
