using MongoDB.Bson;
using MongoDB.Driver;
using WebApp.Enums;
using WebApp.Mongo.DocumentModel;

namespace WebApp.Mongo.MongoRepositories;

public interface IUserMongoRepository
{
    Task InsertUser(UserDoc user);
    Task<UserDoc> GetUser(Guid uId);
    Task UpdateUser(UserDoc user);
    Task UpdateAllUser(List<UserDoc> users);
}

public class UserMongoRepository(IMongoDatabase db) 
    : GenericMongoRepository<UserDoc, string>(CollectionName.User, db), IUserMongoRepository
{
    public async Task InsertUser(UserDoc user)
    {
        var found = await Collection.FindOneAndReplaceAsync(x => x.UserId == user.UserId, user);
        if (found != null) return;
        await Collection.InsertOneAsync(user);
    }

    public async Task<UserDoc> GetUser(Guid uId)
    {
        return await Collection.Find(x => x.UserId == uId.ToString()).FirstOrDefaultAsync();
    }
    
    public async Task UpdateUser(UserDoc user)
    {
        await Collection.UpdateOneAsync(filter: x => x.UserId == user.UserId,
                                        Builders<UserDoc>.Update.Set(d => d.Permissions, user.Permissions));
    }

    public async Task UpdateAllUser(List<UserDoc> users)
    {
        var updateModels = new List<WriteModel<UserDoc>>();
        foreach (var user in users)
        {
            var filter = Builders<UserDoc>.Filter.Eq(u => u.UserId, user.UserId);
            var update = Builders<UserDoc>.Update.Set(u => u.Permissions, user.Permissions);

            var updateModel = new UpdateOneModel<UserDoc>(filter, update);
            updateModels.Add(updateModel);
        }

        await Collection.BulkWriteAsync(updateModels);
    }
};