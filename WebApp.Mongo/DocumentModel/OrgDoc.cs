using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace WebApp.Mongo.DocumentModel;

public class OrgDoc
{
    [BsonId]
    public ObjectId Id { get; set; }

    public string OrgId { get; set; } = string.Empty;
}