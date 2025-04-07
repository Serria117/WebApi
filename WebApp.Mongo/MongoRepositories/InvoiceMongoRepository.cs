using System.Globalization;
using System.Linq.Expressions;
using System.Text.RegularExpressions;
using MongoDB.Bson;
using MongoDB.Driver;
using WebApp.Enums;
using WebApp.Mongo.DeserializedModel;
using WebApp.Mongo.DocumentModel;
using WebApp.Mongo.FilterBuilder;

namespace WebApp.Mongo.MongoRepositories;

public interface IInvoiceMongoRepository
{
    bool InvoiceExists(string? invoiceId, string? taxId = null);
    Task<HashSet<string>> GetExistingInvoiceIdsAsync(List<string> ids, string? taxCode = null);

    Task<PaginatedDocResult<InvoiceDetailDoc>> FindInvoices(FilterDefinition<InvoiceDetailDoc> filter,
                                                            int page, int size);

    Task<bool> InsertInvoicesAsync(List<InvoiceDetailDoc> input);
    Task<InvoiceDetailDoc?> FindOneAsync(Expression<Func<InvoiceDetailDoc, bool>> filter);
    Task CreateOneAsync(InvoiceDetailDoc document, Expression<Func<InvoiceDetailDoc, bool>> filter);
    Task CreateManyAsync(IEnumerable<InvoiceDetailDoc> documents, Expression<Func<InvoiceDetailDoc, bool>> filter);
    Task<long> UpdateInvoiceStatus(string invId, int status);
}

public class InvoiceMongoRepository(IMongoDatabase db)
    : GenericMongoRepository<InvoiceDetailDoc, string>(CollectionName.Invoice, db), IInvoiceMongoRepository
{
    public bool InvoiceExists(string? invoiceId, string? taxId = null)
    {
        if (string.IsNullOrEmpty(invoiceId)) return false;

        if (!string.IsNullOrEmpty(taxId))
        {
            return Collection.CountDocuments(x => x.Id == invoiceId && x.Nmmst == taxId,
                                             new CountOptions { Limit = 1 }) > 0;
        }
        return Collection.CountDocuments(x => x.Id == invoiceId, 
                                         new CountOptions { Limit = 1 }) > 0;
    }

    public async Task<long> UpdateInvoiceStatus(string invId, int status)
    {
        var filter = Builders<InvoiceDetailDoc>.Filter.And(
            Builders<InvoiceDetailDoc>.Filter.Eq(i => i.Id, invId),
            Builders<InvoiceDetailDoc>.Filter.Ne(i => i.Tthai, status));

        var result =
            await Collection.UpdateOneAsync(filter, Builders<InvoiceDetailDoc>.Update.Set(i => i.Tthai, status));
        return result.IsModifiedCountAvailable ? result.ModifiedCount : 0;
    }

    public async Task<HashSet<string>> GetExistingInvoiceIdsAsync(List<string> ids, string? taxCode = null)
    {
        HashSet<string> result = [];
        var filter = Builders<InvoiceDetailDoc>.Filter.In(x => x.Id, ids);
        if (taxCode != null)
        {
            filter &= Builders<InvoiceDetailDoc>.Filter.Eq(x => x.Nmmst, taxCode);
        }

        var dupplicatedIds = await Collection.Find(filter).Project(d => d.Id!).ToListAsync();
        if (dupplicatedIds is not null && dupplicatedIds.Count != 0)
        {
            result.UnionWith(dupplicatedIds);
        }

        return result;
    }

    public async Task<PaginatedDocResult<InvoiceDetailDoc>> FindInvoices(FilterDefinition<InvoiceDetailDoc> filter,
                                                                         int page, int size)
    {
        var sortFilter = Builders<InvoiceDetailDoc>.Sort.Ascending("tdlap");

        var queryDocument = Collection.Find(filter)
                                      .Sort(sortFilter)
                                      .Paging(page, size)
                                      .ToListAsync();

        var countDocument = Collection.CountDocumentsAsync(filter);

        await Task.WhenAll(countDocument, queryDocument);

        return new PaginatedDocResult<InvoiceDetailDoc>
        {
            Data = queryDocument.Result,
            Total = countDocument.Result,
            Page = page,
            Size = size,
            PageCount = (int)(countDocument.Result % size == 0
                ? countDocument.Result / size
                : countDocument.Result / size + 1),
        };
    }

    public async Task<bool> InsertInvoicesAsync(List<InvoiceDetailDoc> input)
    {
        if (input.Count <= 0) return false;

        Console.WriteLine($"Inserting {input.Count} invoices...");
        await Collection.InsertManyAsync(input);
        Console.WriteLine($"Finished inserting {input.Count} invoices.");
        return true;
    }
}