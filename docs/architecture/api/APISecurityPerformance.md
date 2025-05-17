# API Security and Performance

This document outlines the security and performance considerations for the Wendover HOA API, including rate limiting implementation, API versioning strategy, and error handling patterns.

## API Security

### Authentication and Authorization

The Wendover HOA API implements a comprehensive security model based on JWT (JSON Web Tokens) and role-based access control. For detailed information on authentication flows, refer to the [Authentication Flow Documentation](./AuthenticationFlowDocumentation.md).

### Key Security Measures

1. **HTTPS Only**: All API endpoints require HTTPS, with HTTP requests automatically redirected to HTTPS.
2. **JWT-Based Authentication**: All protected endpoints require a valid JWT token.
3. **Role-Based Access Control**: Access to endpoints is restricted based on user roles.
4. **Permission-Based Authorization**: Fine-grained permissions control access to specific resources and operations.
5. **Input Validation**: All input is validated to prevent injection attacks and other security vulnerabilities.
6. **Output Encoding**: All output is properly encoded to prevent XSS attacks.
7. **CORS Protection**: Cross-Origin Resource Sharing (CORS) is configured to allow only trusted origins.
8. **Anti-Forgery Protection**: Anti-forgery tokens are required for state-changing operations.
9. **Content Security Policy**: Strict CSP headers are implemented to prevent various attacks.
10. **Security Headers**: Various security headers are implemented to enhance security.

### Security Headers

The following security headers are implemented:

```csharp
app.Use(async (context, next) =>
{
    context.Response.Headers.Add("X-Content-Type-Options", "nosniff");
    context.Response.Headers.Add("X-Frame-Options", "DENY");
    context.Response.Headers.Add("X-XSS-Protection", "1; mode=block");
    context.Response.Headers.Add("Referrer-Policy", "strict-origin-when-cross-origin");
    context.Response.Headers.Add("Content-Security-Policy", "default-src 'self'; script-src 'self'; style-src 'self'; img-src 'self' data:; font-src 'self'; connect-src 'self'");
    context.Response.Headers.Add("Strict-Transport-Security", "max-age=31536000; includeSubDomains; preload");
    context.Response.Headers.Add("Permissions-Policy", "accelerometer=(), camera=(), geolocation=(), gyroscope=(), magnetometer=(), microphone=(), payment=(), usb=()");
    
    await next();
});
```

### CORS Configuration

CORS is configured to allow only trusted origins:

```csharp
services.AddCors(options =>
{
    options.AddPolicy("ApiCorsPolicy", builder =>
    {
        builder
            .WithOrigins(
                "https://wendoverhoa.org",
                "https://www.wendoverhoa.org",
                "https://staging.wendoverhoa.org",
                "http://localhost:5000",
                "https://localhost:5001")
            .AllowAnyMethod()
            .AllowAnyHeader()
            .AllowCredentials()
            .WithExposedHeaders("X-Pagination", "X-RateLimit-Limit", "X-RateLimit-Remaining", "X-RateLimit-Reset", "Token-Expired");
    });
});

// In Configure method
app.UseCors("ApiCorsPolicy");
```

### API Key Management

For service-to-service communication, API keys are used:

1. **API Key Generation**: Secure, random API keys are generated for each service.
2. **API Key Storage**: API keys are stored securely using a key vault.
3. **API Key Rotation**: API keys are rotated regularly.
4. **API Key Revocation**: API keys can be revoked immediately if compromised.

### Sensitive Data Handling

1. **Data Classification**: All data is classified based on sensitivity.
2. **Encryption at Rest**: Sensitive data is encrypted at rest.
3. **Encryption in Transit**: All data is encrypted in transit using TLS 1.3.
4. **Data Masking**: Sensitive data is masked in logs and error messages.
5. **PII Handling**: Personally Identifiable Information (PII) is handled according to GDPR and CCPA requirements.

### Security Monitoring and Incident Response

