# Cache Invalidation

This document outlines the cache invalidation strategies for the Wendover HOA application, ensuring data consistency across all layers of the application in alignment with Clean Architecture principles.

## Invalidation Strategies

The Wendover HOA application employs multiple cache invalidation strategies:

1. **Time-Based Expiration**: Cache entries automatically expire after a configured duration
2. **Event-Based Invalidation**: Cache entries are invalidated when related data changes
3. **Manual Invalidation**: Cache entries can be explicitly invalidated by application code
4. **Bulk Invalidation**: Multiple related cache entries can be invalidated together
5. **Tag-Based Invalidation**: Cache entries with specific tags can be invalidated as a group

## Time-Based Expiration

Time-based expiration is the simplest invalidation strategy, where cache entries automatically expire after a configured duration.

```csharp
// Set cache with expiration
await _cacheService.SetAsync(
    "WendoverHOA:Announcements:Active",
    announcements,
    TimeSpan.FromMinutes(15));

// Set cache with absolute and sliding expiration
var cacheEntryOptions = new MemoryCacheEntryOptions()
    .SetAbsoluteExpiration(TimeSpan.FromHours(1))
    .SetSlidingExpiration(TimeSpan.FromMinutes(10));

_memoryCache.Set("WendoverHOA:Documents:Recent", documents, cacheEntryOptions);
```

## Event-Based Invalidation

Event-based invalidation removes cache entries when related data changes, ensuring cache consistency.

### MediatR Notification Handlers

```csharp
// Domain event
public class AnnouncementCreatedEvent : INotification
{
    public int AnnouncementId { get; }
    
    public AnnouncementCreatedEvent(int announcementId)
    {
        AnnouncementId = announcementId;
    }
}

// Cache invalidation handler
public class AnnouncementCacheInvalidationHandler : 
    INotificationHandler<AnnouncementCreatedEvent>,
    INotificationHandler<AnnouncementUpdatedEvent>,
    INotificationHandler<AnnouncementDeletedEvent>
{
    private readonly ICacheService _cacheService;
    private readonly ILogger<AnnouncementCacheInvalidationHandler> _logger;
    
    public AnnouncementCacheInvalidationHandler(
        ICacheService cacheService,
        ILogger<AnnouncementCacheInvalidationHandler> logger)
    {
        _cacheService = cacheService;
        _logger = logger;
    }
    
    public async Task Handle(AnnouncementCreatedEvent notification, CancellationToken cancellationToken)
    {
        _logger.LogDebug("Invalidating announcement caches after creation of announcement {AnnouncementId}", 
            notification.AnnouncementId);
            
        await InvalidateAnnouncementCaches();
    }
    
    public async Task Handle(AnnouncementUpdatedEvent notification, CancellationToken cancellationToken)
    {
        _logger.LogDebug("Invalidating announcement caches after update of announcement {AnnouncementId}", 
            notification.AnnouncementId);
            
        await InvalidateAnnouncementCaches();
        
        // Also invalidate specific announcement cache
        await _cacheService.RemoveAsync($"WendoverHOA:Announcement:{notification.AnnouncementId}");
    }
    
    public async Task Handle(AnnouncementDeletedEvent notification, CancellationToken cancellationToken)
    {
        _logger.LogDebug("Invalidating announcement caches after deletion of announcement {AnnouncementId}", 
            notification.AnnouncementId);
            
        await InvalidateAnnouncementCaches();
    }
    
    private async Task InvalidateAnnouncementCaches()
    {
        // Invalidate all announcement-related caches
        await _cacheService.RemoveByPrefixAsync("WendoverHOA:Announcements:");
        
        // Invalidate tag-based caches
        await _cacheService.RemoveByTagAsync("announcements");
    }
}
```

### Command/Query Handlers with Cache Invalidation

```csharp
public class UpdateAnnouncementCommandHandler : IRequestHandler<UpdateAnnouncementCommand, Unit>
{
    private readonly IAnnouncementRepository _repository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICacheService _cacheService;
    private readonly ILogger<UpdateAnnouncementCommandHandler> _logger;
    
    public async Task<Unit> Handle(UpdateAnnouncementCommand request, CancellationToken cancellationToken)
    {
        var announcement = await _repository.GetByIdAsync(request.Id, cancellationToken);
        
        if (announcement == null)
        {
            throw new NotFoundException(nameof(Announcement), request.Id);
        }
        
        announcement.Update(request.Title, request.Content, request.IsPublished);
        
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        
        // Invalidate specific announcement cache
        await _cacheService.RemoveAsync($"WendoverHOA:Announcement:{request.Id}");
        
        // Invalidate list caches
        await _cacheService.RemoveByPrefixAsync("WendoverHOA:Announcements:");
        
        // Invalidate tag-based caches
        await _cacheService.RemoveByTagAsync("announcements");
        
        return Unit.Value;
    }
}
```

