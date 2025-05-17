# Implementation Strategies

This document outlines the practical implementation strategies for caching in the Wendover HOA application, providing concrete patterns and examples that align with Clean Architecture principles, SOLID principles, and other best practices.

## Response Caching

Response caching stores complete HTTP responses to avoid regenerating identical responses for subsequent requests.

### Configuration

```csharp
// Startup.cs
public void ConfigureServices(IServiceCollection services)
{
    services.AddResponseCaching();
    
    services.AddControllers(options =>
    {
        options.CacheProfiles.Add("Default",
            new CacheProfile
            {
                Duration = 60, // 1 minute
                Location = ResponseCacheLocation.Any
            });
        
        options.CacheProfiles.Add("Static",
            new CacheProfile
            {
                Duration = 86400, // 24 hours
                Location = ResponseCacheLocation.Any
            });
    });
}

public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
{
    app.UseResponseCaching();
    
    // Add cache control middleware
    app.Use(async (context, next) =>
    {
        context.Response.GetTypedHeaders().CacheControl = 
            new Microsoft.Net.Http.Headers.CacheControlHeaderValue
            {
                Public = true,
                MaxAge = TimeSpan.FromSeconds(10)
            };
        
        await next();
    });
}
```

### Controller Implementation

```csharp
[ApiController]
[Route("api/v{version:apiVersion}/[controller]")]
public class AnnouncementsController : ControllerBase
{
    [HttpGet]
    [ResponseCache(CacheProfileName = "Default")]
    public async Task<ActionResult<ApiResponse<List<AnnouncementDto>>>> GetAnnouncements()
    {
        // Implementation...
    }
    
    [HttpGet("public")]
    [ResponseCache(Duration = 300)] // 5 minutes
    public async Task<ActionResult<ApiResponse<List<AnnouncementDto>>>> GetPublicAnnouncements()
    {
        // Implementation...
    }
}
```

## Output Caching

Output caching stores rendered fragments or complete pages to avoid re-rendering for subsequent requests.

### Configuration

```csharp
// Startup.cs
public void ConfigureServices(IServiceCollection services)
{
    services.AddOutputCache(options =>
    {
        // Default policy
        options.AddBasePolicy(builder => builder.Cache());
        
        // Custom policies
        options.AddPolicy("Announcements", builder => 
            builder.Cache()
                .Expire(TimeSpan.FromMinutes(15))
                .Tag("announcements"));
                
        options.AddPolicy("Documents", builder => 
            builder.Cache()
                .Expire(TimeSpan.FromHours(1))
                .Tag("documents")
                .SetVaryByQuery("category", "page"));
    });
}

public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
{
    app.UseOutputCache();
}
```

### Controller Implementation

```csharp
[ApiController]
[Route("api/v{version:apiVersion}/[controller]")]
public class DocumentsController : ControllerBase
{
    [HttpGet]
    [OutputCache(PolicyName = "Documents")]
    public async Task<ActionResult<ApiResponse<List<DocumentDto>>>> GetDocuments(
        [FromQuery] string category,
        [FromQuery] int page = 1)
    {
        // Implementation...
    }
}
```

## Data Caching with Repository Pattern

Implement caching at the repository layer to cache query results and domain objects.

### Decorator Pattern Implementation

```csharp
// Interface
public interface IPropertyRepository
{
    Task<Property> GetByIdAsync(int id, CancellationToken cancellationToken);
    Task<List<Property>> GetAllAsync(CancellationToken cancellationToken);
    // Other methods...
}

// Concrete implementation
public class PropertyRepository : IPropertyRepository
{
    private readonly ApplicationDbContext _dbContext;
    
    public PropertyRepository(ApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }
    
    public async Task<Property> GetByIdAsync(int id, CancellationToken cancellationToken)
    {
        return await _dbContext.Properties
            .Include(p => p.Resident)
            .FirstOrDefaultAsync(p => p.Id == id, cancellationToken);
    }
    
    // Other methods...
}

// Caching decorator
public class CachedPropertyRepository : IPropertyRepository
{
    private readonly IPropertyRepository _inner;
    private readonly ICacheService _cacheService;
    private readonly ILogger<CachedPropertyRepository> _logger;
    
    public CachedPropertyRepository(
        IPropertyRepository inner,
        ICacheService cacheService,
        ILogger<CachedPropertyRepository> logger)
    {
        _inner = inner;
        _cacheService = cacheService;
        _logger = logger;
    }
    
    public async Task<Property> GetByIdAsync(int id, CancellationToken cancellationToken)
    {
        string cacheKey = $"WendoverHOA:Property:{id}";
        
        return await _cacheService.GetOrCreateAsync(
            cacheKey,
            async () => 
            {
                _logger.LogDebug("Cache miss for property {PropertyId}", id);
                return await _inner.GetByIdAsync(id, cancellationToken);
            },
            TimeSpan.FromMinutes(30));
    }
    
    // Other methods with similar caching implementation...
}

// Register in DI container
services.AddScoped<IPropertyRepository, PropertyRepository>();
services.Decorate<IPropertyRepository, CachedPropertyRepository>();
```

