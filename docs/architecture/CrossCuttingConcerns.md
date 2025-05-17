# Cross-Cutting Concerns

## Introduction

This document details the implementation of cross-cutting concerns in the Wendover HOA application. Cross-cutting concerns are aspects of a system that affect multiple components and cannot be cleanly decomposed from the rest of the system. In the Wendover HOA application, these concerns are implemented using a combination of middleware, filters, behaviors, and services.

## Logging Implementation

The Wendover HOA application uses a comprehensive logging strategy to track application behavior, errors, and performance metrics.

### Logging Framework

The application uses Serilog as its logging framework, which provides:

- Structured logging with support for complex data types
- Multiple sinks (console, file, database, cloud services)
- Log enrichment with contextual information
- Log filtering and level-based logging

### Logging Configuration

Logging is configured during application startup:

```csharp
public static IHostBuilder CreateHostBuilder(string[] args) =>
    Host.CreateDefaultBuilder(args)
        .UseSerilog((context, services, configuration) => configuration
            .ReadFrom.Configuration(context.Configuration)
            .ReadFrom.Services(services)
            .Enrich.FromLogContext()
            .Enrich.WithMachineName()
            .Enrich.WithEnvironmentName()
            .Enrich.WithProperty("Application", "WendoverHOA")
            .WriteTo.Console()
            .WriteTo.File(
                path: "logs/wendoverhoa-.log",
                rollingInterval: RollingInterval.Day,
                retainedFileCountLimit: 31)
            .WriteTo.MSSqlServer(
                connectionString: context.Configuration.GetConnectionString("DefaultConnection"),
                sinkOptions: new MSSqlServerSinkOptions { TableName = "Logs", AutoCreateSqlTable = true })
            .WriteTo.ApplicationInsights(
                services.GetRequiredService<TelemetryConfiguration>(),
                TelemetryConverter.Traces))
        .ConfigureWebHostDefaults(webBuilder =>
        {
            webBuilder.UseStartup<Startup>();
        });
```

### Logging Middleware

The application uses middleware to log HTTP requests and responses:

```csharp
public class RequestLoggingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<RequestLoggingMiddleware> _logger;

    public RequestLoggingMiddleware(
        RequestDelegate next,
        ILogger<RequestLoggingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var originalBodyStream = context.Response.Body;

        try
        {
            // Log request
            _logger.LogInformation(
                "HTTP {RequestMethod} {RequestPath} started",
                context.Request.Method,
                context.Request.Path);

            // Capture response
            using var responseBody = new MemoryStream();
            context.Response.Body = responseBody;

            var sw = Stopwatch.StartNew();
            await _next(context);
            sw.Stop();

            // Log response
            context.Response.Body.Seek(0, SeekOrigin.Begin);
            var responseBodyText = await new StreamReader(context.Response.Body).ReadToEndAsync();
            context.Response.Body.Seek(0, SeekOrigin.Begin);

            _logger.LogInformation(
                "HTTP {RequestMethod} {RequestPath} completed with {StatusCode} in {ElapsedMilliseconds}ms",
                context.Request.Method,
                context.Request.Path,
                context.Response.StatusCode,
                sw.ElapsedMilliseconds);

            await responseBody.CopyToAsync(originalBodyStream);
        }
        finally
        {
            context.Response.Body = originalBodyStream;
        }
    }
}

// Extension method to register the middleware
public static class RequestLoggingMiddlewareExtensions
{
    public static IApplicationBuilder UseRequestLogging(this IApplicationBuilder builder)
    {
        return builder.UseMiddleware<RequestLoggingMiddleware>();
    }
}
```

### MediatR Logging Behavior

As detailed in the CQRS and MediatR Implementation document, the application uses a MediatR behavior to log all commands and queries:

```csharp
public class LoggingBehavior<TRequest, TResponse> 
    : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
{
    private readonly ILogger<LoggingBehavior<TRequest, TResponse>> _logger;
    private readonly ICurrentUserService _currentUserService;

    public LoggingBehavior(
        ILogger<LoggingBehavior<TRequest, TResponse>> logger,
        ICurrentUserService currentUserService)
    {
        _logger = logger;
        _currentUserService = currentUserService;
    }

    public async Task<TResponse> Handle(
        TRequest request, 
        RequestHandlerDelegate<TResponse> next, 
        CancellationToken cancellationToken)
    {
        var requestName = typeof(TRequest).Name;
        var userId = _currentUserService.UserId;

        _logger.LogInformation(
            "Handling {RequestName} for user {UserId}", 
            requestName, userId);

        var response = await next();

        _logger.LogInformation(
            "Handled {RequestName} for user {UserId}", 
            requestName, userId);

        return response;
    }
}
```

