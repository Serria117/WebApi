using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace WebApp.Mongo.DocumentModel;

public class RefreshTokenDoc
{
    [BsonId] [BsonRepresentation(BsonType.ObjectId)]
    public ObjectId Id { get; set; }

    public string Token { get; set; } = string.Empty;

    public string UserId { get; set; } = string.Empty;

    public DateTime IssuedAt { get; set; }

    public DateTime ExpiresAt { get; set; }

    public bool IsRevoked { get; set; }

    public string? RevokedByIp { get; set; }

    public DateTime? RevokedAt { get; set; }
}