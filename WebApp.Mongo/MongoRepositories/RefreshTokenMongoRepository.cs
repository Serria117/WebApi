using System.Linq.Expressions;
using MongoDB.Driver;
using WebApp.Enums;
using WebApp.Mongo.DocumentModel;

namespace WebApp.Mongo.MongoRepositories;

public interface IRefreshTokenMongoRepository
{
    Task<RefreshTokenDoc?> FindTokenAsync(string token);
    Task CreateTokenAsync(RefreshTokenDoc token);
    Task RevokeTokenAsync(string token, string revokeBy);
    Task CreateManyAsync(IEnumerable<RefreshTokenDoc> documents, Expression<Func<RefreshTokenDoc, bool>> filter);
}

public class RefreshTokenMongoRepository(IMongoDatabase db) 
    : GenericMongoRepository<RefreshTokenDoc, string>(CollectionName.RefreshToken, db), IRefreshTokenMongoRepository
{
    public async Task<RefreshTokenDoc?> FindTokenAsync(string token)
    {
        return await Collection.Find(x => x.Token == token && x.IsRevoked == false)
                               .FirstOrDefaultAsync();
    }

    public async Task CreateTokenAsync(RefreshTokenDoc token)
    {
        await Collection.InsertOneAsync(token);
    }

    public async Task RevokeTokenAsync(string token, string revokeBy)
    {
        var update = Builders<RefreshTokenDoc>.Update
                                              .Set(x => x.IsRevoked, true)
                                              .Set(x => x.RevokedByIp, revokeBy)
                                              .Set(x => x.RevokedAt, DateTime.UtcNow);
        await Collection.UpdateOneAsync(x => x.Token == token, update);
    }
}