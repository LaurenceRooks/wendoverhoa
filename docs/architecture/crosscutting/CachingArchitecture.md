# Caching Architecture

This document outlines the caching architecture for the Wendover HOA application, detailing the technical design and implementation patterns in alignment with Clean Architecture principles.

## Multi-Level Caching Architecture

The Wendover HOA application implements a multi-level caching architecture to optimize performance across different layers:

```
┌─────────────────────────────────────────────────────────────┐
│                      Client Browser                         │
└───────────────────────────────┬─────────────────────────────┘
                                │
                                ▼
┌─────────────────────────────────────────────────────────────┐
│                   CDN / Edge Caching                        │
└───────────────────────────────┬─────────────────────────────┘
                                │
                                ▼
┌─────────────────────────────────────────────────────────────┐
│                   Response Caching                          │
│                                                             │
│  ┌─────────────────────┐    ┌─────────────────────────┐     │
│  │   Output Caching    │    │    API Response Cache   │     │
│  └─────────────────────┘    └─────────────────────────┘     │
└───────────────────────────────┬─────────────────────────────┘
                                │
                                ▼
┌─────────────────────────────────────────────────────────────┐
│                   Application Caching                       │
│                                                             │
│  ┌─────────────────────┐    ┌─────────────────────────┐     │
│  │   In-Memory Cache   │    │    Distributed Cache    │     │
│  └─────────────────────┘    └─────────────────────────┘     │
└───────────────────────────────┬─────────────────────────────┘
                                │
                                ▼
┌─────────────────────────────────────────────────────────────┐
│                   Data Access Caching                       │
│                                                             │
│  ┌─────────────────────┐    ┌─────────────────────────┐     │
│  │   Query Results     │    │    Second-Level Cache   │     │
│  └─────────────────────┘    └─────────────────────────┘     │
└───────────────────────────────┬─────────────────────────────┘
                                │
                                ▼
┌─────────────────────────────────────────────────────────────┐
│                      Database                               │
└─────────────────────────────────────────────────────────────┘
```

## Cache Providers

The application uses the following cache providers:

1. **In-Memory Cache**: Microsoft.Extensions.Caching.Memory
   - For single-instance, non-distributed scenarios
   - Suitable for development and small deployments

2. **Distributed Cache**: Microsoft.Extensions.Caching.StackExchangeRedis
   - Redis-based distributed cache for multi-instance deployments
   - Provides high availability and scalability

## Cache Registration

Cache services are registered in the dependency injection container:

```csharp
// Startup.cs
public void ConfigureServices(IServiceCollection services)
{
    // Register in-memory cache
    services.AddMemoryCache(options =>
    {
        options.SizeLimit = 1024; // 1GB limit
        options.CompactionPercentage = 0.2; // 20% compaction
        options.ExpirationScanFrequency = TimeSpan.FromMinutes(5);
    });
    
    // Register distributed cache (Redis)
    services.AddStackExchangeRedisCache(options =>
    {
        options.Configuration = Configuration.GetConnectionString("Redis");
        options.InstanceName = "WendoverHOA:";
    });
    
    // Register response caching middleware
    services.AddResponseCaching(options =>
    {
        options.MaximumBodySize = 64 * 1024 * 1024; // 64MB
        options.UseCaseSensitivePaths = false;
    });
    
    // Register output caching
    services.AddOutputCache(options =>
    {
        options.AddBasePolicy(builder => builder.Cache());
        options.AddPolicy("Announcements", builder => 
            builder.Cache()
                .Expire(TimeSpan.FromMinutes(15))
                .Tag("announcements"));
    });
    
    // Register custom cache service
    services.AddSingleton<ICacheService, CacheService>();
}
```

## Cache Abstraction

A cache abstraction layer provides a consistent interface across different cache providers:

```csharp
public interface ICacheService
{
    Task<T> GetOrCreateAsync<T>(string key, Func<Task<T>> factory, TimeSpan? expiration = null);
    Task<T> GetAsync<T>(string key);
    Task SetAsync<T>(string key, T value, TimeSpan? expiration = null);
    Task RemoveAsync(string key);
    Task RemoveByPrefixAsync(string prefix);
}

public class CacheService : ICacheService
{
    private readonly IMemoryCache _memoryCache;
    private readonly IDistributedCache _distributedCache;
    private readonly ICacheSerializer _serializer;
    private readonly IOptions<CacheSettings> _settings;
    private readonly bool _useDistributedCache;
    
    public CacheService(
        IMemoryCache memoryCache,
        IDistributedCache distributedCache,
        ICacheSerializer serializer,
        IOptions<CacheSettings> settings)
    {
        _memoryCache = memoryCache;
        _distributedCache = distributedCache;
        _serializer = serializer;
        _settings = settings;
        _useDistributedCache = settings.Value.UseDistributedCache;
    }
    
    public async Task<T> GetOrCreateAsync<T>(string key, Func<Task<T>> factory, TimeSpan? expiration = null)
    {
        var cacheValue = await GetAsync<T>(key);
        
        if (cacheValue != null)
        {
            return cacheValue;
        }
        
        var result = await factory();
        
        if (result != null)
        {
            await SetAsync(key, result, expiration);
        }
        
        return result;
    }
    
    // Implementation of other methods...
}
```

## Serialization Strategy

The application uses a configurable serialization strategy for distributed caching:

```csharp
public interface ICacheSerializer
{
    byte[] Serialize<T>(T value);
    T Deserialize<T>(byte[] bytes);
}

public class JsonCacheSerializer : ICacheSerializer
{
    private readonly JsonSerializerOptions _options;
    
    public JsonCacheSerializer()
    {
        _options = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            WriteIndented = false
        };
    }
    
    public byte[] Serialize<T>(T value)
    {
        if (value == null)
        {
            return null;
        }
        
        return JsonSerializer.SerializeToUtf8Bytes(value, _options);
    }
    
    public T Deserialize<T>(byte[] bytes)
    {
        if (bytes == null || bytes.Length == 0)
        {
            return default;
        }
        
        return JsonSerializer.Deserialize<T>(bytes, _options);
    }
}
```

## Cache Settings

Cache settings are configured in `appsettings.json`:

```json
{
  "CacheSettings": {
    "UseDistributedCache": true,
    "DefaultExpirationMinutes": 30,
    "CacheProfiles": {
      "Static": {
        "Duration": 86400,  // 24 hours
        "Location": "Any"
      },
      "Dynamic": {
        "Duration": 300,    // 5 minutes
        "Location": "Any"
      },
      "NoCache": {
        "Duration": 0,
        "Location": "None",
        "NoStore": true
      }
    }
  }
}
```

## Conclusion

This caching architecture provides a robust foundation for implementing caching across the Wendover HOA application. By following this architecture, the application will maintain a consistent, efficient, and scalable approach to caching that aligns with Clean Architecture principles and optimizes application performance.
