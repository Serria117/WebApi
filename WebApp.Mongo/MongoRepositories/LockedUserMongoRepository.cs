using System.Linq.Expressions;
using MongoDB.Driver;
using WebApp.Enums;
using WebApp.Mongo.DocumentModel;

namespace WebApp.Mongo.MongoRepositories;

public interface ILockedUserMongoRepository
{
    /// <summary>
    /// Checks if a user is locked in the system.
    /// </summary>
    /// <param name="userId">The unique identifier of the user to check for a locked status.</param>
    /// <returns>Returns true if the user is locked; otherwise, false.</returns>
    Task<bool> IsUserLocked(Guid userId);

    /// <summary>
    /// Locks a user in the system by updating their locked status.
    /// </summary>
    /// <param name="userId">The unique identifier of the user to be locked.</param>
    /// <returns>An asynchronous task representing the operation.</returns>
    Task LockUser(Guid userId);

    /// <summary>
    /// Unlocks a user in the system by removing their locked status.
    /// </summary>
    /// <param name="userId">The unique identifier of the user to be unlocked.</param>
    /// <returns>An asynchronous task representing the operation.</returns>
    Task UnlockUser(Guid userId);

    Task<List<Guid>> GetAllLockedUserIds(CancellationToken cancellationToken = default);
}

public class LockedUserMongoRepository(IMongoDatabase db)
    : GenericMongoRepository<LockedUser, string>(CollectionName.LockedUsers, db), ILockedUserMongoRepository
{
    public async Task<bool> IsUserLocked(Guid userId)
    {
        var filter = Builders<LockedUser>.Filter.Eq(x => x.UserId, userId);
        var result = await Collection.CountDocumentsAsync(filter, new CountOptions { Limit = 1 });
        return result > 0;
    }

    public async Task LockUser(Guid userId)
    {
        var filter = Builders<LockedUser>.Filter.Eq(x => x.UserId, userId);
        var update = Builders<LockedUser>.Update.Set(x => x.LockedAt, DateTime.UtcNow);
        await Collection.UpdateOneAsync(filter, update, new UpdateOptions { IsUpsert = true });
    }

    public async Task UnlockUser(Guid userId)
    {
        var filter = Builders<LockedUser>.Filter.Eq(x => x.UserId, userId);
        await Collection.DeleteOneAsync(filter);
    }
    
    public async Task<List<Guid>> GetAllLockedUserIds(CancellationToken cancellationToken = default)
    {
        var users = await Collection.Find(_ => true).Project(x => x.UserId).ToListAsync(cancellationToken);
        return users;
    }
}