1. **Logging**: All security-related events are logged.
2. **Monitoring**: Security logs are monitored for suspicious activity.
3. **Alerting**: Alerts are generated for security incidents.
4. **Incident Response**: A formal incident response process is in place.
5. **Penetration Testing**: Regular penetration testing is conducted.
6. **Vulnerability Scanning**: Regular vulnerability scanning is conducted.

## Rate Limiting

Rate limiting is implemented to protect the API from abuse and to ensure fair usage.

### Rate Limiting Implementation

The Wendover HOA API uses ASP.NET Core's middleware for rate limiting:

```csharp
services.AddMemoryCache();

services.Configure<IpRateLimitOptions>(options =>
{
    options.EnableEndpointRateLimiting = true;
    options.StackBlockedRequests = false;
    options.HttpStatusCode = 429;
    options.RealIpHeader = "X-Real-IP";
    options.ClientIdHeader = "X-ClientId";
    options.GeneralRules = new List<RateLimitRule>
    {
        new RateLimitRule
        {
            Endpoint = "*",
            Period = "1m",
            Limit = 60
        },
        new RateLimitRule
        {
            Endpoint = "*",
            Period = "1h",
            Limit = 1000
        },
        new RateLimitRule
        {
            Endpoint = "POST:/api/v*/auth/login",
            Period = "5m",
            Limit = 5
        },
        new RateLimitRule
        {
            Endpoint = "POST:/api/v*/auth/register",
            Period = "1h",
            Limit = 3
        }
    };
});

services.AddSingleton<IIpPolicyStore, MemoryCacheIpPolicyStore>();
services.AddSingleton<IRateLimitCounterStore, MemoryCacheRateLimitCounterStore>();
services.AddSingleton<IRateLimitConfiguration, RateLimitConfiguration>();
services.AddSingleton<IProcessingStrategy, AsyncKeyLockProcessingStrategy>();
services.AddInMemoryRateLimiting();

// In Configure method
app.UseIpRateLimiting();
```

### Rate Limit Headers

Rate limit information is communicated via headers:

```
X-RateLimit-Limit: 60
X-RateLimit-Remaining: 59
X-RateLimit-Reset: 1589547426
```

### Rate Limit Response

When a rate limit is exceeded, a 429 (Too Many Requests) response is returned:

```json
{
  "success": false,
  "error": {
    "code": "RATE_LIMIT_EXCEEDED",
    "message": "Rate limit exceeded. Try again in 60 seconds.",
    "details": [
      "Limit: 60 requests per minute"
    ],
    "traceId": "00-1234567890abcdef1234567890abcdef-1234567890abcdef-00"
  }
}
```

### Rate Limit Tiers

Different rate limit tiers are applied based on user roles:

| Role | Requests per Minute | Requests per Hour |
|------|---------------------|-------------------|
| Anonymous | 30 | 500 |
| Resident | 60 | 1,000 |
| BoardMember | 120 | 2,000 |
| Administrator | 240 | 5,000 |

### Rate Limit Bypass

In emergency situations, rate limits can be bypassed using a special header:

```
X-RateLimit-Bypass: {secret-key}
```

This header should only be used by authorized internal services.

## API Versioning

API versioning is implemented to ensure backward compatibility and to allow for evolution of the API.

### Versioning Strategy

The Wendover HOA API uses URL-based versioning as the primary versioning strategy:

```
https://api.wendoverhoa.org/api/v1/properties
https://api.wendoverhoa.org/api/v2/properties
```

Header-based versioning is also supported as an alternative:

```
X-Api-Version: 1
```

### Versioning Implementation

API versioning is implemented using Microsoft.AspNetCore.Mvc.Versioning:

