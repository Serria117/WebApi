using MongoDB.Driver;
using WebApp.Mongo.DeserializedModel;
using WebApp.Mongo.DocumentModel.SoldInvoiceDetails;
using WebApp.Mongo.FilterBuilder;

namespace WebApp.Mongo.MongoRepositories;

public interface ISoldInvoiceDetailRepository
{
    /// <summary>
    /// Inserts multiple sold invoice details into the database.
    /// </summary>
    /// <param name="invoiceList">The list of sold invoice details to be inserted into the database.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains an integer representing the number of inserted invoices.</returns>
    Task<int> InsertManyInvoiceAsync(List<SoldInvoiceDetail> invoiceList);

    /// <summary>
    /// Finds and retrieves a paginated list of sold invoice details based on the provided filter and pagination parameters.
    /// </summary>
    /// <param name="filter">The filter definition used to query the sold invoice details.</param>
    /// <param name="page">The page number to retrieve.</param>
    /// <param name="size">The number of results to retrieve per page.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains a paginated list of sold invoice details.</returns>
    Task<PaginatedDocResult<SoldInvoiceDetail>> FindInvoiceAsync(FilterDefinition<SoldInvoiceDetail> filter,
                                                                 int page, int size);

    /// <summary>
    /// Checks if an invoice exists in the database based on the provided filter criteria.
    /// </summary>
    /// <param name="filter">The filter definition to match the criteria of the invoice to be checked.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains a boolean value indicating whether the invoice exists (true) or not (false).</returns>
    Task<bool> InvoiceExist(FilterDefinition<SoldInvoiceDetail> filter);
}

public class SoldInvoiceDetailRepository(IMongoDatabase database)
    : GenericMongoRepository<SoldInvoiceDetail, string>(collectionName: "SoldInvoiceDetail",
                                                        database: database), ISoldInvoiceDetailRepository
{
    public async Task<int> InsertManyInvoiceAsync(List<SoldInvoiceDetail> invoiceList)
    {
        if (invoiceList.Count <= 0) return 0;
        await Collection.InsertManyAsync(invoiceList);
        return invoiceList.Count; // If no exception is thrown, all documents were inserted
    }

    public async Task<PaginatedDocResult<SoldInvoiceDetail>> FindInvoiceAsync(
        FilterDefinition<SoldInvoiceDetail> filter, int page, int size)
    {
        var sortFilter = Builders<SoldInvoiceDetail>.Sort.Ascending("tdlap")
                                                    .Ascending("shdon");
        var countDocument = Collection.CountDocumentsAsync(filter);
        var queryDocument = Collection.Find(filter)
                                      .Sort(sortFilter)
                                      .Paging(page, size)
                                      .ToListAsync();
        await Task.WhenAll(countDocument, queryDocument);

        return new PaginatedDocResult<SoldInvoiceDetail>
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


    public async Task<bool> InvoiceExist(FilterDefinition<SoldInvoiceDetail> filter)
    {
        return await Collection.CountDocumentsAsync(filter, new CountOptions { Limit = 1 }) > 0;
    }
}