### Application Insights Integration

For production environments, the application integrates with Azure Application Insights for comprehensive monitoring and logging:

```csharp
public void ConfigureServices(IServiceCollection services)
{
    // Add Application Insights
    services.AddApplicationInsightsTelemetry(Configuration["ApplicationInsights:InstrumentationKey"]);
    
    // Configure Application Insights
    services.ConfigureTelemetryModule<DependencyTrackingTelemetryModule>((module, o) =>
    {
        module.EnableSqlCommandTextInstrumentation = true;
    });
}
```

## Exception Handling

The Wendover HOA application implements a comprehensive exception handling strategy to ensure that errors are handled consistently and appropriately.

### Global Exception Handling Middleware

The application uses middleware to catch and handle unhandled exceptions:

```csharp
public class ErrorHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ErrorHandlingMiddleware> _logger;
    private readonly IWebHostEnvironment _environment;

    public ErrorHandlingMiddleware(
        RequestDelegate next,
        ILogger<ErrorHandlingMiddleware> logger,
        IWebHostEnvironment environment)
    {
        _next = next;
        _logger = logger;
        _environment = environment;
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

        var response = context.Response;
        response.ContentType = "application/json";

        var (statusCode, message) = GetResponseDetails(exception);
        response.StatusCode = statusCode;

        var result = JsonSerializer.Serialize(new 
        { 
            error = message,
            stackTrace = _environment.IsDevelopment() ? exception.StackTrace : null
        });

        await response.WriteAsync(result);
    }

    private (int statusCode, string message) GetResponseDetails(Exception exception)
    {
        return exception switch
        {
            ValidationException validationException => (
                StatusCodes.Status400BadRequest,
                string.Join("; ", validationException.Errors.Select(e => e.ErrorMessage))),
                
            NotFoundException notFoundException => (
                StatusCodes.Status404NotFound,
                notFoundException.Message),
                
            ForbiddenAccessException forbiddenAccessException => (
                StatusCodes.Status403Forbidden,
                forbiddenAccessException.Message),
                
            UnauthorizedAccessException _ => (
                StatusCodes.Status401Unauthorized,
                "Unauthorized"),
                
            _ => (
                StatusCodes.Status500InternalServerError,
                "An error occurred while processing your request.")
        };
    }
}

// Extension method to register the middleware
public static class ErrorHandlingMiddlewareExtensions
{
    public static IApplicationBuilder UseErrorHandling(this IApplicationBuilder builder)
    {
        return builder.UseMiddleware<ErrorHandlingMiddleware>();
    }
}
```

### Custom Exception Types

The application defines several custom exception types to represent different error conditions:

```csharp
// Base exception for application-specific exceptions
public abstract class ApplicationException : Exception
{
    protected ApplicationException(string message)
        : base(message)
    {
    }
}

// Exception for validation errors
public class ValidationException : ApplicationException
{
    public ValidationException(IEnumerable<ValidationFailure> failures)
        : base("One or more validation failures have occurred.")
    {
        Errors = failures
            .GroupBy(e => e.PropertyName, e => e.ErrorMessage)
            .ToDictionary(
                failureGroup => failureGroup.Key,
                failureGroup => failureGroup.ToArray());
    }

    public IDictionary<string, string[]> Errors { get; }
}

// Exception for entity not found
public class NotFoundException : ApplicationException
{
    public NotFoundException(string name, object key)
        : base($"Entity \"{name}\" ({key}) was not found.")
    {
    }
}

// Exception for forbidden access
public class ForbiddenAccessException : ApplicationException
{
    public ForbiddenAccessException()
        : base("You do not have permission to access this resource.")
    {
    }
}
```

### Exception Logging

All exceptions are logged using Serilog's structured logging capabilities:

```csharp
try
{
    // Some operation that might throw an exception
}
catch (Exception ex)
{
    _logger.LogError(ex, "Error occurred while {Operation}", "processing request");
    throw;
}
```

### API Error Responses

API controllers return standardized error responses using the ProblemDetails format:

