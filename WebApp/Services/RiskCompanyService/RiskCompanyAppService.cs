﻿using Microsoft.EntityFrameworkCore;
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
    Task<AppResponse> GetAsync(PageRequest page);
    Task<AppResponse> CreateAsync(RiskCompany riskCompany);
    Task<AppResponse> CreateManyAsync(List<RiskCompany> riskCompanies);
    Task<AppResponse> CheckInvoicesAsync(List<InvoiceDisplayDto> invoices);
    Task<AppResponse> SoftDeleteAsync(int id);
    Task<AppResponse> SoftDeleteManyAsync(List<int> ids);
    bool IsInvoiceRisk(string? sellerTaxCode);
}

public class RiskCompanyAppService(IAppRepository<RiskCompany, int> riskCompanyRepo,
                                   IUserManager userManager) : AppServiceBase(userManager), IRiskCompanyAppService
{
    public async Task<AppResponse> GetAsync(PageRequest page)
    {
        var riskList = await riskCompanyRepo.Find(filter: x => !x.Deleted
                                                       && (page.Keyword == null
                                                           || x.TaxId.Contains(page.Keyword)
                                                           || x.Name.Contains(page.Keyword)),
                                                  sortBy: nameof(RiskCompany.CreateAt), 
                                                  order: SortOrder.DESC
                                            )
                                            .ToPagedListAsync(page.Number, page.Size);

        return AppResponse.SuccessResponse(riskList);
    }

    public async Task<AppResponse> CreateAsync(RiskCompany riskCompany)
    {
        return AppResponse.SuccessResponse(await riskCompanyRepo.CreateAsync(riskCompany));
    }

    public async Task<AppResponse> CreateManyAsync(List<RiskCompany> riskCompanies)
    {
        var riskList = await riskCompanyRepo.Find(x => !x.Deleted)
                                            .Select(x => x.TaxId)
                                            .ToListAsync();
        riskCompanies = riskCompanies.Where(x => !riskList.Contains(x.TaxId)).ToList();
        await riskCompanyRepo.CreateManyAsync(riskCompanies);
        return AppResponse.Ok();
    }

    public async Task<AppResponse> CheckInvoicesAsync(List<InvoiceDisplayDto> invoices)
    {
        var riskList = await riskCompanyRepo.Find(x => !x.Deleted)
                                            .Select(x => x.TaxId)
                                            .ToListAsync();
        var positiveInv = invoices.Where(invoice => riskList.Contains(invoice.SellerTaxCode))
                                  .ToList();

        return AppResponse.SuccessResponse(positiveInv);
    }

    public bool IsInvoiceRisk(string? sellerTaxCode)
    {
        if (string.IsNullOrEmpty(sellerTaxCode)) return false;
        var riskList = riskCompanyRepo.Find(x => !x.Deleted).Select(x => x.TaxId).ToHashSet();
        return riskList.Contains(sellerTaxCode);
    }

    public async Task<AppResponse> SoftDeleteAsync(int id)
    {
        var deleteResult = await riskCompanyRepo.SoftDeleteAsync(id);
        return deleteResult ? AppResponse.Ok() : AppResponse.Error($"Failed to delete Id: {id}");
    }

    public async Task<AppResponse> SoftDeleteManyAsync(List<int> ids)
    {
        var deleteResult = await riskCompanyRepo.SoftDeleteManyAsync(ids.ToArray());
        return deleteResult ? AppResponse.Ok() : AppResponse.Error("Failed to delete");
    }
}