## Tag-Based Invalidation

Tag-based invalidation allows grouping related cache entries for efficient invalidation.

```csharp
// Output cache with tags
app.MapGet("/api/announcements", GetAnnouncements)
    .CacheOutput(policy => policy
        .Tag("announcements")
        .Expire(TimeSpan.FromMinutes(15)));

// Invalidate by tag
public class AnnouncementController : ControllerBase
{
    private readonly IOutputCacheStore _outputCacheStore;
    
    [HttpPost]
    public async Task<IActionResult> CreateAnnouncement(CreateAnnouncementCommand command)
    {
        // Process command...
        
        // Invalidate all caches with the "announcements" tag
        await _outputCacheStore.EvictByTagAsync("announcements", CancellationToken.None);
        
        return Ok();
    }
}
```

## Dependency-Based Invalidation

Dependency-based invalidation establishes relationships between cache entries to ensure consistency.

```csharp
public class CacheDependencyManager
{
    private readonly ICacheService _cacheService;
    private readonly ConcurrentDictionary<string, HashSet<string>> _dependencies;
    
    public CacheDependencyManager(ICacheService cacheService)
    {
        _cacheService = cacheService;
        _dependencies = new ConcurrentDictionary<string, HashSet<string>>();
    }
    
    public async Task AddDependencyAsync(string primaryKey, string dependentKey)
    {
        var dependencies = _dependencies.GetOrAdd(primaryKey, _ => new HashSet<string>());
        
        lock (dependencies)
        {
            dependencies.Add(dependentKey);
        }
        
        // Also store in distributed cache for multi-instance scenarios
        await _cacheService.SetAsync(
            $"WendoverHOA:CacheDependency:{primaryKey}",
            dependencies.ToArray(),
            TimeSpan.FromDays(1));
    }
    
    public async Task InvalidateDependenciesAsync(string primaryKey)
    {
        // Get dependencies from memory
        if (_dependencies.TryGetValue(primaryKey, out var dependencies))
        {
            foreach (var dependentKey in dependencies)
            {
                await _cacheService.RemoveAsync(dependentKey);
            }
        }
        
        // Get dependencies from distributed cache
        var distributedDependencies = await _cacheService.GetAsync<string[]>(
            $"WendoverHOA:CacheDependency:{primaryKey}");
            
        if (distributedDependencies != null)
        {
            foreach (var dependentKey in distributedDependencies)
            {
                await _cacheService.RemoveAsync(dependentKey);
            }
        }
        
        // Remove the dependency tracking
        _dependencies.TryRemove(primaryKey, out _);
        await _cacheService.RemoveAsync($"WendoverHOA:CacheDependency:{primaryKey}");
    }
}

// Usage example
public async Task<List<AnnouncementDto>> GetAnnouncementsByCategoryAsync(string category)
{
    string categoryKey = $"WendoverHOA:Category:{category}";
    string announcementsKey = $"WendoverHOA:Announcements:Category:{category}";
    
    var announcements = await _cacheService.GetOrCreateAsync(
        announcementsKey,
        async () => await _repository.GetByCategoryAsync(category),
        TimeSpan.FromMinutes(15));
        
    // Add dependency
    await _cacheDependencyManager.AddDependencyAsync(categoryKey, announcementsKey);
    
    return announcements;
}

// When a category is updated
public async Task UpdateCategoryAsync(string category, string newName)
{
    // Update category...
    
    // Invalidate category and all dependent caches
    string categoryKey = $"WendoverHOA:Category:{category}";
    await _cacheDependencyManager.InvalidateDependenciesAsync(categoryKey);
}
```

## Prefix-Based Invalidation

Prefix-based invalidation removes multiple cache entries with a common key prefix.

```csharp
public static class CacheServiceExtensions
{
    public static async Task RemoveByPrefixAsync(this ICacheService cacheService, string prefix)
    {
        if (cacheService is CacheService service)
        {
            await service.RemoveByPrefixInternalAsync(prefix);
        }
    }
}

public class CacheService : ICacheService
{
    // Other methods...
    
    public async Task RemoveByPrefixInternalAsync(string prefix)
    {
        if (_useDistributedCache && _distributedCache is RedisCache redisCache)
        {
            // Use Redis SCAN to find and remove keys with prefix
            var connection = await GetRedisConnectionAsync();
            var server = connection.GetServer(connection.GetEndPoints().First());
            
            var keys = server.Keys(pattern: $"{prefix}*");
            
            foreach (var key in keys)
            {
                await _distributedCache.RemoveAsync(key);
            }
        }
        else
        {
            // For memory cache, we need to track keys by prefix
            var keysToRemove = _memoryKeysTracker
                .Where(k => k.StartsWith(prefix))
                .ToList();
                
            foreach (var key in keysToRemove)
            {
                _memoryCache.Remove(key);
                _memoryKeysTracker.Remove(key);
            }
        }
    }
}
```

