# Caching Monitoring and Optimization

This document outlines the monitoring and optimization strategies for caching in the Wendover HOA application, ensuring optimal performance and resource usage in alignment with Clean Architecture principles.

## Monitoring Metrics

The Wendover HOA application monitors the following key caching metrics:

1. **Cache Hit Rate**: Percentage of successful cache retrievals
2. **Cache Miss Rate**: Percentage of failed cache retrievals
3. **Cache Size**: Current memory usage of the cache
4. **Cache Evictions**: Number of items removed from cache due to memory pressure
5. **Cache Latency**: Time taken to retrieve items from cache
6. **Cache Throughput**: Number of cache operations per second
7. **Cache Expiration Rate**: Number of items expired from cache

## Monitoring Implementation

### Cache Statistics Service

```csharp
public class CacheStatistics
{
    public long Hits { get; set; }
    public long Misses { get; set; }
    public long Size { get; set; }
    public long Evictions { get; set; }
    public double AverageGetTime { get; set; }
    public long TotalOperations { get; set; }
    public long Expirations { get; set; }
    
    public double HitRate => TotalOperations > 0 
        ? (double)Hits / TotalOperations * 100 
        : 0;
        
    public double MissRate => TotalOperations > 0 
        ? (double)Misses / TotalOperations * 100 
        : 0;
}

public interface ICacheMonitor
{
    CacheStatistics GetStatistics();
    void RecordHit();
    void RecordMiss();
    void RecordEviction();
    void RecordExpiration();
    void RecordGetTime(TimeSpan duration);
    void ResetStatistics();
}

public class CacheMonitor : ICacheMonitor
{
    private readonly CacheStatistics _statistics = new();
    private readonly object _lock = new();
    
    public CacheStatistics GetStatistics()
    {
        lock (_lock)
        {
            return new CacheStatistics
            {
                Hits = _statistics.Hits,
                Misses = _statistics.Misses,
                Size = _statistics.Size,
                Evictions = _statistics.Evictions,
                AverageGetTime = _statistics.AverageGetTime,
                TotalOperations = _statistics.Hits + _statistics.Misses,
                Expirations = _statistics.Expirations
            };
        }
    }
    
    public void RecordHit()
    {
        lock (_lock)
        {
            _statistics.Hits++;
        }
    }
    
    public void RecordMiss()
    {
        lock (_lock)
        {
            _statistics.Misses++;
        }
    }
    
    // Other methods implementation...
}
```

### Decorated Cache Service

```csharp
public class MonitoredCacheService : ICacheService
{
    private readonly ICacheService _inner;
    private readonly ICacheMonitor _monitor;
    private readonly ILogger<MonitoredCacheService> _logger;
    
    public MonitoredCacheService(
        ICacheService inner,
        ICacheMonitor monitor,
        ILogger<MonitoredCacheService> logger)
    {
        _inner = inner;
        _monitor = monitor;
        _logger = logger;
    }
    
    public async Task<T> GetAsync<T>(string key)
    {
        var stopwatch = Stopwatch.StartNew();
        
        try
        {
            var value = await _inner.GetAsync<T>(key);
            stopwatch.Stop();
            
            if (value != null)
            {
                _monitor.RecordHit();
                _monitor.RecordGetTime(stopwatch.Elapsed);
                _logger.LogTrace("Cache hit for key {Key} in {ElapsedMs}ms", key, stopwatch.ElapsedMilliseconds);
            }
            else
            {
                _monitor.RecordMiss();
                _logger.LogTrace("Cache miss for key {Key} in {ElapsedMs}ms", key, stopwatch.ElapsedMilliseconds);
            }
            
            return value;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving cache key {Key}", key);
            throw;
        }
    }
    
    // Other methods with similar monitoring...
}

// Register in DI container
services.AddSingleton<ICacheMonitor, CacheMonitor>();
services.AddScoped<ICacheService, CacheService>();
services.Decorate<ICacheService, MonitoredCacheService>();
```

## Telemetry Integration

### Application Insights Integration

