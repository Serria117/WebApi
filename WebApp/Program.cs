using System.Reflection;
using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using RestSharp;
using WebApp;
using WebApp.Authentication;
using WebApp.Core.Data;
using WebApp.Core.DomainEntities;
using WebApp.Mongo;
using WebApp.Register;
using WebApp.Services.CommonService;
using WebApp.Services.Mappers;
using WebApp.Services.RestService;
using WebApp.SignalrConfig;
using Serilog;
using WebApp.GlobalExceptionHandler;
using WebApp.ScheduleTask;
using WebApp.Services;
using WebApp.Services.CachingServices;

// Declare variables.
var builder = WebApplication.CreateBuilder(args);
var services = builder.Services;
var config = builder.Configuration;
var jwtKey = config["JwtSettings:SecretKey"];

var restSettings = config.GetSection("RestSharp").Get<RestSharpSetting>()!;
var mongoSettings = config.GetSection("MongoDbSettings").Get<MongoDbSettings>()!;
var origins = config.GetSection("AllowedOrigins").Get<string[]>() ?? [];

//Logging configuration:
Log.Logger = new LoggerConfiguration()
             .WriteTo.Console()
             .WriteTo.File(
                 path: "logs/log-.txt", // Log file path with rolling logs
                 rollingInterval: RollingInterval.Day, // Roll log files daily
                 outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} [{Level:u3}] {Message:lj}{NewLine}{Exception}",
                 restrictedToMinimumLevel: Serilog.Events.LogEventLevel.Information // Minimum level to log
             )
             .CreateLogger();

// Entity Interceptor for auditing:
services.AddSingleton<AuditableEntityInterceptor>();
services.AddDbContext<AppDbContext>((serviceProvider, options) =>
{
    var auditInterceptor = serviceProvider.GetService<AuditableEntityInterceptor>()!;
    options.UseSqlServer(connectionString: config.GetConnectionString("SqlServer"))
           .AddInterceptors(auditInterceptor);
});

// Handle JSON cycles:
services.AddControllers()
        .AddJsonOptions(options =>
        {
            options.JsonSerializerOptions.ReferenceHandler =
                System.Text.Json.Serialization.ReferenceHandler.IgnoreCycles;
        });

services.Configure<FormOptions>(op =>
{
    op.MultipartBodyLengthLimit = 10 * 1024 * 1024; // 10 MB limit for multipart form data
    op.MultipartHeadersLengthLimit = 16 * 1024; // 16 KB limit for headers length
});

builder.WebHost.ConfigureKestrel(options =>
{
    options.Limits.MaxRequestBodySize = 10 * 1024 * 1024; // Maximum body size is 10MB
});

// Authentication:
services.AddAuthentication(options =>
        {
            options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
            options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
        })
        .AddJwtBearer(options =>
        {
            options.SaveToken = true;
            options.RequireHttpsMetadata = false;
            options.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidateLifetime = true,
                ValidateIssuerSigningKey = true,
                ValidIssuer = config["JwtSettings:Issuer"],
                ValidAudience = config["JwtSettings:Audience"],
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey!)),
                NameClaimType = "name"
            };
            // JWT token validation events
            options.Events = new JwtBearerEvents()
            {
                OnMessageReceived = context =>
                {
                    var accessToken = context.Request.Query["access_token"];
                    var path = context.HttpContext.Request.Path;
                    if (!accessToken.IsNullOrEmpty() && path.StartsWithSegments("/progressHub"))
                    {
                        context.Token = accessToken;
                    }

                    return Task.CompletedTask;
                }
            };
        });

// Custom authorization handlers:
services.AddAuthorization();
services.AddSingleton<IAuthorizationHandler, PermissionAuthorizationHandler>();
services.AddSingleton<IAuthorizationPolicyProvider, PermissionAuthorizationPolicyProvider>();

// SignalR configuration:
services.AddSignalR(op =>
{
    op.EnableDetailedErrors = true;
    op.HandshakeTimeout = TimeSpan.FromSeconds(10);
    op.ClientTimeoutInterval = TimeSpan.FromSeconds(60);
    op.KeepAliveInterval = TimeSpan.FromSeconds(15);
});

services.AddQuartzJobs();

services.AddEndpointsApiExplorer();

services.AddSwaggerGen(ops =>
{
    ops.SwaggerDoc("v1", new OpenApiInfo()
    {
        Title = "Sline-App",
        Version = "1.0",
        Description = "API documentation for SLine Service App"
    });
    ops.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "JWT bearer authentication",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.Http,
        Scheme = "Bearer",
        BearerFormat = "JWT"
    });
    ops.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                },
                /*Scheme = "Bearer",
                Name = "Bearer",
                In = ParameterLocation.Header,*/
            },
            []
        }
    });
    // Set the comments path for the Swagger JSON and UI.
    var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
    ops.IncludeXmlComments(xmlPath);
});

services.AddSingleton(restSettings);
services.AddSingleton<IRestClient>(new RestClient(new RestClientOptions(restSettings.BaseUrl)));

/* Add mapper services */
/*services.AddAutoMapper(typeof(UserMapper), typeof(RoleMapper),
                       typeof(OrgMapper), typeof(PagedMapper), typeof(RegionMapper));*/

services.AddSingleton<CustomMap>();

services.AddHttpContextAccessor();
services.AddScoped<JwtService>();
services.AddMemoryCache();

/* Add application services */
services.AddAppServices();
services.AddMongoServices(mongoSettings);
builder.Host.UseSerilog();
var app = builder.Build();

// Initialize database and seed default permissions
using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    var cacheService = scope.ServiceProvider.GetRequiredService<ICachingRoleService>();
    context.Database.EnsureCreated();
    await PreLoadCachingRoles(cacheService, context);
    await SeedPermissions(context);
}

// Configure the HTTP request pipeline.
app.UseCors(op =>
{
    op.WithOrigins(origins);
    op.AllowAnyMethod();
    op.AllowCredentials();
    op.WithExposedHeaders("X-Filename"); //custom header for client to access
    op.AllowAnyHeader();
    op.Build();
});
app.UseWebSockets();
app.UseMiddleware<ExceptionMiddleware>();
app.UseStaticFiles();
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "My API V1");
        c.InjectJavascript("/swagger-custom.js");
    });
}


app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

app.MapHub<AppHub>("/progressHub");

app.Run();
return;

//Seed default permissions
async Task SeedPermissions(AppDbContext context)
{
    var existingPermissions = context.Permissions.Select(p => p.PermissionName).ToHashSet();
    var defaultPermissions = PermissionSeeder.GetDefaultPermissions();
    var permissionsToAdd = defaultPermissions.Where(permission => !existingPermissions.Contains(permission))
                                             .Select(permission => new Permission { PermissionName = permission })
                                             .ToList();

    await context.AddRangeAsync(permissionsToAdd);
    context.SaveChanges();
}

//Preload all permission of each role into memory cache for fast authorization check
async Task PreLoadCachingRoles(ICachingRoleService caching, AppDbContext context)
{
    var roles = await context.Roles.Select(r => r.RoleName).ToListAsync();
    foreach (var role in roles)
    {
        await caching.GetPermissionsInRole(role);
    }
}