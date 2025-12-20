using MongoDB.Bson;
using MongoDB.Driver;
using WebApp.Enums;
using WebApp.Mongo.DocumentModel;
using WebApp.Mongo.DocumentModel.SoldInvoiceDetails;
using WebApp.Payloads;
using WebApp.Utils;

namespace WebApp.Services.InvoiceService;

public partial class InvoiceService
{
    /// <summary>
    /// Tìm người bán của đơn vị
    /// </summary>
    /// <param name="year"></param>
    /// <param name="keyword"></param>
    /// <returns></returns>
    public async Task<ResponseEntity> ScanOrganizationSeller(int? year, string? keyword)
    {
        var currentOrg = await dbContext.Organizations.FindAsync(WorkingOrg.ToGuid());
        if (currentOrg is null) return ResponseEntity.Error404("Organization not found");
        var filterBuilder = Builders<InvoiceDetailDoc>.Filter;
        var filter = filterBuilder.Eq(x => x.Nmmst, currentOrg.TaxId);
        filter &= filterBuilder.Ne(x => x.Nbmst, null);
        if (year.HasValue)
        {
            filter &= filterBuilder.Regex(x => x.Tdlap, $"^{year.Value}");
        }

        if (!string.IsNullOrWhiteSpace(keyword))
        {
            filter &= filterBuilder.Or(filterBuilder.Regex(x => x.Nbten, new BsonRegularExpression(keyword.TrimSpace(), "i")), 
                                       filterBuilder.Regex(x => x.Nbmst, new BsonRegularExpression(keyword.TrimSpace(), "i")));
        }
        var collection = mongoDatabase.GetCollection<InvoiceDetailDoc>(CollectionName.Invoice);
        List<int> statusForSum = [1, 2];
        var sellers = await collection.Aggregate()
                                             .Match(filter)
                                             .SortByDescending(x => x.Tdlap)
                                             .Group(x => x.Nbmst, g => new
                                             {
                                                 TaxId = g.Key,
                                                 Name = g.First().Nbten,
                                                 Address = g.First().Nbdchi,
                                                 Count = g.Count(),
                                                 Total = g.Where(v => v.Tthai != null
                                                                      && statusForSum.Contains(v.Tthai.Value))
                                                          .Sum(v => v.Tgtcthue)
                                             })
                                             .ToListAsync();
        return ResponseEntity.OkResult(sellers.OrderByDescending(x => x.Count).ToList());
    }

    public async Task<ResponseEntity> GetInvoiceBySeller(string sellerTaxId, int? year)
    {
        var currentOrg = await dbContext.Organizations.FindAsync(WorkingOrg.ToGuid());
        if (currentOrg is null) return ResponseEntity.Error404("Organization not found");
        
        var collection = mongoDatabase.GetCollection<InvoiceDetailDoc>(CollectionName.Invoice);
        var filterBuilder = Builders<InvoiceDetailDoc>.Filter;
        
        var filter = filterBuilder.And(filterBuilder.Eq(x => x.Nmmst, currentOrg.TaxId),
                                       filterBuilder.Eq(x => x.Nbmst, sellerTaxId));
        if (year.HasValue)
        {
            filter &= filterBuilder.Regex(x => x.Tdlap, $"^{year.Value}");
        }

        var invoices = await collection.Find(filter)
                                       .ToListAsync();
        
        return ResponseEntity.OkResult(invoices.Select(i => i.ToDisplayModel()));
    }

    /// <summary>
    /// Tìm người mua của đơn vị
    /// </summary>
    /// <param name="year"></param>
    /// <param name="keyword"></param>
    /// <returns></returns>
    public async Task<ResponseEntity> ScanOrganizationBuyer(int? year, string? keyword)
    {
        var currentOrg = await dbContext.Organizations.FindAsync(WorkingOrg.ToGuid());
        if (currentOrg is null) return ResponseEntity.Error404("Organization not found");

        var collection = mongoDatabase.GetCollection<SoldInvoiceDetail>(name: CollectionName.SoldInvoiceDetail);
        var filterBuilder = Builders<SoldInvoiceDetail>.Filter;
        var filter = filterBuilder.Eq(x => x.Nbmst, currentOrg.TaxId);
        filter &= filterBuilder.Ne(x => x.Nmmst, null);
        if (year.HasValue)
        {
            filter &= filterBuilder.Regex(x => x.Tdlap, $"^{year.Value}");
        }
        if (!string.IsNullOrWhiteSpace(keyword))
        {
            keyword = keyword.TrimSpace();
            filter &= filterBuilder.Or(filterBuilder.Regex(x => x.Nmten, new BsonRegularExpression(keyword, "i")), 
                                       filterBuilder.Regex(x => x.Nmmst, new BsonRegularExpression(keyword, "i")));
        }
        List<int> statusForSum = [1, 2];

        var buyers = await collection.Aggregate()
                                                .Match(filter)
                                                .SortByDescending(x => x.Tdlap)
                                                .Group(x => x.Nmmst, g => new
                                                {
                                                    TaxId = g.Key,
                                                    Name = g.First().Nmten,
                                                    Address = g.First().Nmdchi,
                                                    Count = g.Count(),
                                                    Total = g.Sum(v => v.Tthai.HasValue
                                                                       && statusForSum.Contains(v.Tthai.Value) 
                                                                      ? v.Tgtcthue : 0)
                                                })
                                                .ToListAsync();
        return ResponseEntity.OkResult(buyers.OrderByDescending(x => x.Count).ToList());
    }
}