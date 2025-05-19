namespace WebApp.Mongo.DocumentModel;

public class LockedUser
{
    public Guid UserId { get; set; }
    public DateTime LockedAt { get; set; } = DateTime.UtcNow;
}