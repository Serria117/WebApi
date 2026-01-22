using WebApp.Core.Data;
using WebApp.Services.RestService;
using WebApp.Services.UserService;

namespace WebApp.Services.InvoiceService;

public class InvoiceService2(IUserManager userManager,
                             IRestAppService apiAppService,
                             AppDbContext dbContext) : BaseAppService(userManager)
{
    
}
