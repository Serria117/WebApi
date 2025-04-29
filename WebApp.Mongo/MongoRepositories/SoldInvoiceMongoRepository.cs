using MongoDB.Driver;
using MongoDB.Driver.Linq;
using WebApp.Enums;
using WebApp.Mongo.DeserializedModel;
using WebApp.Mongo.DocumentModel;
using WebApp.Mongo.DocumentModel.SoldInvoiceDetails;
using WebApp.Mongo.FilterBuilder;

namespace WebApp.Mongo.MongoRepositories;

public interface ISoldInvoiceMongoRepository
{
    Task<int> InsertInvoicesAsync(List<SoldInvoiceDoc> input);

    Task<PaginatedDocResult<SoldInvoiceDoc>> FindInvoices(FilterDefinition<SoldInvoiceDoc> filter,
                                                          int page, int size);
}

public class SoldInvoiceMongoRepository(IMongoDatabase database)
    : GenericMongoRepository<SoldInvoiceDoc, string>(CollectionName.SoldInvoice, database), ISoldInvoiceMongoRepository
{
    public async Task<int> InsertInvoicesAsync(List<SoldInvoiceDoc> input)
    {
        if (input.Count <= 0) return 0;
        var insertList = input.Where(x => !InvoiceExists(x.Id, x.Nbmst)).ToList();
        if (insertList.Count == 0) return 0;

        //Console.WriteLine($"Inserting {insertList.Count}/{input.Count} invoices...");
        await Collection.InsertManyAsync(insertList);

        //Console.WriteLine($"Finished inserting {insertList.Count} invoices.");
        return insertList.Count;
    }

    public async Task<PaginatedDocResult<SoldInvoiceDoc>> FindInvoices(FilterDefinition<SoldInvoiceDoc> filter,
                                                                       int page, int size)
    {
        var sortFilter = Builders<SoldInvoiceDoc>.Sort.Ascending("tdlap");
        var queryDocument = Collection.Find(filter)
                                      .Sort(sortFilter)
                                      .Paging(page, size)
                                      .ToListAsync();

        var countDocument = Collection.CountDocumentsAsync(filter);

        await Task.WhenAll(queryDocument, countDocument);
        return new PaginatedDocResult<SoldInvoiceDoc>
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

    private bool InvoiceExists(string? id, string? taxId = null)
    {
        var filter = Builders<SoldInvoiceDoc>.Filter.Eq(x => x.Id, id)
                     & Builders<SoldInvoiceDoc>.Filter.Eq(x => x.Nbmst, taxId);
        return Collection.Find(filter).Any();
    }
}