```csharp
[ApiController]
public class ApiControllerBase : ControllerBase
{
    [NonAction]
    protected ActionResult Problem(Exception exception)
    {
        return exception switch
        {
            ValidationException validationException => ValidationProblem(
                new ValidationProblemDetails(validationException.Errors)
                {
                    Status = StatusCodes.Status400BadRequest
                }),
                
            NotFoundException _ => NotFound(),
            
            ForbiddenAccessException _ => Forbid(),
            
            _ => Problem()
        };
    }
}
```

## Authentication and Authorization Flow

The Wendover HOA application implements a comprehensive authentication and authorization strategy using ASP.NET Core Identity and JWT tokens.

### Authentication Configuration

Authentication is configured during application startup:

```csharp
public void ConfigureServices(IServiceCollection services)
{
    // Add Identity
    services.AddIdentity<ApplicationUser, IdentityRole<int>>(options =>
    {
        // Password settings
        options.Password.RequireDigit = true;
        options.Password.RequireLowercase = true;
        options.Password.RequireUppercase = true;
        options.Password.RequireNonAlphanumeric = true;
        options.Password.RequiredLength = 12;
        
        // Lockout settings
        options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(15);
        options.Lockout.MaxFailedAccessAttempts = 5;
        
        // User settings
        options.User.RequireUniqueEmail = true;
        
        // Sign-in settings
        options.SignIn.RequireConfirmedEmail = true;
        options.SignIn.RequireConfirmedAccount = true;
    })
    .AddEntityFrameworkStores<ApplicationDbContext>()
    .AddDefaultTokenProviders();
    
    // Add JWT authentication
    services.AddAuthentication(options =>
    {
        options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
        options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
    })
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = Configuration["Jwt:Issuer"],
            ValidAudience = Configuration["Jwt:Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(Configuration["Jwt:Key"]))
        };
        
        options.Events = new JwtBearerEvents
        {
            OnAuthenticationFailed = context =>
            {
                if (context.Exception.GetType() == typeof(SecurityTokenExpiredException))
                {
                    context.Response.Headers.Add("Token-Expired", "true");
                }
                return Task.CompletedTask;
            }
        };
    })
    .AddGoogle(options =>
    {
        options.ClientId = Configuration["Authentication:Google:ClientId"];
        options.ClientSecret = Configuration["Authentication:Google:ClientSecret"];
    })
    .AddMicrosoftAccount(options =>
    {
        options.ClientId = Configuration["Authentication:Microsoft:ClientId"];
        options.ClientSecret = Configuration["Authentication:Microsoft:ClientSecret"];
    })
    .AddApple(options =>
    {
        options.ClientId = Configuration["Authentication:Apple:ClientId"];
        options.KeyId = Configuration["Authentication:Apple:KeyId"];
        options.TeamId = Configuration["Authentication:Apple:TeamId"];
        options.PrivateKey = Configuration["Authentication:Apple:PrivateKey"];
    });
    
    // Add authorization policies
    services.AddAuthorization(options =>
    {
        options.AddPolicy("RequireAdministratorRole", policy =>
            policy.RequireRole("Administrator"));
            
        options.AddPolicy("RequireBoardMemberRole", policy =>
            policy.RequireRole("Administrator", "BoardMember"));
            
        options.AddPolicy("RequireCommitteeMemberRole", policy =>
            policy.RequireRole("Administrator", "BoardMember", "CommitteeMember"));
            
        options.AddPolicy("RequireResidentRole", policy =>
            policy.RequireRole("Administrator", "BoardMember", "CommitteeMember", "Resident"));
    });
}
```

### Authentication Flow

The authentication flow in the Wendover HOA application follows these steps:

1. **User Registration**:
   - User registers with email, password, and personal information
   - Confirmation email is sent to the user
   - User confirms email by clicking on the link
   - User account is activated

2. **User Login**:
   - User provides email and password
   - If credentials are valid, a JWT token is generated and returned
   - Token contains user ID, roles, and claims
   - Token is stored in local storage or HTTP-only cookie

3. **External Authentication**:
   - User clicks on external provider button (Google, Microsoft, Apple)
   - User is redirected to the provider's login page
   - After successful authentication, user is redirected back to the application
   - Application creates or updates user account with provider information
   - JWT token is generated and returned

4. **Token Validation**:
   - Each request to a protected endpoint includes the JWT token in the Authorization header
   - Token is validated for authenticity, expiration, and issuer
   - If valid, user identity is established for the request

5. **Token Refresh**:
   - When token is about to expire, a refresh token is used to obtain a new JWT token
   - Refresh token is stored in an HTTP-only cookie for security
   - If refresh token is valid, a new JWT token is generated and returned