```csharp
services.AddApiVersioning(options =>
{
    options.ReportApiVersions = true;
    options.AssumeDefaultVersionWhenUnspecified = true;
    options.DefaultApiVersion = new ApiVersion(1, 0);
    options.ApiVersionReader = ApiVersionReader.Combine(
        new UrlSegmentApiVersionReader(),
        new HeaderApiVersionReader("X-Api-Version"));
});

services.AddVersionedApiExplorer(options =>
{
    options.GroupNameFormat = "'v'VVV";
    options.SubstituteApiVersionInUrl = true;
});
```

### Controller Versioning

Controllers are versioned using attributes:

```csharp
[ApiController]
[Route("api/v{version:apiVersion}/[controller]")]
[ApiVersion("1.0")]
public class PropertiesController : ControllerBase
{
    // Controller actions
}

[ApiController]
[Route("api/v{version:apiVersion}/[controller]")]
[ApiVersion("2.0")]
public class PropertiesV2Controller : ControllerBase
{
    // Controller actions
}
```

### Version Lifecycle

1. **Current Version**: The latest stable version (e.g., v1)
2. **Supported Versions**: Versions still supported but not latest (e.g., none yet)
3. **Deprecated Versions**: Versions scheduled for removal (e.g., none yet)
4. **Sunset Versions**: Versions no longer supported (e.g., none yet)

Versions are supported for at least 12 months after a new version is released.

### Version Deprecation

When a version is deprecated, the following headers are included in responses:

```
API-Deprecated: true
API-Deprecation-Date: 2026-05-15
API-Sunset-Date: 2026-11-15
API-Suggested-Version: 2
```

### Breaking vs. Non-Breaking Changes

1. **Breaking Changes**: Require a new API version
   - Removing or renaming fields
   - Changing field types
   - Changing response structure
   - Removing endpoints
   - Changing authentication requirements

2. **Non-Breaking Changes**: Do not require a new API version
   - Adding new fields (with default values)
   - Adding new endpoints
   - Adding new optional parameters
   - Extending enumerations
   - Bug fixes that don't change behavior

## Error Handling

The Wendover HOA API implements a consistent error handling pattern to provide clear and actionable error messages.

### Global Exception Handling

Global exception handling is implemented using middleware:

```csharp
public class GlobalExceptionHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<GlobalExceptionHandlingMiddleware> _logger;
    private readonly IWebHostEnvironment _env;

    public GlobalExceptionHandlingMiddleware(
        RequestDelegate next,
        ILogger<GlobalExceptionHandlingMiddleware> logger,
        IWebHostEnvironment env)
    {
        _next = next;
        _logger = logger;
        _env = env;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            await HandleExceptionAsync(context, ex);
        }
    }

    private async Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        _logger.LogError(exception, "An unhandled exception occurred");

        var errorDetails = new ErrorDetails
        {
            TraceId = Activity.Current?.Id ?? context.TraceIdentifier
        };

        int statusCode;
        
        switch (exception)
        {
            case ValidationException validationException:
                statusCode = StatusCodes.Status422UnprocessableEntity;
                errorDetails.Code = "VALIDATION_ERROR";
                errorDetails.Message = "Validation failed";
                errorDetails.Details = validationException.Errors
                    .Select(e => $"{e.PropertyName}: {e.ErrorMessage}")
                    .ToList();
                break;
                
            case NotFoundException notFoundException:
                statusCode = StatusCodes.Status404NotFound;
                errorDetails.Code = "RESOURCE_NOT_FOUND";
                errorDetails.Message = notFoundException.Message;
                break;
                
            case UnauthorizedAccessException:
                statusCode = StatusCodes.Status403Forbidden;
                errorDetails.Code = "FORBIDDEN";
                errorDetails.Message = "Access denied";
                break;
                
            case BadRequestException badRequestException:
                statusCode = StatusCodes.Status400BadRequest;
                errorDetails.Code = "BAD_REQUEST";
                errorDetails.Message = badRequestException.Message;
                break;
                
            case ConflictException conflictException:
                statusCode = StatusCodes.Status409Conflict;
                errorDetails.Code = "CONFLICT";
                errorDetails.Message = conflictException.Message;
                break;
                
            default:
                statusCode = StatusCodes.Status500InternalServerError;
                errorDetails.Code = "INTERNAL_SERVER_ERROR";
                errorDetails.Message = "An error occurred while processing your request";
                
                // Only include exception details in development
                if (_env.IsDevelopment())
                {
                    errorDetails.Details = new List<string>
                    {
                        exception.Message,
                        exception.StackTrace
                    };
                }
                break;
        }

        context.Response.StatusCode = statusCode;
        context.Response.ContentType = "application/json";
        
        var response = new ApiResponse<object>
        {
            Success = false,
            Error = errorDetails
        };
        
        await context.Response.WriteAsync(JsonSerializer.Serialize(response, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            WriteIndented = true
        }));
    }
}

// In Configure method
app.UseMiddleware<GlobalExceptionHandlingMiddleware>();
```

