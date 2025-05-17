# Logging Strategy

This document outlines the comprehensive logging strategy for the Wendover HOA application, ensuring consistent and effective logging across all layers of the application in alignment with Clean Architecture principles.

## Logging Principles

The Wendover HOA application follows these core logging principles:

1. **Structured Logging**: Use structured logging for better searchability and analysis
2. **Contextual Information**: Include relevant context with each log entry
3. **Appropriate Log Levels**: Use the correct log level for different types of events
4. **Separation of Concerns**: Maintain clean separation between application code and logging
5. **Security-First Approach**: Never log sensitive information
6. **Performance Awareness**: Minimize logging overhead in production
7. **Comprehensive Coverage**: Log important events across all application layers

## Logging Architecture

### Logging Framework

The application uses Serilog as the primary logging framework:

```csharp
// Program.cs
public static IHostBuilder CreateHostBuilder(string[] args) =>
    Host.CreateDefaultBuilder(args)
        .UseSerilog((context, services, configuration) => configuration
            .ReadFrom.Configuration(context.Configuration)
            .ReadFrom.Services(services)
            .Enrich.FromLogContext()
            .Enrich.WithMachineName()
            .Enrich.WithEnvironmentName()
            .WriteTo.Console(outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss} [{Level:u3}] {Message:lj} {Properties:j}{NewLine}{Exception}")
            .WriteTo.File(
                path: "logs/wendoverhoa-.log",
                rollingInterval: RollingInterval.Day,
                outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss} [{Level:u3}] {Message:lj} {Properties:j}{NewLine}{Exception}")
            .WriteTo.ApplicationInsights(
                services.GetRequiredService<TelemetryConfiguration>(),
                TelemetryConverter.Traces))
        .ConfigureWebHostDefaults(webBuilder =>
        {
            webBuilder.UseStartup<Startup>();
        });
```

### Log Configuration

Logging is configured in `appsettings.json`:

```json
{
  "Serilog": {
    "MinimumLevel": {
      "Default": "Information",
      "Override": {
        "Microsoft": "Warning",
        "Microsoft.Hosting.Lifetime": "Information",
        "Microsoft.EntityFrameworkCore": "Warning",
        "System": "Warning"
      }
    },
    "Enrich": ["FromLogContext", "WithMachineName", "WithThreadId"],
    "Properties": {
      "Application": "WendoverHOA"
    }
  }
}
```

### Log Enrichment

Logs are enriched with contextual information:

```csharp
// Startup.cs
public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
{
    // Add correlation ID middleware
    app.Use(async (context, next) =>
    {
        var correlationId = context.Request.Headers["X-Correlation-ID"].FirstOrDefault() ?? Guid.NewGuid().ToString();
        context.Response.Headers["X-Correlation-ID"] = correlationId;
        
        using (LogContext.PushProperty("CorrelationId", correlationId))
        using (LogContext.PushProperty("RequestPath", context.Request.Path))
        using (LogContext.PushProperty("RequestMethod", context.Request.Method))
        using (LogContext.PushProperty("UserName", context.User.Identity?.Name ?? "Anonymous"))
        using (LogContext.PushProperty("UserRoles", string.Join(",", GetUserRoles(context.User))))
        using (LogContext.PushProperty("IPAddress", context.Connection.RemoteIpAddress?.ToString()))
        using (LogContext.PushProperty("UserAgent", context.Request.Headers["User-Agent"].ToString()))
        {
            await next();
        }
    });
    
    // Other middleware configuration
}

private IEnumerable<string> GetUserRoles(ClaimsPrincipal user)
{
    if (user.Identity?.IsAuthenticated != true)
    {
        return Array.Empty<string>();
    }
    
    return user.Claims
        .Where(c => c.Type == ClaimTypes.Role)
        .Select(c => c.Value);
}
```

## Log Levels

The application uses the following log levels:

| Level | Usage | Example |
|-------|-------|---------|
| **Trace** | Detailed debugging information | Method entry/exit, variable values |
| **Debug** | Debugging information | Configuration values, state changes |
| **Information** | Normal application flow | User logged in, record created |
| **Warning** | Potential issues that don't stop execution | Retrying operation, deprecated feature used |
| **Error** | Errors that prevent an operation from completing | Exception thrown, operation failed |
| **Critical** | Critical errors that may cause application failure | Database connection lost, unhandled exception |

### Log Level Guidelines