### Authorization Flow

The authorization flow in the Wendover HOA application follows these steps:

1. **Role-Based Authorization**:
   - User roles are stored in the database and included in the JWT token
   - Controllers and actions are decorated with `[Authorize]` attributes specifying required roles
   - If user does not have the required role, a 403 Forbidden response is returned

2. **Policy-Based Authorization**:
   - Policies are defined in the startup configuration
   - Controllers and actions are decorated with `[Authorize(Policy = "PolicyName")]` attributes
   - If user does not satisfy the policy, a 403 Forbidden response is returned

3. **Resource-Based Authorization**:
   - For more complex authorization scenarios, resource-based authorization is used
   - Authorization handlers check if the user has permission to access a specific resource
   - If user does not have permission, a 403 Forbidden response is returned

4. **MediatR Authorization Behavior**:
   - Commands and queries are decorated with `[Authorize]` attributes
   - Authorization behavior checks if user has the required role or satisfies the policy
   - If not, an exception is thrown and handled by the global exception handler

### CurrentUserService

The application uses a `CurrentUserService` to access the current user's information:

```csharp
public interface ICurrentUserService
{
    int UserId { get; }
    string UserName { get; }
    bool IsAuthenticated { get; }
    IEnumerable<string> Roles { get; }
    bool IsInRole(string role);
}

public class CurrentUserService : ICurrentUserService
{
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly UserManager<ApplicationUser> _userManager;

    public CurrentUserService(
        IHttpContextAccessor httpContextAccessor,
        UserManager<ApplicationUser> userManager)
    {
        _httpContextAccessor = httpContextAccessor;
        _userManager = userManager;
    }

    public int UserId => 
        int.TryParse(_httpContextAccessor.HttpContext?.User?.FindFirstValue(ClaimTypes.NameIdentifier), out var id) 
            ? id 
            : 0;

    public string UserName => 
        _httpContextAccessor.HttpContext?.User?.FindFirstValue(ClaimTypes.Name);

    public bool IsAuthenticated => 
        _httpContextAccessor.HttpContext?.User?.Identity?.IsAuthenticated ?? false;

    public IEnumerable<string> Roles => 
        _httpContextAccessor.HttpContext?.User?.Claims
            .Where(c => c.Type == ClaimTypes.Role)
            .Select(c => c.Value);

    public bool IsInRole(string role) => 
        _httpContextAccessor.HttpContext?.User?.IsInRole(role) ?? false;
}
```

## Caching Strategy

The Wendover HOA application implements a comprehensive caching strategy to improve performance and reduce database load.

### Caching Configuration

Caching is configured during application startup:

```csharp
public void ConfigureServices(IServiceCollection services)
{
    // Add memory cache
    services.AddMemoryCache();
    
    // Add distributed cache
    services.AddStackExchangeRedisCache(options =>
    {
        options.Configuration = Configuration.GetConnectionString("Redis");
        options.InstanceName = "WendoverHOA:";
    });
    
    // Add response caching
    services.AddResponseCaching();
    
    // Add output caching
    services.AddOutputCache(options =>
    {
        options.AddBasePolicy(builder => builder.Cache());
        
        options.AddPolicy("AnnouncementsList", builder =>
            builder.Cache()
                   .Expire(TimeSpan.FromMinutes(10))
                   .SetVaryByQuery("includeExpired")
                   .Tag("announcements"));
                   
        options.AddPolicy("DocumentsList", builder =>
            builder.Cache()
                   .Expire(TimeSpan.FromMinutes(15))
                   .SetVaryByQuery("category")
                   .Tag("documents"));
    });
}

public void Configure(IApplicationBuilder app)
{
    // Use response caching
    app.UseResponseCaching();
    
    // Use output caching
    app.UseOutputCache();
}
```

### Caching Service

The application uses a `CacheService` to abstract caching operations:

```csharp
public interface ICacheService
{
    Task<T> GetOrCreateAsync<T>(string key, Func<Task<T>> factory, TimeSpan? expiration = null);
    Task RemoveAsync(string key);
    Task RemoveByPrefixAsync(string prefix);
}

public class CacheService : ICacheService
{
    private readonly IDistributedCache _distributedCache;
    private readonly ILogger<CacheService> _logger;

    public CacheService(
        IDistributedCache distributedCache,
        ILogger<CacheService> logger)
    {
        _distributedCache = distributedCache;
        _logger = logger;
    }

    public async Task<T> GetOrCreateAsync<T>(string key, Func<Task<T>> factory, TimeSpan? expiration = null)
    {
        var cachedValue = await _distributedCache.GetStringAsync(key);
        
        if (cachedValue != null)
        {
            _logger.LogDebug("Cache hit for key {Key}", key);
            return JsonSerializer.Deserialize<T>(cachedValue);
        }
        
        _logger.LogDebug("Cache miss for key {Key}", key);
        var result = await factory();
        
        var options = new DistributedCacheEntryOptions
        {
            AbsoluteExpirationRelativeToNow = expiration ?? TimeSpan.FromMinutes(5)
        };
        
        await _distributedCache.SetStringAsync(
            key, 
            JsonSerializer.Serialize(result), 
            options);
            
        return result;
    }

    public async Task RemoveAsync(string key)
    {
        _logger.LogDebug("Removing cache entry for key {Key}", key);
        await _distributedCache.RemoveAsync(key);
    }

    public async Task RemoveByPrefixAsync(string prefix)
    {
        _logger.LogDebug("Removing cache entries with prefix {Prefix}", prefix);
        
        // Note: This is a simplified implementation
        // In a real application, you would need to track keys with the given prefix
        // or use a Redis-specific implementation that supports pattern matching
    }
}
```

### Caching in Queries

Queries use the `CacheService` to cache results:

```csharp
public class GetAnnouncementsQueryHandler 
    : IRequestHandler<GetAnnouncementsQuery, List<AnnouncementDto>>
{
    private readonly IAnnouncementRepository _announcementRepository;
    private readonly IMapper _mapper;
    private readonly ICacheService _cacheService;

    public GetAnnouncementsQueryHandler(
        IAnnouncementRepository announcementRepository,
        IMapper mapper,
        ICacheService cacheService)
    {
        _announcementRepository = announcementRepository;
        _mapper = mapper;
        _cacheService = cacheService;
    }

    public async Task<List<AnnouncementDto>> Handle(
        GetAnnouncementsQuery request, 
        CancellationToken cancellationToken)
    {
        var cacheKey = $"announcements:{request.IncludeExpired}";
        
        return await _cacheService.GetOrCreateAsync(
            cacheKey,
            async () =>
            {
                var announcements = await _announcementRepository.GetAllAsync();
                
                if (!request.IncludeExpired)
                {
                    announcements = announcements
                        .Where(a => a.ExpiryDate == null || a.ExpiryDate >= DateTime.UtcNow)
                        .ToList();
                }
                
                return _mapper.Map<List<AnnouncementDto>>(announcements);
            },
            TimeSpan.FromMinutes(10));
    }
}
```

### Cache Invalidation

The application uses a cache invalidation strategy to ensure that cached data is updated when the underlying data changes:

```csharp
public class CreateAnnouncementCommandHandler 
    : IRequestHandler<CreateAnnouncementCommand, int>
{
    private readonly IAnnouncementRepository _announcementRepository;
    private readonly ICurrentUserService _currentUserService;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICacheService _cacheService;
    private readonly IOutputCacheStore _outputCacheStore;

    public CreateAnnouncementCommandHandler(
        IAnnouncementRepository announcementRepository,
        ICurrentUserService currentUserService,
        IUnitOfWork unitOfWork,
        ICacheService cacheService,
        IOutputCacheStore outputCacheStore)
    {
        _announcementRepository = announcementRepository;
        _currentUserService = currentUserService;
        _unitOfWork = unitOfWork;
        _cacheService = cacheService;
        _outputCacheStore = outputCacheStore;
    }

    public async Task<int> Handle(
        CreateAnnouncementCommand request, 
        CancellationToken cancellationToken)
    {
        var userId = _currentUserService.UserId;
        
        var announcement = new Announcement(
            request.Title,
            request.Content,
            request.PublishDate,
            request.ExpiryDate,
            request.ImportanceLevel,
            userId);

        _announcementRepository.Add(announcement);
        
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        
        // Invalidate cache
        await _cacheService.RemoveByPrefixAsync("announcements:");
        await _outputCacheStore.EvictByTagAsync("announcements", cancellationToken);
        
        return announcement.Id;
    }
}
```

## Conclusion

The cross-cutting concerns implementation in the Wendover HOA application provides a solid foundation for handling logging, exception handling, authentication/authorization, and caching. By implementing these concerns in a consistent and reusable way, the application achieves a high degree of maintainability, security, and performance.