### Error Response Format

All error responses follow a consistent format:

```json
{
  "success": false,
  "error": {
    "code": "ERROR_CODE",
    "message": "Human-readable error message",
    "details": [
      "Additional error detail 1",
      "Additional error detail 2"
    ],
    "traceId": "00-1234567890abcdef1234567890abcdef-1234567890abcdef-00"
  }
}
```

### Common Error Codes

| Code | Description | HTTP Status |
|------|-------------|-------------|
| VALIDATION_ERROR | Validation failed | 422 |
| RESOURCE_NOT_FOUND | Resource not found | 404 |
| BAD_REQUEST | Bad request | 400 |
| UNAUTHORIZED | Authentication required | 401 |
| FORBIDDEN | Access denied | 403 |
| CONFLICT | Resource conflict | 409 |
| RATE_LIMIT_EXCEEDED | Rate limit exceeded | 429 |
| INTERNAL_SERVER_ERROR | Internal server error | 500 |

### Exception Types

Custom exception types are used to represent different error conditions:

```csharp
public class NotFoundException : Exception
{
    public NotFoundException(string message) : base(message) { }
}

public class BadRequestException : Exception
{
    public BadRequestException(string message) : base(message) { }
}

public class ConflictException : Exception
{
    public ConflictException(string message) : base(message) { }
}

public class ValidationException : Exception
{
    public List<ValidationFailure> Errors { get; }
    
    public ValidationException(List<ValidationFailure> errors)
        : base("Validation failed")
    {
        Errors = errors;
    }
}
```

### Validation Errors

Validation errors return a 422 status code with details:

```json
{
  "success": false,
  "error": {
    "code": "VALIDATION_ERROR",
    "message": "Validation failed",
    "details": [
      "address: Address is required",
      "zipCode: Zip code must be in format XXXXX or XXXXX-XXXX"
    ],
    "traceId": "00-1234567890abcdef1234567890abcdef-1234567890abcdef-00"
  }
}
```

### Error Logging

All errors are logged with contextual information:

```csharp
_logger.LogError(
    exception,
    "Error processing request {Method} {Path} from {IP}. TraceId: {TraceId}",
    context.Request.Method,
    context.Request.Path,
    context.Connection.RemoteIpAddress,
    Activity.Current?.Id ?? context.TraceIdentifier);
```

## API Performance

### Performance Optimization Techniques

1. **Caching**: Implement caching to reduce database load and improve response times.
2. **Compression**: Enable response compression to reduce bandwidth usage.
3. **Pagination**: Implement pagination for all collection endpoints.
4. **Projection**: Use projection queries to return only the required fields.
5. **Asynchronous Processing**: Use asynchronous processing for long-running operations.
6. **Database Optimization**: Optimize database queries and indexes.
7. **Eager Loading**: Use eager loading to reduce the number of database queries.
8. **Connection Pooling**: Use connection pooling to reduce database connection overhead.
9. **Minimizing Payload Size**: Use field selection to reduce payload size.
10. **Content Delivery Network (CDN)**: Use a CDN for static content.

### Response Compression

