using MongoDB.Driver;
using WebApp.Mongo;
using WebApp.Mongo.MongoRepositories;
using WebApp.Repositories;
using WebApp.Services;
using WebApp.Services.BalanceSheetService;
using WebApp.Services.CachingServices;
using WebApp.Services.DocumentService;
using WebApp.Services.InvoiceService;
using WebApp.Services.NotificationService;
using WebApp.Services.OrganizationService;
using WebApp.Services.RegionService;
using WebApp.Services.RestService;
using WebApp.Services.RiskCompanyService;
using WebApp.Services.UserService;

namespace WebApp.Register;

public static class DependencyRegister
{
    /// <summary>
    /// Configures MongoDB related services for dependency injection.
    /// </summary>
    /// <param name="s">The service collection to which the services are added.</param>
    /// <param name="settings">The MongoDB settings containing connection details.</param>
    /// <remarks>
    /// Registers MongoDB settings and client as singletons, and database and repositories as scoped services.
    /// </remarks>
    public static void AddMongoServices(this IServiceCollection s, MongoDbSettings settings)
    {
        s.AddSingleton(settings);
        s.AddSingleton<IMongoClient, MongoClient>(_ => new MongoClient(settings.ConnectionString));

        s.AddScoped<IMongoDatabase>(provider => provider.GetRequiredService<IMongoClient>()
                                                               .GetDatabase(settings.DatabaseName));
        s.AddScoped<IInvoiceMongoRepository, InvoiceMongoRepository>();
        s.AddScoped<IUserMongoRepository, UserMongoRepository>();
        s.AddScoped<ISoldInvoiceMongoRepository, SoldInvoiceMongoRepository>();
        s.AddScoped<IOrgMongoRepository, OrgMongoRepository>();
        s.AddScoped<IBlacklistedTokenMongoRepository, BlacklistedTokenMongoRepository>();
        s.AddScoped<IRefreshTokenMongoRepository, RefreshTokenMongoRepository>();
    }

    /// <summary>
    /// Configures application services for dependency injection.
    /// </summary>
    /// <param name="s">The service collection to which the services are added.</param>
    /// <remarks>
    /// Registers various application services, including notification, repository, and business services,
    /// with appropriate lifetimes such as singleton, scoped, and transient.
    /// </remarks>
    public static void AddAppServices(this IServiceCollection s)
    {
        //Add Singleton Services (like notification, caching, logging etc...) here:
        s.AddSingleton<INotificationAppService, NotificationAppService>();
        s.AddSingleton<ICachingRoleService, CachingRoleService>();

        
        //Add repositories services here:
        s.AddScoped(typeof(IAppRepository<,>), typeof(AppRepository<,>));
        s.AddScoped<IRestAppService, RestAppService>();
        s.AddScoped<IUserManager, UserManager>();
        s.AddScoped<IUnitOfWork, UnitOfWork>();
        
        //Add business services here:
        s.AddTransient<IUserAppService, UserAppAppService>();
        s.AddTransient<IRoleAppService, RoleAppService>();
        s.AddTransient<IPermissionAppService, PermissionAppService>();
        s.AddTransient<IOrganizationAppService, OrganizationAppService>();
        s.AddTransient<IInvoiceAppService, InvoiceAppService>();
        s.AddTransient<IRegionAppService, RegionAppService>();
        s.AddTransient<IRiskCompanyAppService, RiskCompanyAppService>();
        s.AddTransient<IBalanceSheetAppService, BalanceSheetAppService>();
        s.AddTransient<IDocumentAppService, DocumentAppService>();
    }
}