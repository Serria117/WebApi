using MongoDB.Driver;
using WebApp.Enums;
using WebApp.Mongo.DocumentModel;

namespace WebApp.Mongo.MongoRepositories;

/// <summary>
/// Represents a repository for managing organization documents in a MongoDB collection.
/// Inherits from GenericMongoRepository and implements IOrgMongoRepository.
/// </summary>
/// <remarks>
/// Provides methods to insert a single organization document, check for the existence of an organization by ID,
/// and insert multiple organization documents.
/// </remarks>
public interface IOrgMongoRepository
{
    /// <summary>
    /// Asynchronously inserts a single organization document into the collection.
    /// </summary>
    /// <param name="org">The organization document to insert.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    Task InsertOrgId(OrgDoc org);

    /// <summary>
    /// Asynchronously checks if an organization with the specified ID exists in the database.
    /// </summary>
    /// <param name="orgId">The ID of the organization to check for existence.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains a boolean indicating whether the organization exists.</returns>
    Task<bool> ExistAsync(string orgId);

    /// <summary>
    /// Asynchronously inserts multiple organization documents into the collection.
    /// </summary>
    /// <param name="orgs">A list of organization documents to be inserted.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    Task InsertMany(List<OrgDoc> orgs);
}

public class OrgMongoRepository(IMongoDatabase db)
    : GenericMongoRepository<OrgDoc, string>(CollectionName.Organization, db),
      IOrgMongoRepository
{
    public async Task InsertOrgId(OrgDoc org)
    {
        await Collection.InsertOneAsync(org);
    }

    public async Task<bool> ExistAsync(string orgId)
    {
        return await Collection.CountDocumentsAsync(x => x.OrgId == orgId,
                                                    options: new CountOptions { Limit = 1 },
                                                    cancellationToken: CancellationToken.None) > 0;
    }

    public async Task InsertMany(List<OrgDoc> orgs)
    {
        await Collection.InsertManyAsync(orgs);
    }
}