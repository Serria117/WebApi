using WebApp.Mongo.DocumentModel;
using WebApp.Mongo.MongoRepositories;
using WebApp.Services.UserService;

namespace WebApp.Services.InvoiceService;

public interface IErrorInvoiceAppService
{
    Task<List<ErrorInvoiceDoc>> FindInvoiceAsync(string orgId, int type);
}

public class ErrorInvoiceBaseAppService(IUserManager userManager,
    IErrorInvoiceRepository errorInvoiceRepository) : BaseAppService(userManager), IErrorInvoiceAppService
{
    public async Task<List<ErrorInvoiceDoc>> FindInvoiceAsync(string orgId, int type)
    {
        return await errorInvoiceRepository.FindByOrganizationAsync(orgId, type);
    }

}
