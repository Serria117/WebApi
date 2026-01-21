using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MongoDB.Driver;
using WebApp.Mongo.DocumentModel.Misc;

namespace WebApp.Mongo.MongoRepositories;

/// <summary>
/// Repository for ErrorInvoice documents.
/// </summary>
public interface IErrorInvoiceRepository
{
    Task InsertAsync(ErrorInvoiceDoc errorInvoiceDoc);
    Task<List<ErrorInvoiceDoc>> FindByOrganizationAsync(string clientId, int type);
    Task InsertManyAsync(ICollection<ErrorInvoiceDoc> collection);
}

public class ErrorInvoiceRepository(IMongoDatabase database)
    : GenericMongoRepository<ErrorInvoiceDoc, string>("ErrorInvoices", database), IErrorInvoiceRepository
{
    public async Task InsertAsync(ErrorInvoiceDoc errorInvoiceDoc)
    {
        await Collection.InsertOneAsync(errorInvoiceDoc);
    }

    public async Task InsertManyAsync(ICollection<ErrorInvoiceDoc> collection)
    {
        await Collection.InsertManyAsync(collection);
    }

    public async Task<List<ErrorInvoiceDoc>> FindByOrganizationAsync(string clientId, int type)
    {
        var filter = Builders<ErrorInvoiceDoc>.Filter.Eq(x => x.OrgId, clientId);
        filter &= Builders<ErrorInvoiceDoc>.Filter.Eq(x => x.Type, type);
        var result = await Collection.FindAsync(filter);
        return result.ToList();
    }
}
