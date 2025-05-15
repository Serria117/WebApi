using MongoDB.Driver;
using WebApp.Mongo;
using WebApp.Mongo.MongoRepositories;
using WebApp.Queues;
using WebApp.Repositories;
using WebApp.Services;
using WebApp.Services.BackgroundServices;
using WebApp.Services.BalanceSheetService;
using WebApp.Services.CachingServices;
using WebApp.Services.DocumentService;
using WebApp.Services.InvoiceService;
using WebApp.Services.LoggingService;
using WebApp.Services.NotificationService;
using WebApp.Services.OrganizationService;
using WebApp.Services.RegionService;
using WebApp.Services.RestService;
using WebApp.Services.RiskCompanyService;
using WebApp.Services.UserService;
using WebApp.Services.UserService.AdminService;

namespace WebApp.Register;

/// <summary>
/// Provides methods to configure dependency injection for both MongoDB s
/// and general application s.
/// </summary>
/// <remarks>
/// This static class is responsible for registering the required s
/// into the application's dependency injection container.
/// </remarks>
public static class DependencyInjector
{
    /// <summary>
    /// Configures MongoDB related s for dependency injection.
    /// </summary>
    /// <param name="s">The service collection to which the s are added.</param>
    /// <param name="settings">The MongoDB settings containing connection details.</param>
    /// <remarks>
    /// Registers MongoDB settings and client as singletons, and database and repositories as scoped s.
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
        s.AddScoped<ISoldInvoiceDetailRepository, SoldInvoiceDetailRepository>();
        s.AddScoped<IErrorInvoiceRepository, ErrorInvoiceRepository>();
    }

    /// <summary>
    /// Configures application s for dependency injection.
    /// </summary>
    /// <param name="s">The service collection to which the s are added.</param>
    /// <remarks>
    /// Registers various application s, including notification, repository, and business s,
    /// with appropriate lifetimes such as singleton, scoped, and transient.
    /// </remarks>
    public static void AddAppServices(this IServiceCollection s)
    {
        //Add Singleton services (like notification, caching, logging etc...) here:
        s.AddSingleton<INotificationAppService, NotificationAppService>();
        s.AddSingleton<ICachingRoleService, CachingRoleService>();
        s.AddSingleton<IUserLogQueue, UserLogQueue>();

        //Add background services here:
        s.AddHostedService<UserLogBackgroundService>();

        //Add repositories services here:
        s.AddScoped(typeof(IAppRepository<,>), typeof(AppRepository<,>));
        s.AddScoped<IRestAppService, RestAppService>();
        s.AddScoped<IUnitOfWork, UnitOfWork>();

        //Add business services here:
        s.AddScoped<IUserManager, UserManager>();
        s.AddScoped<IUserAppService, UserAppAppService>();
        s.AddScoped<IRoleAppService, RoleAppService>();
        s.AddScoped<IPermissionAppService, PermissionAppService>();
        s.AddScoped<IOrganizationAppService, OrganizationAppService>();
        s.AddScoped<IInvoiceAppService, InvoiceAppService>();
        s.AddScoped<IRegionAppService, RegionAppService>();
        s.AddScoped<IRiskCompanyAppService, RiskCompanyAppService>();
        s.AddScoped<IBalanceSheetAppService, BalanceSheetAppService>();
        s.AddScoped<IDocumentAppService, DocumentAppService>();
        s.AddScoped<ISoldInvoiceAppService, SoldInvoiceAppService>();
        s.AddScoped<IErrorInvoiceAppService, ErrorInvoiceAppService>();
        s.AddScoped<IUserLogAppService, UserLogAppService>();
        s.AddScoped<IAdminAppService, AdminAppService>();

    }
}