```csharp
// Trace - Very detailed information
_logger.LogTrace("Entering method {MethodName} with parameters {@Parameters}", 
    nameof(CreateProperty), new { command.Address, command.City });

// Debug - Debugging information
_logger.LogDebug("Property validation completed with {ValidationResult}", 
    validationResult.IsValid ? "success" : "failure");

// Information - Normal application flow
_logger.LogInformation("Property {PropertyId} created by user {UserId}", 
    property.Id, userId);

// Warning - Potential issues
_logger.LogWarning("Retrying database operation (Attempt {AttemptNumber} of {MaxAttempts})", 
    attempt, maxAttempts);

// Error - Operation failures
_logger.LogError(exception, "Failed to create property. User: {UserId}, Address: {Address}", 
    userId, command.Address);

// Critical - Application-threatening issues
_logger.LogCritical(exception, "Database connection failed after {RetryCount} attempts", 
    retryCount);
```

## Layer-Specific Logging

### Domain Layer

Domain layer logging focuses on business rule violations and domain events:

```csharp
public class Property
{
    private readonly ILogger<Property> _logger;
    
    public Property(ILogger<Property> logger)
    {
        _logger = logger;
    }
    
    public void AssignResident(Resident resident)
    {
        if (Status != PropertyStatus.Vacant)
        {
            _logger.LogWarning("Attempted to assign resident {ResidentId} to non-vacant property {PropertyId}", 
                resident.Id, Id);
            throw new DomainException("Cannot assign resident to a non-vacant property.");
        }
        
        Resident = resident;
        ResidentId = resident.Id;
        Status = PropertyStatus.Occupied;
        
        _logger.LogInformation("Resident {ResidentId} assigned to property {PropertyId}", 
            resident.Id, Id);
    }
}
```

### Application Layer

Application layer logging focuses on command/query execution and validation:

```csharp
public class CreatePropertyCommandHandler : IRequestHandler<CreatePropertyCommand, int>
{
    private readonly IPropertyRepository _propertyRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<CreatePropertyCommandHandler> _logger;
    
    public async Task<int> Handle(CreatePropertyCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Creating property at {Address}, {City}, {State}", 
            request.Address, request.City, request.State);
        
        var property = new Property
        {
            Address = request.Address,
            City = request.City,
            State = request.State,
            ZipCode = request.ZipCode,
            Status = PropertyStatus.Vacant
        };
        
        try
        {
            var result = await _propertyRepository.AddAsync(property, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);
            
            _logger.LogInformation("Property {PropertyId} created successfully", result.Id);
            
            return result.Id;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to create property at {Address}", request.Address);
            throw;
        }
    }
}
```

### Infrastructure Layer

Infrastructure layer logging focuses on external service interactions and database operations:

```csharp
public class PropertyRepository : IPropertyRepository
{
    private readonly ApplicationDbContext _dbContext;
    private readonly ILogger<PropertyRepository> _logger;
    
    public async Task<Property> GetByIdAsync(int id, CancellationToken cancellationToken)
    {
        _logger.LogDebug("Retrieving property with ID {PropertyId}", id);
        
        try
        {
            var property = await _dbContext.Properties
                .Include(p => p.Resident)
                .FirstOrDefaultAsync(p => p.Id == id, cancellationToken);
                
            if (property == null)
            {
                _logger.LogWarning("Property with ID {PropertyId} not found", id);
                throw new NotFoundException(nameof(Property), id);
            }
            
            _logger.LogDebug("Retrieved property {PropertyId} successfully", id);
            return property;
        }
        catch (Exception ex) when (ex is not NotFoundException)
        {
            _logger.LogError(ex, "Error retrieving property with ID {PropertyId}", id);
            throw;
        }
    }
}

public class EmailService : IEmailService
{
    private readonly ISmtpClient _smtpClient;
    private readonly ILogger<EmailService> _logger;
    
    public async Task SendEmailAsync(string to, string subject, string body)
    {
        _logger.LogInformation("Sending email to {Recipient} with subject {Subject}", 
            to, subject);
        
        try
        {
            await _smtpClient.SendAsync(to, subject, body);
            _logger.LogInformation("Email sent successfully to {Recipient}", to);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send email to {Recipient} with subject {Subject}", 
                to, subject);
            throw;
        }
    }
}
```

### Presentation Layer

Presentation layer logging focuses on API requests and responses:

```csharp
[ApiController]
[Route("api/v{version:apiVersion}/[controller]")]
public class PropertiesController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly ILogger<PropertiesController> _logger;
    
    [HttpGet("{id}")]
    public async Task<ActionResult<ApiResponse<PropertyDto>>> GetProperty(int id)
    {
        _logger.LogInformation("Received request to get property {PropertyId}", id);
        
        try
        {
            var property = await _mediator.Send(new GetPropertyByIdQuery { Id = id });
            
            _logger.LogInformation("Successfully retrieved property {PropertyId}", id);
            
            return Ok(new ApiResponse<PropertyDto> { Success = true, Data = property });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving property {PropertyId}", id);
            throw;
        }
    }
}
```

