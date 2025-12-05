using MongoDB.Driver;
using WebApp.Mongo;
using WebApp.Mongo.MongoRepositories;
using WebApp.Queues;
using WebApp.Repositories;
using WebApp.Services.AccountingServices;
using WebApp.Services.BackgroundServices;
using WebApp.Services.BalanceSheetService;
using WebApp.Services.CachingServices;
using WebApp.Services.CommonService;
using WebApp.Services.DocumentService;
using WebApp.Services.EmailService;
using WebApp.Services.InvoiceService;
using WebApp.Services.LoggingService;
using WebApp.Services.NotificationService;
using WebApp.Services.OrganizationService;
using WebApp.Services.PayrollService;
using WebApp.Services.RegionService;
using WebApp.Services.RestService;
using WebApp.Services.RiskCompanyService;
using WebApp.Services.TaxDutyServices;
using WebApp.Services.TaxProcedureService;
using WebApp.Services.TemplateServices;
using WebApp.Services.UserService;
using WebApp.Services.UserService.AdminService;
using WebApp.Services.WorkDailyService;

namespace WebApp.Configuration;

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
    /// <param name="s">The service collection to which the s are added.</param>
    extension(IServiceCollection s)
    {
        /// <summary>
        /// Configures MongoDB related s for dependency injection.
        /// </summary>
        /// <param name="settings">The MongoDB settings containing connection details.</param>
        /// <remarks>
        /// Registers MongoDB settings and client as singletons, and database and repositories as scoped s.
        /// </remarks>
        public void AddMongoServices(MongoDbSettings settings)
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
            s.AddScoped<ILockedUserMongoRepository, LockedUserMongoRepository>();
        }

        /// <summary>
        /// Configures application s for dependency injection.
        /// </summary>
        /// <remarks>
        /// Registers various application s, including notification, repository, and business s,
        /// with appropriate lifetimes such as singleton, scoped, and transient.
        /// </remarks>
        public void AddAppServices()
        {
            //Add Singleton services (like notification, caching, logging etc...) here:
            s.AddSingleton<INotificationAppService, NotificationAppService>();
            s.AddSingleton<ICachingRoleService, CachingRoleService>();
            s.AddSingleton<IUserLogQueue, UserLogQueue>();
        
            //Add Http Context Accessor:
            s.AddHttpContextAccessor();
        
            //Add Memory Cache:
            s.AddMemoryCache();
        
            //Add background services here:
            s.AddHostedService<UserLogBackgroundService>();
            s.AddHostedService<LockedUserSyncService>();

            //Add repositories services here:
            s.AddScoped(typeof(IAppRepository<,>), typeof(AppRepository<,>));
            s.AddScoped<IRestAppService, RestBaseAppService>();
            s.AddScoped<IUnitOfWork, UnitOfWork>();

            //Add business services here:
            s.AddScoped<JwtService>();
            s.AddScoped<IUserManager, UserManager>();
            s.AddScoped<IUserAppService, UserBaseAppBaseAppService>();
            s.AddScoped<IRoleAppService, RoleAppService>();
            s.AddScoped<IPermissionAppService, PermissionBaseAppService>();
            s.AddScoped<IOrganizationAppService, OrganizationBaseAppService>();
            s.AddScoped<IInvoiceAppService, InvoiceAppService_Old>();
            s.AddScoped<IInvoiceService, InvoiceService>(); //the new invoice service
            s.AddScoped<IInvoiceImportService, InvoiceImportService>();
            s.AddScoped<IRegionAppService, RegionAppService>();
            s.AddScoped<IRiskCompanyAppService, RiskCompanyBaseAppService>();
            s.AddScoped<IFinancialStatementAppService, FinancialStatementAppService>();
            s.AddScoped<IDocumentAppService, DocumentBaseAppService>();
            s.AddScoped<ISoldInvoiceAppService, SoldInvoiceAppService>();
            s.AddScoped<IErrorInvoiceAppService, ErrorInvoiceBaseAppService>();
            s.AddScoped<IUserLogAppService, UserLogBaseAppService>();
            s.AddScoped<IAdminAppService, AdminBaseAppService>();
            s.AddScoped<IPayrollAppService, PayrollAppService>();
            s.AddScoped<IInvoiceHistoryAppService, InvoiceHistoryAppService>();
            s.AddScoped<IEmailAppService, EmailAppService>();
            s.AddScoped<ITaxProcedureAppService, TaxProcedureAppService>();
            s.AddScoped<ITemplateAppService, TemplateAppService>();
            s.AddScoped<ITaxDutyAppService, TaxDutyAppService>();
            s.AddScoped<ITaxDutyRecordAppService, TaxDutyRecordAppService>();
            s.AddScoped<ITaxDutyCategoryAppService, TaxDutyCategoryAppService>();
            s.AddScoped<IRedisCacheService, RedisCacheService>();
            s.AddScoped<IWorkDiaryAppService, WorkDiaryAppService>();
        }
    }
}