Response compression is enabled to reduce bandwidth usage:

```csharp
services.AddResponseCompression(options =>
{
    options.Providers.Add<BrotliCompressionProvider>();
    options.Providers.Add<GzipCompressionProvider>();
    options.MimeTypes = ResponseCompressionDefaults.MimeTypes.Concat(
        new[] { "application/json", "application/xml", "text/plain" });
    options.EnableForHttps = true;
});

services.Configure<BrotliCompressionProviderOptions>(options =>
{
    options.Level = CompressionLevel.Fastest;
});

services.Configure<GzipCompressionProviderOptions>(options =>
{
    options.Level = CompressionLevel.Fastest;
});

// In Configure method
app.UseResponseCompression();
```

### Caching Implementation

Caching is implemented using response caching middleware and cache headers:

```csharp
services.AddResponseCaching();

// In Configure method
app.UseResponseCaching();

// In controller action
[HttpGet]
[ResponseCache(Duration = 60, Location = ResponseCacheLocation.Any, VaryByQueryKeys = new[] { "pageNumber", "pageSize" })]
public async Task<ActionResult<ApiResponse<IEnumerable<PropertyDto>>>> GetProperties([FromQuery] PropertyQueryParameters parameters)
{
    // Implementation
}
```

### Cache Headers

Cache headers are used to control caching behavior:

```
Cache-Control: max-age=3600, public
ETag: "33a64df551425fcc55e4d42a148795d9f25f89d4"
Last-Modified: Wed, 15 May 2025 10:30:00 GMT
```

### Distributed Caching

For scalability, distributed caching is implemented using Redis:

```csharp
services.AddStackExchangeRedisCache(options =>
{
    options.Configuration = Configuration.GetConnectionString("Redis");
    options.InstanceName = "WendoverHOA:";
});
```

### Database Performance

1. **Query Optimization**: Use optimized queries and indexes.
2. **Eager Loading**: Use eager loading to reduce the number of database queries.
3. **Projection**: Use projection queries to return only the required fields.
4. **Paging**: Implement paging for all collection queries.
5. **Asynchronous Queries**: Use asynchronous queries for better scalability.

Example of optimized query:

```csharp
public async Task<IEnumerable<PropertyDto>> GetPropertiesAsync(PropertyQueryParameters parameters)
{
    var query = _context.Properties
        .AsNoTracking()  // No tracking for read-only queries
        .Include(p => p.Resident)  // Eager loading
        .Where(p => parameters.Status == null || p.Status == parameters.Status);
        
    // Apply filtering
    if (!string.IsNullOrEmpty(parameters.City))
    {
        query = query.Where(p => p.City == parameters.City);
    }
    
    // Apply sorting
    query = parameters.SortBy switch
    {
        "address" => parameters.SortDirection == "desc" 
            ? query.OrderByDescending(p => p.Address) 
            : query.OrderBy(p => p.Address),
        "city" => parameters.SortDirection == "desc" 
            ? query.OrderByDescending(p => p.City) 
            : query.OrderBy(p => p.City),
        _ => parameters.SortDirection == "desc" 
            ? query.OrderByDescending(p => p.Id) 
            : query.OrderBy(p => p.Id)
    };
    
    // Apply paging
    var totalCount = await query.CountAsync();
    var totalPages = (int)Math.Ceiling(totalCount / (double)parameters.PageSize);
    
    var properties = await query
        .Skip((parameters.PageNumber - 1) * parameters.PageSize)
        .Take(parameters.PageSize)
        .Select(p => new PropertyDto  // Projection
        {
            Id = p.Id,
            Address = p.Address,
            City = p.City,
            State = p.State,
            ZipCode = p.ZipCode,
            LotNumber = p.LotNumber,
            Status = p.Status.ToString(),
            Resident = p.Resident == null ? null : new ResidentDto
            {
                Id = p.Resident.Id,
                FirstName = p.Resident.FirstName,
                LastName = p.Resident.LastName,
                Email = p.Resident.Email,
                PhoneNumber = p.Resident.PhoneNumber
            }
        })
        .ToListAsync();
    
    // Set pagination headers
    _httpContextAccessor.HttpContext.Response.Headers.Add(
        "X-Pagination",
        JsonSerializer.Serialize(new
        {
            totalCount,
            totalPages,
            currentPage = parameters.PageNumber,
            pageSize = parameters.PageSize,
            hasNext = parameters.PageNumber < totalPages,
            hasPrevious = parameters.PageNumber > 1
        }));
    
    return properties;
}
```

