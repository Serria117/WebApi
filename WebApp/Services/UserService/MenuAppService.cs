using WebApp.Core.DomainEntities;
using WebApp.Payloads;
using WebApp.Repositories;

namespace WebApp.Services.UserService;

public class MenuAppService(IUserManager userManager, 
                            IAppRepository<Permission, int> permissionRepo,
                            IAppRepository<Role, int> roleRepo,
                            IAppRepository<MenuItem, int> menuRepo) 
    : AppServiceBase(userManager)
{
   
}