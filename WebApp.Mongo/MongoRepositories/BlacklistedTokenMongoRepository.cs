using MongoDB.Driver;
using WebApp.Enums;
using WebApp.Mongo.DocumentModel;

namespace WebApp.Mongo.MongoRepositories;

/// <summary>
/// Represents a repository for managing blacklisted tokens in a MongoDB collection.
/// </summary>
/// <remarks>
/// Inherits from <see cref="GenericMongoRepository{BlacklistedTokenDoc, string}"/> and implements
/// <see cref="IBlacklistedTokenMongoRepository"/> to provide specific operations for blacklisted tokens.
/// </remarks>
/// <param name="db">The MongoDB database instance used for accessing the collection.</param>
/// <method name="CheckBlacklist">
/// Asynchronously checks if a token is present in the blacklist.
/// </method>
/// <param name="token">The token to check against the blacklist.</param>
/// <returns>A task that represents the asynchronous operation. The task result contains a boolean indicating
/// whether the token is blacklisted.</returns>
/// <method name="AddTokenToBlackList">
/// Asynchronously adds a token to the blacklist.
/// </method>
/// <param name="token">The token to add to the blacklist.</param>
public interface IBlacklistedTokenMongoRepository
{
    /// <summary>
/// Checks if a given token is present in the blacklist collection.
/// </summary>
/// <param name="token">The token to check against the blacklist.</param>
/// <returns>A task that represents the asynchronous operation. The task result contains a boolean indicating whether the token is blacklisted.</returns>
    Task<bool> CheckBlacklist(string token);

    /// <summary>
    /// Asynchronously adds a token to the blacklist collection in the database.
    /// </summary>
    /// <param name="token">The token to be blacklisted.</param>
    /// <param name="expTime">The time token will get expired</param>
    Task AddTokenToBlackList(string token, DateTime expTime);

    /// <summary>
    /// Deletes all tokens from the collection that have expired based on the current UTC time.
    /// </summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    Task DeleteExpiredToken();
}

public class BlacklistedTokenMongoRepository(IMongoDatabase db)
    : GenericMongoRepository<BlacklistedTokenDoc, string>(CollectionName.BlacklistedToken, db), IBlacklistedTokenMongoRepository
{
    public async Task<bool> CheckBlacklist(string token)
    {
        return await Collection.CountDocumentsAsync(x => x.Token == token, 
                                                    options: new CountOptions { Limit = 1 }, 
                                                    cancellationToken: CancellationToken.None) > 0;
    }

    public async Task AddTokenToBlackList(string token, DateTime expTime)
    {
        if (expTime < DateTime.UtcNow)
        {
            await Collection.InsertOneAsync(new BlacklistedTokenDoc { Token = token, ExpTime = expTime });
        }
    }

    public async Task DeleteExpiredToken()
    {
        await Collection.DeleteManyAsync(x => x.ExpTime >= DateTime.UtcNow);
    }
}