### Asynchronous Processing

Long-running operations are processed asynchronously using background tasks:

```csharp
[HttpPost("reports/generate")]
public async Task<ActionResult<ApiResponse<ReportGenerationResponse>>> GenerateReport([FromBody] GenerateReportCommand command)
{
    // Validate command
    
    // Create a background job
    var jobId = BackgroundJob.Enqueue<IReportGenerationService>(
        service => service.GenerateReportAsync(command, CancellationToken.None));
    
    // Return job ID
    return Ok(new ApiResponse<ReportGenerationResponse>
    {
        Success = true,
        Data = new ReportGenerationResponse
        {
            JobId = jobId,
            Status = "Processing",
            EstimatedCompletionTime = DateTime.UtcNow.AddMinutes(5)
        }
    });
}

[HttpGet("reports/jobs/{jobId}")]
public async Task<ActionResult<ApiResponse<ReportGenerationResponse>>> GetReportGenerationStatus(string jobId)
{
    var job = await _reportJobRepository.GetJobAsync(jobId);
    
    if (job == null)
    {
        return NotFound(new ApiResponse<ReportGenerationResponse>
        {
            Success = false,
            Error = new ErrorDetails
            {
                Code = "RESOURCE_NOT_FOUND",
                Message = "Report generation job not found",
                TraceId = Activity.Current?.Id ?? HttpContext.TraceIdentifier
            }
        });
    }
    
    return Ok(new ApiResponse<ReportGenerationResponse>
    {
        Success = true,
        Data = new ReportGenerationResponse
        {
            JobId = jobId,
            Status = job.Status,
            EstimatedCompletionTime = job.EstimatedCompletionTime,
            ReportId = job.ReportId,
            ErrorMessage = job.ErrorMessage
        }
    });
}
```

### Performance Monitoring

1. **Application Insights**: Monitor API performance using Application Insights.
2. **Custom Metrics**: Track custom metrics for key operations.
3. **Health Checks**: Implement health checks for all dependencies.
4. **Performance Logging**: Log performance metrics for key operations.
5. **Alerting**: Set up alerts for performance degradation.

Example of health checks:

```csharp
services.AddHealthChecks()
    .AddDbContextCheck<ApplicationDbContext>("Database")
    .AddRedis(Configuration.GetConnectionString("Redis"), "Redis")
    .AddUrlGroup(new Uri("https://api.wendoverhoa.org/api/health"), "API")
    .AddCheck<StorageHealthCheck>("Storage");

app.UseHealthChecks("/health", new HealthCheckOptions
{
    ResponseWriter = async (context, report) =>
    {
        context.Response.ContentType = "application/json";
        
        var response = new
        {
            status = report.Status.ToString(),
            checks = report.Entries.Select(e => new
            {
                name = e.Key,
                status = e.Value.Status.ToString(),
                description = e.Value.Description,
                duration = e.Value.Duration
            }),
            totalDuration = report.TotalDuration
        };
        
        await context.Response.WriteAsync(JsonSerializer.Serialize(response));
    }
});
```

## Conclusion

The Wendover HOA API is designed with security and performance as top priorities. By implementing industry best practices for rate limiting, versioning, error handling, and performance optimization, the API provides a secure, reliable, and high-performance experience for all users.

Regular security audits, performance testing, and monitoring ensure that the API continues to meet the highest standards for security and performance.