## Logging Behaviors

A MediatR behavior logs all commands and queries:

```csharp
public class LoggingBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
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
            "Handling {RequestName} for user {UserId} with data {@Request}", 
            requestName, 
            userId,
            SanitizeRequest(request));
        
        try
        {
            var result = await next();
            
            _logger.LogInformation(
                "Handled {RequestName} for user {UserId} successfully", 
                requestName, 
                userId);
            
            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Error handling {RequestName} for user {UserId}", 
                requestName, 
                userId);
            
            throw;
        }
    }
    
    private object SanitizeRequest(TRequest request)
    {
        // Create a sanitized copy of the request that excludes sensitive properties
        var sanitized = new Dictionary<string, object>();
        
        foreach (var property in typeof(TRequest).GetProperties())
        {
            if (IsSensitiveProperty(property))
            {
                sanitized[property.Name] = "[REDACTED]";
            }
            else
            {
                sanitized[property.Name] = property.GetValue(request);
            }
        }
        
        return sanitized;
    }
    
    private bool IsSensitiveProperty(PropertyInfo property)
    {
        var sensitiveNames = new[] 
        { 
            "password", "secret", "key", "token", "credential", 
            "ssn", "creditcard", "cardnumber", "cvv" 
        };
        
        return sensitiveNames.Any(s => property.Name.ToLower().Contains(s));
    }
}
```

## Performance Considerations

### Conditional Logging

Use conditional logging to minimize overhead:

```csharp
// Check if log level is enabled before expensive operations
if (_logger.IsEnabled(LogLevel.Debug))
{
    _logger.LogDebug("Property details: {@PropertyDetails}", 
        new { property.Id, property.Address, property.Status });
}
```

### Asynchronous Logging

Configure Serilog for asynchronous logging:

```csharp
.WriteTo.Async(a => a.File(
    path: "logs/wendoverhoa-.log",
    rollingInterval: RollingInterval.Day,
    outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss} [{Level:u3}] {Message:lj} {Properties:j}{NewLine}{Exception}"))
```

### Log Sampling

Implement log sampling for high-volume events:

```csharp
// Configure log sampling for specific events
services.Configure<LoggingOptions>(options =>
{
    options.SamplingRules.Add(new SamplingRule
    {
        SourceContext = "WendoverHOA.Web.Middleware.RequestLoggingMiddleware",
        SampleRate = 0.1 // Log 10% of requests
    });
});
```

## Security Considerations

### Sensitive Data Protection

Never log sensitive information:

```csharp
// Bad - Logs sensitive information
_logger.LogInformation("User {UserName} logged in with password {Password}", 
    username, password);

// Good - Logs only necessary information
_logger.LogInformation("User {UserName} logged in successfully", username);
```

### Log Data Sanitization

Sanitize log data to remove sensitive information:

```csharp
public static class LogSanitizer
{
    public static string SanitizeData(string data)
    {
        if (string.IsNullOrEmpty(data))
        {
            return data;
        }
        
        // Remove connection strings
        data = Regex.Replace(data, @"(connection string|connectionstring)=([^;])*", 
            "connection string=[REDACTED]", RegexOptions.IgnoreCase);
        
        // Remove API keys
        data = Regex.Replace(data, @"(api[_-]?key|apikey)=([^&\s]*)", 
            "api_key=[REDACTED]", RegexOptions.IgnoreCase);
        
        // Remove passwords
        data = Regex.Replace(data, @"(password|pwd)=([^;])*", 
            "password=[REDACTED]", RegexOptions.IgnoreCase);
        
        // Remove credit card numbers
        data = Regex.Replace(data, @"\b(?:\d[ -]*?){13,16}\b", 
            "[REDACTED-CARD-NUMBER]", RegexOptions.IgnoreCase);
        
        // Remove SSNs
        data = Regex.Replace(data, @"\b\d{3}[-]?\d{2}[-]?\d{4}\b", 
            "[REDACTED-SSN]", RegexOptions.IgnoreCase);
        
        return data;
    }
}
```

### Log Access Control

Restrict access to log files:

```csharp
// Configure file permissions for log files
services.Configure<FileLoggerOptions>(options =>
{
    options.FilePermissions = new FilePermissions
    {
        FileMode = FileMode.Append,
        FileAccess = FileAccess.Write,
        FileShare = FileShare.Read
    };
});
```

