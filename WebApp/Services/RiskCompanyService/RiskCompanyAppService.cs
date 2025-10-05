using Microsoft.EntityFrameworkCore;
using WebApp.Core.DomainEntities;
using WebApp.Enums;
using WebApp.Payloads;
using WebApp.Repositories;
using WebApp.Services.InvoiceService.dto;
using WebApp.Services.UserService;
using X.Extensions.PagedList.EF;

namespace WebApp.Services.RiskCompanyService;

public interface IRiskCompanyAppService
{
    Task<ResponseBase> GetAsync(PageRequest page);
    Task<ResponseBase> CreateAsync(RiskCompany riskCompany);
    Task<ResponseBase> CreateManyAsync(List<RiskCompany> riskCompanies);
    Task<ResponseBase> CheckInvoicesAsync(List<InvoiceDisplayDto> invoices);
    Task<ResponseBase> SoftDeleteAsync(int id);
    Task<ResponseBase> SoftDeleteManyAsync(List<int> ids);
    bool IsInvoiceRisk(string? sellerTaxCode);
}

public class RiskCompanyBaseAppService(IAppRepository<RiskCompany, int> riskCompanyRepo,
                                   IUserManager userManager) : BaseAppService(userManager), IRiskCompanyAppService
{
    public async Task<ResponseBase> GetAsync(PageRequest page)
    {
        var riskList = await riskCompanyRepo.Find(filter: x => !x.Deleted
                                                       && (page.Keyword == null
                                                           || x.TaxId.Contains(page.Keyword)
                                                           || x.Name.Contains(page.Keyword)),
                                                  sortBy: nameof(RiskCompany.CreateAt), 
                                                  order: SortOrder.DESC
                                            )
                                            .ToPagedListAsync(page.Page, page.Size);

        return ResponseBase.OkResult(riskList);
    }

    public async Task<ResponseBase> CreateAsync(RiskCompany riskCompany)
    {
        return ResponseBase.OkResult(await riskCompanyRepo.CreateAsync(riskCompany));
    }

    public async Task<ResponseBase> CreateManyAsync(List<RiskCompany> riskCompanies)
    {
        var riskList = await riskCompanyRepo.Find(x => !x.Deleted)
                                            .Select(x => x.TaxId)
                                            .ToListAsync();
        riskCompanies = riskCompanies.Where(x => !riskList.Contains(x.TaxId)).ToList();
        await riskCompanyRepo.CreateManyAsync(riskCompanies);
        return ResponseBase.Ok();
    }

    public async Task<ResponseBase> CheckInvoicesAsync(List<InvoiceDisplayDto> invoices)
    {
        var riskList = await riskCompanyRepo.Find(x => !x.Deleted)
                                            .Select(x => x.TaxId)
                                            .ToListAsync();
        var positiveInv = invoices.Where(invoice => riskList.Contains(invoice.SellerTaxCode))
                                  .ToList();

        return ResponseBase.OkResult(positiveInv);
    }

    public bool IsInvoiceRisk(string? sellerTaxCode)
    {
        if (string.IsNullOrEmpty(sellerTaxCode)) return false;
        var riskList = riskCompanyRepo.Find(x => !x.Deleted).Select(x => x.TaxId).ToHashSet();
        return riskList.Contains(sellerTaxCode);
    }

    public async Task<ResponseBase> SoftDeleteAsync(int id)
    {
        var deleteResult = await riskCompanyRepo.SoftDeleteAsync(id);
        return deleteResult ? ResponseBase.Ok() : ResponseBase.Error($"Failed to delete Id: {id}");
    }

    public async Task<ResponseBase> SoftDeleteManyAsync(List<int> ids)
    {
        var deleteResult = await riskCompanyRepo.SoftDeleteManyAsync(ids.ToArray());
        return deleteResult ? ResponseBase.Ok() : ResponseBase.Error("Failed to delete");
    }
}