## MediatR Caching Pipeline

Implement caching as a cross-cutting concern using MediatR pipeline behaviors.

```csharp
public class CachingBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>, ICacheableQuery
{
    private readonly ICacheService _cacheService;
    private readonly ILogger<CachingBehavior<TRequest, TResponse>> _logger;
    
    public CachingBehavior(
        ICacheService cacheService,
        ILogger<CachingBehavior<TRequest, TResponse>> logger)
    {
        _cacheService = cacheService;
        _logger = logger;
    }
    
    public async Task<TResponse> Handle(
        TRequest request, 
        RequestHandlerDelegate<TResponse> next, 
        CancellationToken cancellationToken)
    {
        if (!request.BypassCache)
        {
            var cacheKey = request.CacheKey;
            var cachedResponse = await _cacheService.GetAsync<TResponse>(cacheKey);
            
            if (cachedResponse != null)
            {
                _logger.LogDebug("Returning cached result for {CacheKey}", cacheKey);
                return cachedResponse;
            }
        }
        
        var response = await next();
        
        if (!request.BypassCache && response != null)
        {
            await _cacheService.SetAsync(
                request.CacheKey,
                response,
                TimeSpan.FromMinutes(request.CacheTime));
        }
        
        return response;
    }
}

// Interface for cacheable queries
public interface ICacheableQuery
{
    string CacheKey { get; }
    int CacheTime { get; }
    bool BypassCache { get; }
}

// Example query implementation
public class GetPropertyByIdQuery : IRequest<PropertyDto>, ICacheableQuery
{
    public int Id { get; set; }
    
    public string CacheKey => $"WendoverHOA:Property:{Id}";
    public int CacheTime => 30; // minutes
    public bool BypassCache { get; set; }
}

// Register in DI container
services.AddTransient(typeof(IPipelineBehavior<,>), typeof(CachingBehavior<,>));
```

## Lazy Loading Cache Pattern

Implement lazy loading for expensive operations with automatic cache refresh.

```csharp
public class LazyCache<T> where T : class
{
    private readonly SemaphoreSlim _lock = new SemaphoreSlim(1, 1);
    private readonly ICacheService _cacheService;
    private readonly Func<Task<T>> _valueFactory;
    private readonly string _cacheKey;
    private readonly TimeSpan _cacheTime;
    private readonly TimeSpan _refreshTime;
    private DateTime _lastRefreshTime;
    
    public LazyCache(
        ICacheService cacheService,
        Func<Task<T>> valueFactory,
        string cacheKey,
        TimeSpan cacheTime,
        TimeSpan refreshTime)
    {
        _cacheService = cacheService;
        _valueFactory = valueFactory;
        _cacheKey = cacheKey;
        _cacheTime = cacheTime;
        _refreshTime = refreshTime;
        _lastRefreshTime = DateTime.UtcNow;
    }
    
    public async Task<T> GetValueAsync()
    {
        var value = await _cacheService.GetAsync<T>(_cacheKey);
        
        if (value != null)
        {
            // Check if refresh is needed in background
            if (DateTime.UtcNow - _lastRefreshTime > _refreshTime)
            {
                // Refresh cache in background
                _ = Task.Run(async () =>
                {
                    await _lock.WaitAsync();
                    try
                    {
                        if (DateTime.UtcNow - _lastRefreshTime > _refreshTime)
                        {
                            var newValue = await _valueFactory();
                            await _cacheService.SetAsync(_cacheKey, newValue, _cacheTime);
                            _lastRefreshTime = DateTime.UtcNow;
                        }
                    }
                    finally
                    {
                        _lock.Release();
                    }
                });
            }
            
            return value;
        }
        
        // Cache miss, get value and cache it
        await _lock.WaitAsync();
        try
        {
            // Double-check
            value = await _cacheService.GetAsync<T>(_cacheKey);
            if (value != null)
            {
                return value;
            }
            
            value = await _valueFactory();
            await _cacheService.SetAsync(_cacheKey, value, _cacheTime);
            _lastRefreshTime = DateTime.UtcNow;
            return value;
        }
        finally
        {
            _lock.Release();
        }
    }
}

// Usage example
public class AnnouncementService
{
    private readonly LazyCache<List<AnnouncementDto>> _announcementsCache;
    
    public AnnouncementService(
        ICacheService cacheService,
        IAnnouncementRepository repository)
    {
        _announcementsCache = new LazyCache<List<AnnouncementDto>>(
            cacheService,
            async () => await repository.GetActiveAnnouncementsAsync(),
            "WendoverHOA:Announcements:Active",
            TimeSpan.FromMinutes(30),
            TimeSpan.FromMinutes(5));
    }
    
    public async Task<List<AnnouncementDto>> GetActiveAnnouncementsAsync()
    {
        return await _announcementsCache.GetValueAsync();
    }
}
```