## Log Storage and Retention

### Log Storage

Logs are stored in multiple locations:

1. **Local Files**: Rolling daily log files for development and troubleshooting
2. **Application Insights**: Cloud-based storage for production monitoring
3. **Blob Storage**: Long-term archival storage for compliance

```csharp
// Configure log storage
.WriteTo.File(
    path: "logs/wendoverhoa-.log",
    rollingInterval: RollingInterval.Day,
    retainedFileCountLimit: 31) // Keep 31 days of logs
.WriteTo.ApplicationInsights(
    services.GetRequiredService<TelemetryConfiguration>(),
    TelemetryConverter.Traces)
.WriteTo.AzureBlobStorage(
    connectionString: Configuration["Storage:ConnectionString"],
    storageContainerName: "logs",
    storageFileName: "{yyyy}/{MM}/{dd}/wendoverhoa-{HH}.log")
```

### Log Retention

Log retention policies are configured based on environment and log level:

| Environment | Log Level | Retention Period |
|-------------|-----------|------------------|
| Development | All | 7 days |
| Staging | Information+ | 30 days |
| Production | Warning+ | 1 year |
| Production | Information | 90 days |
| Production | Debug and below | 7 days |

```csharp
// Configure log retention in Azure Blob Storage
services.AddLogRetentionPolicy(options =>
{
    options.RetentionPolicies.Add(new RetentionPolicy
    {
        Environment = "Production",
        LogLevel = LogLevel.Warning,
        RetentionPeriod = TimeSpan.FromDays(365)
    });
    
    options.RetentionPolicies.Add(new RetentionPolicy
    {
        Environment = "Production",
        LogLevel = LogLevel.Information,
        RetentionPeriod = TimeSpan.FromDays(90)
    });
    
    // Additional policies...
});
```

## Log Analysis and Monitoring

### Application Insights Integration

```csharp
// Configure Application Insights
services.AddApplicationInsightsTelemetry(Configuration["ApplicationInsights:InstrumentationKey"]);

// Track custom events
_telemetryClient.TrackEvent("PropertyCreated", new Dictionary<string, string>
{
    ["PropertyId"] = property.Id.ToString(),
    ["Address"] = property.Address,
    ["UserId"] = userId.ToString()
});
```

### Log Queries

Common log queries for troubleshooting:

```sql
-- Find errors for a specific user
traces
| where timestamp > ago(24h)
| where severityLevel >= 3
| where customDimensions.UserId == "123"
| order by timestamp desc

-- Track performance of property creation
traces
| where timestamp > ago(7d)
| where message startswith "Handling CreatePropertyCommand"
| project timestamp, duration, customDimensions.UserId, customDimensions.Address
| summarize avg(duration), count() by bin(timestamp, 1h)
| render timechart

-- Find failed payment attempts
traces
| where timestamp > ago(24h)
| where message contains "payment" and severityLevel >= 3
| project timestamp, message, customDimensions.UserId, customDimensions.PropertyId, customDimensions.Amount
| order by timestamp desc
```

### Alerting

Configure alerts for critical issues:

```csharp
// Configure alerts
services.AddAlertRules(options =>
{
    options.AlertRules.Add(new AlertRule
    {
        Name = "HighErrorRate",
        Query = @"
            traces
            | where timestamp > ago(5m)
            | where severityLevel >= 3
            | summarize ErrorCount = count() by bin(timestamp, 1m)
            | where ErrorCount > 10
        ",
        Frequency = TimeSpan.FromMinutes(5),
        Window = TimeSpan.FromMinutes(5),
        Threshold = 10,
        Severity = AlertSeverity.Critical,
        NotificationChannels = new[] { "Email", "Teams" }
    });
    
    // Additional alert rules...
});
```

## Testing Logging

### Unit Testing

