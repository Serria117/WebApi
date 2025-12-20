using System.Reflection;
using System.Text;
using System.Text.Json;
using System.Threading.RateLimiting;
using EFCoreSecondLevelCacheInterceptor;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi;
using Microsoft.OpenApi.Models;
using RestSharp;
using WebApp;
using WebApp.Authentication;
using WebApp.Core.Data;
using WebApp.Mongo;
using WebApp.Services.CommonService;
using WebApp.Services.Mappers;
using WebApp.Services.RestService;
using WebApp.SignalrConfig;
using Serilog;
using WebApp.Configuration;
using WebApp.GlobalExceptionHandler;
using WebApp.ScheduleTask;
using WebApp.Services;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Serializers;
using Scalar.AspNetCore;
using StackExchange.Redis;
using WebApp.Enums;
using ZiggyCreatures.Caching.Fusion;

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
             .MinimumLevel.Override("Microsoft.EntityFrameworkCore.Database.Command",
                                    Serilog.Events.LogEventLevel.Warning)
             .Enrich.FromLogContext()
             .WriteTo.File(
                 path: "logs/log-.txt", // Log file path with rolling logs
                 rollingInterval: RollingInterval.Day, // Roll log files daily
                 outputTemplate:
                 "{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} [{Level:u3}] {Message:lj}{NewLine}{Exception}",
                 restrictedToMinimumLevel: Serilog.Events.LogEventLevel.Debug // Minimum level to log
             )
             .CreateLogger();

// Add Redis Cache to DI container
services.AddStackExchangeRedisCache(op =>
{
    op.Configuration = config.GetConnectionString("Redis");
    op.InstanceName = config["RedisCache:InstanceName"];
});

// FusionCache configuration
services.AddFusionCache().WithDefaultEntryOptions(
    new FusionCacheEntryOptions()
    {
        Duration = TimeSpan.FromMinutes(1),
        FailSafeMaxDuration = TimeSpan.FromMinutes(60)
    });

// EF Second Level Cache configuration
services.AddEFSecondLevelCache(options =>
{
    /*var redisOptions = ConfigurationOptions.Parse(config.GetConnectionString("Redis")!);
    redisOptions.AllowAdmin = true;
    redisOptions.AbortOnConnectFail  = false;
    redisOptions.Ssl = true;
    options.UseStackExchangeRedisCacheProvider(redisOptions, TimeSpan.FromMinutes(5))
           .ConfigureLogging(true)
           .UseDbCallsIfCachingProviderIsDown(TimeSpan.FromSeconds(20));
    options.CacheAllQueries(CacheExpirationMode.Absolute, TimeSpan.FromMinutes(30));*/
    options.UseFusionCacheProvider()
           .ConfigureLogging(true)
           .UseDbCallsIfCachingProviderIsDown(TimeSpan.FromSeconds(30));
});

// Entity Interceptor for auditing and caching:
services.AddSingleton<AuditableEntityInterceptor>();
services.AddDbContext<AppDbContext>((serviceProvider, options) =>
{
    var auditInterceptor = serviceProvider.GetRequiredService<AuditableEntityInterceptor>();
    var cacheInterceptor = serviceProvider.GetRequiredService<SecondLevelCacheInterceptor>();
    options.UseSqlServer(connectionString: config.GetConnectionString("SqlServer"))
           .AddInterceptors(auditInterceptor)
           .AddInterceptors(cacheInterceptor);
});


// Đăng ký GuidSerializer
BsonSerializer.RegisterSerializer(new GuidSerializer(GuidRepresentation.CSharpLegacy));

// Nếu cần hỗ trợ mixed data (cũ + mới), thêm ObjectSerializer
var objectSerializer = new ObjectSerializer(BsonSerializer.LookupDiscriminatorConvention(typeof(object)),
                                            GuidRepresentation.Standard);
BsonSerializer.RegisterSerializer(objectSerializer);

//polling rate config:
services.AddRateLimiter(op =>
{
    op.AddFixedWindowLimiter("VerificationCodeLimit", opt =>
    {
        opt.PermitLimit = 10;
        opt.Window = TimeSpan.FromMinutes(1);
        opt.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
        opt.QueueLimit = 0;
    });
    op.OnRejected = async (context, token) =>
    {
        context.HttpContext.Response.StatusCode = 429;
        await context.HttpContext.Response.WriteAsync("Too many request. Try again later.",
                                                      cancellationToken: token);
    };
});

// Handle JSON cycles:
services.AddControllers()
        .AddJsonOptions(options =>
        {
            options.JsonSerializerOptions.ReferenceHandler =
                System.Text.Json.Serialization.ReferenceHandler.IgnoreCycles;
            //options.JsonSerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
        });

services.Configure<FormOptions>(op =>
{
    op.MultipartBodyLengthLimit = SystemBoundary.MaxFileSize; // 10 MB limit for multipart form data
    op.MultipartHeadersLengthLimit = 16 * 1024; // 16 KB limit for headers length
});

builder.WebHost.ConfigureKestrel(options =>
{
    options.Limits.MaxRequestBodySize = SystemBoundary.MaxFileSize; // Maximum body size is 10MB
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
                    if (!string.IsNullOrEmpty(accessToken) && path.StartsWithSegments("/progressHub"))
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



/*services.AddHttpContextAccessor();
services.AddScoped<JwtService>();
services.AddMemoryCache();*/

/* Add application services */
services.AddAppServices();
services.AddMongoServices(mongoSettings);

// Register DatabaseSeeder
services.AddTransient<DatabaseSeeder>();
builder.Host.UseSerilog();

var app = builder.Build();

// Initialize database and seed default values
using (var scope = app.Services.CreateScope())
{
    if (!app.Environment.IsDevelopment())
    {
        var seeder = scope.ServiceProvider.GetRequiredService<DatabaseSeeder>();
        await seeder.SeedAsync();
    }
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
app.UseMiddleware<ExceptionHandlerMiddleware>();
app.UseStaticFiles();
app.UseRouting();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "My API V1");
        c.InjectJavascript("/swagger-custom.js");
    });
    app.MapScalarApiReference(options =>
    {
        options.WithTitle("Sline-App API Documentation");
        
        // Point đến OpenAPI JSON của Swashbuckle (với documentName = v1)
        options.WithOpenApiRoutePattern("/swagger/{documentName}/swagger.json"); 
        
        // Các tùy chọn khác nếu cần:
        //options.ForceDarkMode(); // Bật dark mode
        options.Theme = ScalarTheme.BluePlanet;
        
        options.AddPreferredSecuritySchemes("Bearer"); 
    });
}

app.UseAuthentication();
app.UseAuthorization();
app.UseRateLimiter();

app.MapControllers();

app.MapHub<AppHub>("/progressHub");

app.Run();