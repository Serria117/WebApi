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
    Task<ResponseEntity> GetAsync(PageRequest page);
    Task<ResponseEntity> CreateAsync(RiskCompany riskCompany);
    Task<ResponseEntity> CreateManyAsync(List<RiskCompany> riskCompanies);
    Task<ResponseEntity> CheckInvoicesAsync(List<InvoiceDisplayDto> invoices);
    Task<ResponseEntity> SoftDeleteAsync(int id);
    Task<ResponseEntity> SoftDeleteManyAsync(List<int> ids);
    bool IsInvoiceRisk(string? sellerTaxCode);
}

public class RiskCompanyBaseAppService(IAppRepository<RiskCompany, int> riskCompanyRepo,
                                   IUserManager userManager) : BaseAppService(userManager), IRiskCompanyAppService
{
    public async Task<ResponseEntity> GetAsync(PageRequest page)
    {
        var riskList = await riskCompanyRepo.Find(filter: x => !x.Deleted
                                                       && (page.Keyword == null
                                                           || x.TaxId.Contains(page.Keyword)
                                                           || x.Name.Contains(page.Keyword)),
                                                  sortBy: nameof(RiskCompany.CreateAt), 
                                                  order: SortOrder.DESC
                                            )
                                            .ToPagedListAsync(page.Page, page.Size);

        return ResponseEntity.OkResult(riskList);
    }

    public async Task<ResponseEntity> CreateAsync(RiskCompany riskCompany)
    {
        return ResponseEntity.OkResult(await riskCompanyRepo.CreateAsync(riskCompany));
    }

    public async Task<ResponseEntity> CreateManyAsync(List<RiskCompany> riskCompanies)
    {
        var riskList = await riskCompanyRepo.Find(x => !x.Deleted)
                                            .Select(x => x.TaxId)
                                            .ToListAsync();
        riskCompanies = riskCompanies.Where(x => !riskList.Contains(x.TaxId)).ToList();
        await riskCompanyRepo.CreateManyAsync(riskCompanies);
        return ResponseEntity.Ok();
    }

    public async Task<ResponseEntity> CheckInvoicesAsync(List<InvoiceDisplayDto> invoices)
    {
        var riskList = await riskCompanyRepo.Find(x => !x.Deleted)
                                            .Select(x => x.TaxId)
                                            .ToListAsync();
        var positiveInv = invoices.Where(invoice => riskList.Contains(invoice.SellerTaxCode))
                                  .ToList();

        return ResponseEntity.OkResult(positiveInv);
    }

    public bool IsInvoiceRisk(string? sellerTaxCode)
    {
        if (string.IsNullOrEmpty(sellerTaxCode)) return false;
        var riskList = riskCompanyRepo.Find(x => !x.Deleted).Select(x => x.TaxId).ToHashSet();
        return riskList.Contains(sellerTaxCode);
    }

    public async Task<ResponseEntity> SoftDeleteAsync(int id)
    {
        var deleteResult = await riskCompanyRepo.SoftDeleteAsync(id);
        return deleteResult ? ResponseEntity.Ok() : ResponseEntity.Error($"Failed to delete Id: {id}");
    }

    public async Task<ResponseEntity> SoftDeleteManyAsync(List<int> ids)
    {
        var deleteResult = await riskCompanyRepo.SoftDeleteManyAsync(ids.ToArray());
        return deleteResult ? ResponseEntity.Ok() : ResponseEntity.Error("Failed to delete");
    }
}