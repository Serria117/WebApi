using System.Text.Json;
using Microsoft.Extensions.Caching.Distributed;
using StackExchange.Redis;

namespace WebApp.Services.CachingServices;

public interface IRedisCacheService
{
    Task<T?> GetAsync<T>(string key);
    Task SetAsync<T>(string key, T value, TimeSpan? expiry = null);
    Task RemoveAsync(string key);

    Task<T> GetOrCreateAsync<T>(string key,
                                Func<Task<T>> factory,
                                TimeSpan? expiry = null,
                                bool useSlidingExpiration = false);

    Task<string> GetVersionedKeyAsync(string baseKey,
                                      string versionPrefix, // ví dụ: "org_version:u:123e4567-e89b-..."
                                      CancellationToken ct = default);

    Task<string?> GetStringAsync(string key, CancellationToken ct = default);
    Task SetStringAsync(string key, string value, DistributedCacheEntryOptions options, CancellationToken ct = default);
}

public class RedisCacheService(IDistributedCache cache,
                               IDatabase redisDb,
                               IConnectionMultiplexer connectionMultiplexer,
                               ILogger<RedisCacheService> logger) : IRedisCacheService
{
    private readonly JsonSerializerOptions _jsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        PropertyNameCaseInsensitive = true
    };

    public async Task<T?> GetAsync<T>(string key)
    {
        var json = await cache.GetStringAsync(key);
        if (json == null) return default;

        try
        {
            return JsonSerializer.Deserialize<T>(json, _jsonOptions);
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Deserialize cache failed for key {Key}", key);
            await cache.RemoveAsync(key);
            return default;
        }
    }

    public async Task SetAsync<T>(string key, T value, TimeSpan? expiry = null)
    {
        var json = JsonSerializer.Serialize(value, _jsonOptions);
        var options = new DistributedCacheEntryOptions
        {
            AbsoluteExpirationRelativeToNow = expiry ?? TimeSpan.FromMinutes(10)
        };

        await cache.SetStringAsync(key, json, options);
    }

    public async Task RemoveAsync(string key) => await cache.RemoveAsync(key);

    
    public async Task<T> GetOrCreateAsync<T>(string key,
                                             Func<Task<T>> factory,
                                             TimeSpan? expiry = null,
                                             bool useSlidingExpiration = false)
    {
        var cached = await GetAsync<T>(key);
        if (cached != null)
        {
            logger.LogInformation("Get from redis cache: {Key}", key);
            return cached;
        }

        logger.LogInformation("cache [{Key}] not found. Load data from db...", key);
        var result = await factory();

        var options = new DistributedCacheEntryOptions();
        if (useSlidingExpiration)
            options.SlidingExpiration = expiry ?? TimeSpan.FromMinutes(5);
        else
            options.AbsoluteExpirationRelativeToNow = expiry ?? TimeSpan.FromMinutes(10);

        await SetAsync(key, result, options.AbsoluteExpirationRelativeToNow);
        return result;
    }
    
    public async Task<string> GetVersionedKeyAsync(string baseKey,
                                                   string versionPrefix, // ví dụ: "org_version:u:123e4567-e89b-..."
                                                   CancellationToken ct = default)
    {
        var version = await cache.GetStringAsync(versionPrefix, ct) ?? "v1";
        return $"{baseKey}|{version}";
    }
    public async Task<string?> GetStringAsync(string key, CancellationToken ct = default)
        => await cache.GetStringAsync(key, ct);

    public async Task SetStringAsync(string key, string value, DistributedCacheEntryOptions options, CancellationToken ct = default)
        => await cache.SetStringAsync(key, value, options, ct);
}