## Automatic Invalidation with EF Core

Automatically invalidate cache when entities are modified using EF Core change tracking.

```csharp
public class CacheInvalidationInterceptor : SaveChangesInterceptor
{
    private readonly ICacheService _cacheService;
    private readonly ILogger<CacheInvalidationInterceptor> _logger;
    
    public CacheInvalidationInterceptor(
        ICacheService cacheService,
        ILogger<CacheInvalidationInterceptor> logger)
    {
        _cacheService = cacheService;
        _logger = logger;
    }
    
    public override ValueTask<InterceptionResult<int>> SavingChangesAsync(
        DbContextEventData eventData,
        InterceptionResult<int> result,
        CancellationToken cancellationToken = default)
    {
        var changedEntities = eventData.Context.ChangeTracker
            .Entries()
            .Where(e => e.State is EntityState.Added or EntityState.Modified or EntityState.Deleted)
            .ToList();
            
        if (changedEntities.Any())
        {
            // Group by entity type
            var entityGroups = changedEntities
                .GroupBy(e => e.Metadata.Name)
                .ToList();
                
            foreach (var group in entityGroups)
            {
                var entityType = group.Key;
                var cachePrefix = $"WendoverHOA:{entityType}";
                
                _logger.LogDebug("Invalidating cache for entity type {EntityType}", entityType);
                
                // Invalidate type-based caches
                _ = _cacheService.RemoveByPrefixAsync(cachePrefix);
                
                // Invalidate specific entity caches
                foreach (var entry in group)
                {
                    if (entry.State is EntityState.Modified or EntityState.Deleted)
                    {
                        var keyValues = entry.Metadata.FindPrimaryKey()
                            .Properties
                            .Select(p => entry.Property(p.Name).CurrentValue)
                            .ToArray();
                            
                        var keyString = string.Join(":", keyValues);
                        var entityKey = $"{cachePrefix}:{keyString}";
                        
                        _logger.LogDebug("Invalidating cache for entity {EntityKey}", entityKey);
                        
                        _ = _cacheService.RemoveAsync(entityKey);
                    }
                }
                
                // Invalidate related entity caches based on navigation properties
                // This is a simplified example - a more comprehensive solution would
                // traverse the entire entity graph
                foreach (var entry in group)
                {
                    var navigations = entry.Metadata.GetNavigations();
                    
                    foreach (var navigation in navigations)
                    {
                        var relatedEntityType = navigation.TargetEntityType.Name;
                        var relatedCachePrefix = $"WendoverHOA:{relatedEntityType}";
                        
                        _logger.LogDebug("Invalidating cache for related entity type {RelatedEntityType}", 
                            relatedEntityType);
                            
                        _ = _cacheService.RemoveByPrefixAsync(relatedCachePrefix);
                    }
                }
            }
        }
        
        return base.SavingChangesAsync(eventData, result, cancellationToken);
    }
}

// Register in DbContext
protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
{
    optionsBuilder.AddInterceptors(_cacheInvalidationInterceptor);
}
```

## Best Practices

1. **Be Selective**: Only invalidate what's necessary to maintain consistency
2. **Be Proactive**: Invalidate cache entries as soon as the underlying data changes
3. **Be Comprehensive**: Ensure all related cache entries are invalidated together
4. **Be Efficient**: Use bulk invalidation techniques for related entries
5. **Be Resilient**: Handle invalidation failures gracefully
6. **Be Consistent**: Apply the same invalidation strategy across similar data types
7. **Be Mindful**: Consider the performance impact of frequent invalidation

## Conclusion

This cache invalidation strategy provides a comprehensive approach to maintaining cache consistency across the Wendover HOA application. By following these guidelines, the application will ensure that cached data remains consistent with the source of truth while optimizing performance and resource usage.

The strategy ensures:

1. **Data Consistency**: Cached data is invalidated when the source data changes
2. **Performance**: Invalidation is performed efficiently to minimize overhead
3. **Scalability**: Invalidation strategies work in distributed environments
4. **Maintainability**: Clear patterns for cache invalidation across the application

By implementing these invalidation strategies, the Wendover HOA application will maintain a reliable and efficient caching system that aligns with Clean Architecture principles.