```csharp
public class ApplicationInsightsCacheMonitor : ICacheMonitor
{
    private readonly ICacheMonitor _inner;
    private readonly TelemetryClient _telemetryClient;
    
    public ApplicationInsightsCacheMonitor(
        ICacheMonitor inner,
        TelemetryClient telemetryClient)
    {
        _inner = inner;
        _telemetryClient = telemetryClient;
    }
    
    public CacheStatistics GetStatistics()
    {
        var statistics = _inner.GetStatistics();
        
        // Track metrics
        _telemetryClient.TrackMetric("Cache.HitRate", statistics.HitRate);
        _telemetryClient.TrackMetric("Cache.MissRate", statistics.MissRate);
        _telemetryClient.TrackMetric("Cache.Size", statistics.Size);
        _telemetryClient.TrackMetric("Cache.Evictions", statistics.Evictions);
        _telemetryClient.TrackMetric("Cache.AverageGetTime", statistics.AverageGetTime);
        
        return statistics;
    }
    
    // Other methods that delegate to inner and track metrics...
}

// Register in DI container
services.Decorate<ICacheMonitor, ApplicationInsightsCacheMonitor>();
```

### Health Checks

```csharp
public class CacheHealthCheck : IHealthCheck
{
    private readonly ICacheMonitor _monitor;
    private readonly ICacheService _cacheService;
    
    public CacheHealthCheck(
        ICacheMonitor monitor,
        ICacheService cacheService)
    {
        _monitor = monitor;
        _cacheService = cacheService;
    }
    
    public async Task<HealthCheckResult> CheckHealthAsync(
        HealthCheckContext context,
        CancellationToken cancellationToken = default)
    {
        try
        {
            // Test cache operations
            await _cacheService.SetAsync("health-check", DateTime.UtcNow, TimeSpan.FromSeconds(5));
            var result = await _cacheService.GetAsync<DateTime>("health-check");
            
            if (result == default)
            {
                return HealthCheckResult.Degraded("Cache read/write test failed");
            }
            
            // Get statistics
            var statistics = _monitor.GetStatistics();
            
            // Check hit rate
            var hitRate = statistics.HitRate;
            var status = hitRate switch
            {
                >= 80 => HealthStatus.Healthy,
                >= 50 => HealthStatus.Degraded,
                _ => HealthStatus.Unhealthy
            };
            
            var data = new Dictionary<string, object>
            {
                { "HitRate", $"{hitRate:F2}%" },
                { "MissRate", $"{statistics.MissRate:F2}%" },
                { "Evictions", statistics.Evictions },
                { "AverageGetTime", $"{statistics.AverageGetTime:F2}ms" }
            };
            
            return new HealthCheckResult(
                status,
                $"Cache hit rate: {hitRate:F2}%",
                null,
                data);
        }
        catch (Exception ex)
        {
            return HealthCheckResult.Unhealthy("Cache health check failed", ex);
        }
    }
}

// Register health check
services.AddHealthChecks()
    .AddCheck<CacheHealthCheck>("cache", tags: new[] { "cache" });
```

## Performance Optimization

### Cache Profiling