```csharp
[Fact]
public async Task Handle_LogsInformationBeforeAndAfterExecution()
{
    // Arrange
    var command = new CreatePropertyCommand
    {
        Address = "123 Main St",
        City = "Bedford",
        State = "TX",
        ZipCode = "76021"
    };
    
    var mockLogger = new Mock<ILogger<LoggingBehavior<CreatePropertyCommand, int>>>();
    var mockCurrentUserService = new Mock<ICurrentUserService>();
    mockCurrentUserService.Setup(s => s.UserId).Returns("user-123");
    
    var behavior = new LoggingBehavior<CreatePropertyCommand, int>(
        mockLogger.Object,
        mockCurrentUserService.Object);
    
    var mockNext = new Mock<RequestHandlerDelegate<int>>();
    mockNext.Setup(n => n()).ReturnsAsync(42);
    
    // Act
    var result = await behavior.Handle(command, mockNext.Object, CancellationToken.None);
    
    // Assert
    mockLogger.Verify(
        x => x.Log(
            LogLevel.Information,
            It.IsAny<EventId>(),
            It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Handling CreatePropertyCommand")),
            null,
            It.IsAny<Func<It.IsAnyType, Exception, string>>()),
        Times.Once);
    
    mockLogger.Verify(
        x => x.Log(
            LogLevel.Information,
            It.IsAny<EventId>(),
            It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Handled CreatePropertyCommand")),
            null,
            It.IsAny<Func<It.IsAnyType, Exception, string>>()),
        Times.Once);
}
```

### Integration Testing

```csharp
[Fact]
public async Task CreateProperty_LogsSuccessfulCreation()
{
    // Arrange
    var client = _factory.CreateClient();
    var command = new CreatePropertyCommand
    {
        Address = "123 Main St",
        City = "Bedford",
        State = "TX",
        ZipCode = "76021"
    };
    
    // Configure test logger
    var loggerProvider = _factory.Services.GetRequiredService<ILoggerProvider>();
    var testSink = (TestSink)typeof(LoggerProvider)
        .GetField("_sink", BindingFlags.NonPublic | BindingFlags.Instance)
        .GetValue(loggerProvider);
    
    // Act
    var response = await client.PostAsJsonAsync("/api/v1/properties", command);
    
    // Assert
    response.EnsureSuccessStatusCode();
    
    // Check logs
    var createLog = testSink.Writes.FirstOrDefault(w => 
        w.LogLevel == LogLevel.Information && 
        w.Message.Contains("Property") && 
        w.Message.Contains("created successfully"));
    
    Assert.NotNull(createLog);
    Assert.Contains("123 Main St", createLog.Message);
}
```

## Logging Documentation

### Log Format

All logs follow this format:

```
{Timestamp:yyyy-MM-dd HH:mm:ss} [{Level:u3}] {Message:lj} {Properties:j}{NewLine}{Exception}
```

Example:
```
2025-05-15 10:30:45 [INF] Property 123 created by user user-123 {"PropertyId":123,"UserId":"user-123","Address":"123 Main St","CorrelationId":"abc-123","RequestPath":"/api/v1/properties","RequestMethod":"POST"}
```

### Log Properties

Standard properties included in all logs:

| Property | Description | Example |
|----------|-------------|---------|
| Timestamp | When the log was created | `2025-05-15 10:30:45` |
| Level | Log level | `INF`, `WRN`, `ERR` |
| Message | Log message | `Property 123 created by user user-123` |
| CorrelationId | Unique ID for tracking requests | `abc-123` |
| RequestPath | API endpoint path | `/api/v1/properties` |
| RequestMethod | HTTP method | `POST` |
| UserName | User name | `john.doe@example.com` |
| UserId | User ID | `user-123` |
| IPAddress | Client IP address | `192.168.1.1` |
| UserAgent | Client user agent | `Mozilla/5.0...` |
| Application | Application name | `WendoverHOA` |
| Environment | Environment name | `Production` |
| MachineName | Server name | `web-server-01` |
| Exception | Exception details if applicable | Stack trace and message |

## Best Practices

1. **Be Consistent**: Use consistent log formats and levels across the application
2. **Be Concise**: Keep log messages clear and to the point
3. **Be Contextual**: Include relevant context with each log entry
4. **Be Secure**: Never log sensitive information
5. **Be Selective**: Log at the appropriate level to avoid noise
6. **Be Performant**: Minimize logging overhead in production
7. **Be Comprehensive**: Log important events across all application layers
8. **Be Actionable**: Ensure logs provide enough information to diagnose issues

## Conclusion

This logging strategy provides a comprehensive approach to logging across all layers of the Wendover HOA application. By following these guidelines, the application will maintain a consistent, secure, and effective logging system that supports troubleshooting, monitoring, and auditing needs.

The strategy ensures:

1. **Consistency**: All logs follow a consistent format and structure
2. **Security**: Sensitive information is never logged
3. **Performance**: Logging overhead is minimized in production
4. **Usability**: Logs provide actionable information for troubleshooting
5. **Compliance**: Log retention policies meet regulatory requirements

By implementing this strategy, the Wendover HOA application will maintain comprehensive logs that support effective monitoring, troubleshooting, and auditing while adhering to Clean Architecture principles.
