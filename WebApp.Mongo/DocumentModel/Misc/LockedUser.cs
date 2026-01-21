using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace WebApp.Mongo.DocumentModel.Misc;

public class LockedUser
{
    [BsonGuidRepresentation(GuidRepresentation.CSharpLegacy)]
    public Guid UserId { get; set; }
    public DateTime LockedAt { get; set; } = DateTime.UtcNow;
}