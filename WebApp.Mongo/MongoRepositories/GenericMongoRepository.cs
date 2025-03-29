using System.Linq.Expressions;
using MongoDB.Driver;

namespace WebApp.Mongo.MongoRepositories;

public interface IGenericMongoRepository<T, in TKey>
{
    public Task<T?> FindOneAsync(Expression<Func<T, bool>> filter);
    Task CreateOneAsync(T document, Expression<Func<T, bool>> filter);
}

public class GenericMongoRepository<T, TKey>(string collectionName,
                                             IMongoDatabase database) : IGenericMongoRepository<T, TKey>
{
    protected readonly IMongoCollection<T> Collection = database.GetCollection<T>(collectionName);

    public async Task<T?> FindOneAsync(Expression<Func<T, bool>> filter)
    {
        return await Collection.Find(filter).FirstOrDefaultAsync();
    }

    public async Task CreateOneAsync(T document, Expression<Func<T, bool>> filter)
    {
        var found = await Collection.FindOneAndReplaceAsync(filter, document);
        if(found != null) return;
        await Collection.InsertOneAsync(document);
    }

    public async Task CreateManyAsync(IEnumerable<T> documents, Expression<Func<T, bool>> filter)
    {
        
    }
}