```csharp
public class CacheProfiler
{
    private readonly ICacheMonitor _monitor;
    private readonly ILogger<CacheProfiler> _logger;
    private readonly Dictionary<string, CacheEntryProfile> _profiles = new();
    
    public CacheProfiler(
        ICacheMonitor monitor,
        ILogger<CacheProfiler> logger)
    {
        _monitor = monitor;
        _logger = logger;
    }
    
    public void RecordAccess(string key, bool hit, TimeSpan duration)
    {
        if (!_profiles.TryGetValue(key, out var profile))
        {
            profile = new CacheEntryProfile { Key = key };
            _profiles[key] = profile;
        }
        
        profile.AccessCount++;
        profile.TotalAccessTime += duration;
        
        if (hit)
        {
            profile.HitCount++;
        }
        else
        {
            profile.MissCount++;
        }
    }
    
    public IEnumerable<CacheEntryProfile> GetTopAccessedEntries(int count)
    {
        return _profiles
            .Values
            .OrderByDescending(p => p.AccessCount)
            .Take(count);
    }
    
    public IEnumerable<CacheEntryProfile> GetLeastEffectiveEntries(int count)
    {
        return _profiles
            .Values
            .Where(p => p.AccessCount > 10) // Only consider entries with sufficient data
            .OrderBy(p => p.HitRate)
            .Take(count);
    }
    
    public void GenerateReport()
    {
        var statistics = _monitor.GetStatistics();
        
        _logger.LogInformation("Cache Performance Report");
        _logger.LogInformation("----------------------");
        _logger.LogInformation("Overall Hit Rate: {HitRate:F2}%", statistics.HitRate);
        _logger.LogInformation("Overall Miss Rate: {MissRate:F2}%", statistics.MissRate);
        _logger.LogInformation("Average Get Time: {AverageGetTime:F2}ms", statistics.AverageGetTime);
        _logger.LogInformation("Total Operations: {TotalOperations}", statistics.TotalOperations);
        _logger.LogInformation("Cache Size: {Size} items", statistics.Size);
        _logger.LogInformation("Evictions: {Evictions}", statistics.Evictions);
        _logger.LogInformation("Expirations: {Expirations}", statistics.Expirations);
        _logger.LogInformation("");
        
        _logger.LogInformation("Top 10 Most Accessed Entries:");
        foreach (var profile in GetTopAccessedEntries(10))
        {
            _logger.LogInformation("Key: {Key}, Access Count: {AccessCount}, Hit Rate: {HitRate:F2}%",
                profile.Key, profile.AccessCount, profile.HitRate);
        }
        
        _logger.LogInformation("");
        
        _logger.LogInformation("Top 10 Least Effective Entries:");
        foreach (var profile in GetLeastEffectiveEntries(10))
        {
            _logger.LogInformation("Key: {Key}, Access Count: {AccessCount}, Hit Rate: {HitRate:F2}%",
                profile.Key, profile.AccessCount, profile.HitRate);
        }
    }
}

public class CacheEntryProfile
{
    public string Key { get; set; }
    public long AccessCount { get; set; }
    public long HitCount { get; set; }
    public long MissCount { get; set; }
    public TimeSpan TotalAccessTime { get; set; }
    
    public double HitRate => AccessCount > 0 
        ? (double)HitCount / AccessCount * 100 
        : 0;
        
    public double AverageAccessTime => AccessCount > 0 
        ? TotalAccessTime.TotalMilliseconds / AccessCount 
        : 0;
}
```

### Scheduled Cache Analysis

```csharp
public class CacheAnalysisService : BackgroundService
{
    private readonly CacheProfiler _profiler;
    private readonly ICacheService _cacheService;
    private readonly ILogger<CacheAnalysisService> _logger;
    
    public CacheAnalysisService(
        CacheProfiler profiler,
        ICacheService cacheService,
        ILogger<CacheAnalysisService> logger)
    {
        _profiler = profiler;
        _cacheService = cacheService;
        _logger = logger;
    }
    
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                // Generate report every hour
                _profiler.GenerateReport();
                
                // Analyze and optimize cache
                await OptimizeCacheAsync();
                
                await Task.Delay(TimeSpan.FromHours(1), stoppingToken);
            }
            catch (OperationCanceledException)
            {
                // Graceful shutdown
                break;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in cache analysis service");
                await Task.Delay(TimeSpan.FromMinutes(5), stoppingToken);
            }
        }
    }
    
    private async Task OptimizeCacheAsync()
    {
        // Get least effective entries
        var ineffectiveEntries = _profiler.GetLeastEffectiveEntries(20);
        
        foreach (var entry in ineffectiveEntries)
        {
            if (entry.HitRate < 10) // Less than 10% hit rate
            {
                _logger.LogInformation("Removing ineffective cache entry: {Key} with hit rate {HitRate:F2}%",
                    entry.Key, entry.HitRate);
                    
                await _cacheService.RemoveAsync(entry.Key);
            }
        }
    }
}

// Register background service
services.AddSingleton<CacheProfiler>();
services.AddHostedService<CacheAnalysisService>();
```

## Cache Visualization

### Dashboard Integration

```csharp
[ApiController]
[Route("api/v{version:apiVersion}/[controller]")]
[Authorize(Roles = "Administrator")]
public class CacheMonitoringController : ControllerBase
{
    private readonly ICacheMonitor _monitor;
    private readonly CacheProfiler _profiler;
    
    public CacheMonitoringController(
        ICacheMonitor monitor,
        CacheProfiler profiler)
    {
        _monitor = monitor;
        _profiler = profiler;
    }
    
    [HttpGet("statistics")]
    public ActionResult<CacheStatistics> GetStatistics()
    {
        return Ok(_monitor.GetStatistics());
    }
    
    [HttpGet("top-accessed")]
    public ActionResult<IEnumerable<CacheEntryProfile>> GetTopAccessedEntries(
        [FromQuery] int count = 10)
    {
        return Ok(_profiler.GetTopAccessedEntries(count));
    }
    
    [HttpGet("least-effective")]
    public ActionResult<IEnumerable<CacheEntryProfile>> GetLeastEffectiveEntries(
        [FromQuery] int count = 10)
    {
        return Ok(_profiler.GetLeastEffectiveEntries(count));
    }
    
    [HttpPost("reset")]
    public ActionResult ResetStatistics()
    {
        _monitor.ResetStatistics();
        return NoContent();
    }
}
```

