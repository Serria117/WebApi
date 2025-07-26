using Microsoft.EntityFrameworkCore;
using WebApp.Core.DomainEntities.Accounting;
using WebApp.Payloads;
using WebApp.Repositories;
using WebApp.Services.CommonService;
using WebApp.Services.UserService;
using X.Extensions.PagedList.EF;

namespace WebApp.Services.InvoiceService;
public interface IInvoiceHistoryAppService
{
    Task<AppResponse> CreateHistoryAsync(DateTime from, DateTime to, 
                                         long totalFound, 
                                         long totalSuccess, 
                                         SyncType type, bool? success = true);
    Task<AppResponse> GetHistoryAsync(PageRequest req);

}

public class InvoiceHistoryAppService(IUserManager userManager,
    IAppRepository<InvoiceHistory, long> historyRepo) : BaseAppService(userManager), IInvoiceHistoryAppService
{
    // This class can be extended with methods to handle invoice history operations
    // For example, methods to retrieve, create, update, or delete invoice history records
    // It can utilize the UserManager for user-related operations if needed

    public async Task<AppResponse> GetHistoryAsync(PageRequest req)
    {
        var orgId = WorkingOrg.ToGuid();
        if (orgId == Guid.Empty)
        {
            return AppResponse.Error("Organization ID is not set or invalid.");
        }

        var result = await historyRepo
            .Find(h => h.OrganizationId == orgId)
            .OrderByDescending(h => h.CreateAt)
            .Select(h => new
            {
                h.Id,
                SyncType = h.SyncType,
                h.TotalFound,
                h.TotalSuccess,
                Failed = h.TotalFound - h.TotalSuccess,
                FromDate = h.FromDate,
                ToDate = h.ToDate,
                h.RetryTime,
                h.IsRetried,
                Completed = h.Completed,
                CreateAt = h.CreateAt,
                h.CreateBy
            })
            .ToPagedListAsync(req.Page, req.Size);

        return AppResponse.OkResult(result);
    }

    public async Task<AppResponse> CreateHistoryAsync(DateTime from, DateTime to, 
                                                      long totalFound, long totalSuccess, 
                                                      SyncType type, bool? success = true)
    {
        var history = new InvoiceHistory
        {
            OrganizationId = WorkingOrg.ToGuid(),
            SyncType = type,
            TotalFound = (int)totalFound,
            TotalSuccess = (int)totalSuccess,
            FromDate = from,
            ToDate = to,
            IsRetried = false,
            RetryTime = 0,
            Completed = success ?? totalFound == totalSuccess
        };
        await historyRepo.CreateAsync(history);
        return AppResponse.OkResult(history);
    }
}