## Entity Framework Second-Level Cache

Implement a second-level cache for Entity Framework to improve query performance.

```csharp
public class EfCoreSecondLevelCacheInterceptor : DbCommandInterceptor
{
    private readonly ICacheService _cacheService;
    private readonly ILogger<EfCoreSecondLevelCacheInterceptor> _logger;
    
    public EfCoreSecondLevelCacheInterceptor(
        ICacheService cacheService,
        ILogger<EfCoreSecondLevelCacheInterceptor> logger)
    {
        _cacheService = cacheService;
        _logger = logger;
    }
    
    public override async ValueTask<InterceptionResult<DbDataReader>> ReaderExecutingAsync(
        DbCommand command,
        CommandEventData eventData,
        InterceptionResult<DbDataReader> result,
        CancellationToken cancellationToken = default)
    {
        // Check if query is cacheable (read-only)
        if (!IsCacheable(command))
        {
            return result;
        }
        
        var cacheKey = GenerateCacheKey(command);
        var cachedResults = await _cacheService.GetAsync<List<Dictionary<string, object>>>(cacheKey);
        
        if (cachedResults != null)
        {
            _logger.LogDebug("Cache hit for query: {CommandText}", command.CommandText);
            return InterceptionResult<DbDataReader>.SuppressWithResult(
                new CachedDataReader(cachedResults));
        }
        
        return result;
    }
    
    public override async ValueTask<DbDataReader> ReaderExecutedAsync(
        DbCommand command,
        CommandExecutedEventData eventData,
        DbDataReader result,
        CancellationToken cancellationToken = default)
    {
        // Check if query is cacheable and was not cached before
        if (!IsCacheable(command))
        {
            return result;
        }
        
        var cacheKey = GenerateCacheKey(command);
        var cachedResults = await _cacheService.GetAsync<List<Dictionary<string, object>>>(cacheKey);
        
        if (cachedResults == null)
        {
            _logger.LogDebug("Caching results for query: {CommandText}", command.CommandText);
            
            // Cache the results
            var results = new List<Dictionary<string, object>>();
            while (await result.ReadAsync(cancellationToken))
            {
                var row = new Dictionary<string, object>();
                for (int i = 0; i < result.FieldCount; i++)
                {
                    row[result.GetName(i)] = result.GetValue(i);
                }
                results.Add(row);
            }
            
            // Reset the reader
            result.Close();
            
            // Cache the results
            await _cacheService.SetAsync(cacheKey, results, TimeSpan.FromMinutes(5));
            
            // Return a new reader with the cached results
            return new CachedDataReader(results);
        }
        
        return result;
    }
    
    private bool IsCacheable(DbCommand command)
    {
        // Only cache SELECT statements
        return command.CommandText.TrimStart().StartsWith("SELECT", StringComparison.OrdinalIgnoreCase);
    }
    
    private string GenerateCacheKey(DbCommand command)
    {
        var sb = new StringBuilder("EF:");
        sb.Append(command.CommandText);
        
        foreach (DbParameter parameter in command.Parameters)
        {
            sb.Append(parameter.ParameterName);
            sb.Append("=");
            sb.Append(parameter.Value);
            sb.Append(";");
        }
        
        return sb.ToString();
    }
}

// Register in DbContext
protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
{
    optionsBuilder.AddInterceptors(_secondLevelCacheInterceptor);
}
```

## Conclusion

These implementation strategies provide practical approaches to implementing caching across the Wendover HOA application. By following these patterns, the application will maintain a consistent, efficient, and scalable approach to caching that aligns with Clean Architecture principles and optimizes application performance.

Each strategy can be adapted and combined based on specific requirements, ensuring that the caching implementation is tailored to the needs of each feature while maintaining a consistent architectural approach.