## Performance Testing

### Cache Benchmarking

```csharp
public class CacheBenchmark
{
    private readonly ICacheService _cacheService;
    private readonly ILogger<CacheBenchmark> _logger;
    
    public CacheBenchmark(
        ICacheService cacheService,
        ILogger<CacheBenchmark> logger)
    {
        _cacheService = cacheService;
        _logger = logger;
    }
    
    public async Task RunBenchmarkAsync(int iterations = 10000)
    {
        _logger.LogInformation("Starting cache benchmark with {Iterations} iterations", iterations);
        
        // Generate test data
        var testData = Enumerable.Range(1, 100)
            .Select(i => new TestData
            {
                Id = i,
                Name = $"Item {i}",
                Value = Guid.NewGuid().ToString()
            })
            .ToList();
            
        // Benchmark write performance
        var writeStopwatch = Stopwatch.StartNew();
        
        for (int i = 0; i < iterations; i++)
        {
            var item = testData[i % testData.Count];
            await _cacheService.SetAsync($"benchmark:item:{item.Id}", item, TimeSpan.FromMinutes(5));
        }
        
        writeStopwatch.Stop();
        
        // Benchmark read performance (cache hit)
        var readHitStopwatch = Stopwatch.StartNew();
        
        for (int i = 0; i < iterations; i++)
        {
            var id = (i % testData.Count) + 1;
            await _cacheService.GetAsync<TestData>($"benchmark:item:{id}");
        }
        
        readHitStopwatch.Stop();
        
        // Benchmark read performance (cache miss)
        var readMissStopwatch = Stopwatch.StartNew();
        
        for (int i = 0; i < iterations; i++)
        {
            var id = 10000 + i; // Non-existent keys
            await _cacheService.GetAsync<TestData>($"benchmark:item:{id}");
        }
        
        readMissStopwatch.Stop();
        
        // Report results
        _logger.LogInformation("Cache Benchmark Results:");
        _logger.LogInformation("Write: {OperationsPerSecond:F2} ops/sec ({AverageMs:F3} ms/op)",
            iterations / writeStopwatch.Elapsed.TotalSeconds,
            writeStopwatch.Elapsed.TotalMilliseconds / iterations);
            
        _logger.LogInformation("Read (Hit): {OperationsPerSecond:F2} ops/sec ({AverageMs:F3} ms/op)",
            iterations / readHitStopwatch.Elapsed.TotalSeconds,
            readHitStopwatch.Elapsed.TotalMilliseconds / iterations);
            
        _logger.LogInformation("Read (Miss): {OperationsPerSecond:F2} ops/sec ({AverageMs:F3} ms/op)",
            iterations / readMissStopwatch.Elapsed.TotalSeconds,
            readMissStopwatch.Elapsed.TotalMilliseconds / iterations);
    }
    
    private class TestData
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Value { get; set; }
    }
}
```

## Best Practices

1. **Monitor Continuously**: Track cache performance metrics in real-time
2. **Analyze Patterns**: Identify access patterns and optimize cache accordingly
3. **Set Thresholds**: Establish performance thresholds and alerts
4. **Benchmark Regularly**: Conduct regular performance tests to detect regressions
5. **Optimize Selectively**: Focus optimization efforts on high-impact cache entries
6. **Balance Resources**: Find the right balance between memory usage and performance
7. **Document Findings**: Maintain documentation of cache performance characteristics

## Conclusion

This caching monitoring and optimization strategy provides a comprehensive approach to ensuring optimal cache performance across the Wendover HOA application. By following these guidelines, the application will maintain an efficient and effective caching system that maximizes performance while minimizing resource usage.

The strategy ensures:

1. **Visibility**: Comprehensive monitoring of cache performance metrics
2. **Optimization**: Data-driven approach to cache optimization
3. **Reliability**: Early detection of cache-related issues
4. **Efficiency**: Optimal use of memory and computing resources

By implementing these monitoring and optimization strategies, the Wendover HOA application will maintain a high-performance caching system that aligns with Clean Architecture principles and supports the application's performance goals.
