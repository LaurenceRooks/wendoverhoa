# Error Handling Strategy

This document outlines the comprehensive error handling strategy for the Wendover HOA application, ensuring consistent error management across all layers of the application in alignment with Clean Architecture principles.

## Error Handling Principles

The Wendover HOA application follows these core error handling principles:

1. **Centralized Error Handling**: Implement a consistent approach across all application layers
2. **Meaningful Error Messages**: Provide clear, actionable error information
3. **Appropriate Error Types**: Use specific exception types for different error scenarios
4. **Graceful Degradation**: Ensure the application remains functional when possible
5. **Comprehensive Logging**: Log all errors with contextual information
6. **Security-First Approach**: Prevent sensitive information exposure in error messages
7. **User-Friendly Presentation**: Display appropriate error messages to end users

## Error Handling Architecture

### Exception Hierarchy

The application uses a custom exception hierarchy to represent different error types:

```csharp
// Base exception for all application-specific exceptions
public abstract class ApplicationException : Exception
{
    public string Code { get; }
    
    protected ApplicationException(string message, string code) 
        : base(message)
    {
        Code = code;
    }
}

// Domain layer exceptions
public class DomainException : ApplicationException
{
    public DomainException(string message, string code = "DOMAIN_ERROR") 
        : base(message, code) { }
}

// Application layer exceptions
public class ValidationException : ApplicationException
{
    public IReadOnlyDictionary<string, string[]> Errors { get; }
    
    public ValidationException(IReadOnlyDictionary<string, string[]> errors) 
        : base("One or more validation errors occurred.", "VALIDATION_ERROR")
    {
        Errors = errors;
    }
}

public class NotFoundException : ApplicationException
{
    public NotFoundException(string name, object key)
        : base($"Entity \"{name}\" ({key}) was not found.", "NOT_FOUND") { }
}

public class ForbiddenAccessException : ApplicationException
{
    public ForbiddenAccessException()
        : base("You do not have permission to access this resource.", "FORBIDDEN") { }
}

// Infrastructure layer exceptions
public class InfrastructureException : ApplicationException
{
    public InfrastructureException(string message, string code = "INFRASTRUCTURE_ERROR") 
        : base(message, code) { }
}

public class ExternalServiceException : ApplicationException
{
    public ExternalServiceException(string service, string message)
        : base($"External service {service} error: {message}", "EXTERNAL_SERVICE_ERROR") { }
}
```

### Global Exception Handling Middleware

A global exception handling middleware captures all unhandled exceptions:

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
        // Log the exception with contextual information
        _logger.LogError(
            exception,
            "Unhandled exception occurred. TraceId: {TraceId}, Path: {Path}, Method: {Method}, User: {User}",
            Activity.Current?.Id ?? context.TraceIdentifier,
            context.Request.Path,
            context.Request.Method,
            context.User.Identity?.Name ?? "Anonymous");

        // Set appropriate status code and prepare error response
        var (statusCode, response) = exception switch
        {
            ValidationException validationException => (
                StatusCodes.Status422UnprocessableEntity,
                CreateErrorResponse(validationException.Code, validationException.Message, validationException.Errors)
            ),
            NotFoundException notFoundException => (
                StatusCodes.Status404NotFound,
                CreateErrorResponse(notFoundException.Code, notFoundException.Message)
            ),
            ForbiddenAccessException forbiddenAccessException => (
                StatusCodes.Status403Forbidden,
                CreateErrorResponse(forbiddenAccessException.Code, forbiddenAccessException.Message)
            ),
            DomainException domainException => (
                StatusCodes.Status400BadRequest,
                CreateErrorResponse(domainException.Code, domainException.Message)
            ),
            ApplicationException applicationException => (
                StatusCodes.Status400BadRequest,
                CreateErrorResponse(applicationException.Code, applicationException.Message)
            ),
            _ => (
                StatusCodes.Status500InternalServerError,
                CreateErrorResponse(
                    "INTERNAL_SERVER_ERROR",
                    "An unexpected error occurred. Please try again later.",
                    _environment.IsDevelopment() ? new { Exception = exception.ToString() } : null)
            )
        };

        context.Response.StatusCode = statusCode;
        context.Response.ContentType = "application/json";
        
        await context.Response.WriteAsync(JsonSerializer.Serialize(response, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            WriteIndented = true
        }));
    }

    private static object CreateErrorResponse(string code, string message, object details = null)
    {
        return new
        {
            Success = false,
            Error = new
            {
                Code = code,
                Message = message,
                Details = details,
                TraceId = Activity.Current?.Id ?? HttpContext.TraceIdentifier
            }
        };
    }
}
```

## Layer-Specific Error Handling

### Domain Layer

In the domain layer, errors represent business rule violations:

```csharp
public class Property
{
    public void AssignResident(Resident resident)
    {
        if (Status != PropertyStatus.Vacant)
        {
            throw new DomainException("Cannot assign resident to a non-vacant property.");
        }
        
        Resident = resident;
        ResidentId = resident.Id;
        Status = PropertyStatus.Occupied;
    }
}
```

### Application Layer

The application layer handles validation and business logic errors:

```csharp
public class CreatePropertyCommandValidator : AbstractValidator<CreatePropertyCommand>
{
    public CreatePropertyCommandValidator()
    {
        RuleFor(p => p.Address).NotEmpty().MaximumLength(100);
        RuleFor(p => p.City).NotEmpty().MaximumLength(50);
        RuleFor(p => p.State).NotEmpty().Length(2);
        RuleFor(p => p.ZipCode).NotEmpty().Matches(@"^\d{5}(-\d{4})?$");
    }
}

public class CreatePropertyCommandHandler : IRequestHandler<CreatePropertyCommand, int>
{
    private readonly IPropertyRepository _propertyRepository;
    private readonly IUnitOfWork _unitOfWork;
    
    public async Task<int> Handle(CreatePropertyCommand request, CancellationToken cancellationToken)
    {
        // Validation handled by MediatR pipeline behavior
        
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
            return result.Id;
        }
        catch (DbUpdateException ex) when (IsDuplicateKeyException(ex))
        {
            throw new ValidationException(new Dictionary<string, string[]>
            {
                { "Address", new[] { "A property with this address already exists." } }
            });
        }
    }
}
```

### Infrastructure Layer

The infrastructure layer handles external service and database errors:

```csharp
public class PropertyRepository : IPropertyRepository
{
    private readonly ApplicationDbContext _dbContext;
    private readonly ILogger<PropertyRepository> _logger;
    
    public async Task<Property> GetByIdAsync(int id, CancellationToken cancellationToken)
    {
        try
        {
            var property = await _dbContext.Properties
                .Include(p => p.Resident)
                .FirstOrDefaultAsync(p => p.Id == id, cancellationToken);
                
            if (property == null)
            {
                throw new NotFoundException(nameof(Property), id);
            }
            
            return property;
        }
        catch (Exception ex) when (ex is not NotFoundException)
        {
            _logger.LogError(ex, "Error retrieving property with ID {PropertyId}", id);
            throw new InfrastructureException("An error occurred while retrieving the property.");
        }
    }
}

public class EmailService : IEmailService
{
    private readonly ISmtpClient _smtpClient;
    private readonly ILogger<EmailService> _logger;
    
    public async Task SendEmailAsync(string to, string subject, string body)
    {
        try
        {
            await _smtpClient.SendAsync(to, subject, body);
        }
        catch (SmtpException ex)
        {
            _logger.LogError(ex, "Failed to send email to {Recipient} with subject {Subject}", to, subject);
            throw new ExternalServiceException("Email Service", "Failed to send email.");
        }
    }
}
```

### Presentation Layer

The presentation layer handles user input validation and API response formatting:

```csharp
[ApiController]
[Route("api/v{version:apiVersion}/[controller]")]
public class PropertiesController : ControllerBase
{
    private readonly IMediator _mediator;
    
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(ApiResponse<PropertyDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<ErrorDetails>), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<PropertyDto>>> GetProperty(int id)
    {
        try
        {
            var property = await _mediator.Send(new GetPropertyByIdQuery { Id = id });
            return Ok(new ApiResponse<PropertyDto> { Success = true, Data = property });
        }
        catch (NotFoundException ex)
        {
            // This will be caught by the global exception middleware
            throw;
        }
    }
    
    [HttpPost]
    [ProducesResponseType(typeof(ApiResponse<int>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiResponse<ErrorDetails>), StatusCodes.Status422UnprocessableEntity)]
    public async Task<ActionResult<ApiResponse<int>>> CreateProperty(CreatePropertyCommand command)
    {
        var propertyId = await _mediator.Send(command);
        return CreatedAtAction(nameof(GetProperty), new { id = propertyId }, new ApiResponse<int> { Success = true, Data = propertyId });
    }
}
```

## Validation Pipeline

A MediatR pipeline behavior handles validation:

```csharp
public class ValidationBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
{
    private readonly IEnumerable<IValidator<TRequest>> _validators;
    
    public ValidationBehavior(IEnumerable<IValidator<TRequest>> validators)
    {
        _validators = validators;
    }
    
    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
    {
        if (!_validators.Any())
        {
            return await next();
        }
        
        var context = new ValidationContext<TRequest>(request);
        
        var validationResults = await Task.WhenAll(
            _validators.Select(v => v.ValidateAsync(context, cancellationToken)));
        
        var failures = validationResults
            .SelectMany(r => r.Errors)
            .Where(f => f != null)
            .GroupBy(x => x.PropertyName)
            .ToDictionary(
                g => g.Key,
                g => g.Select(x => x.ErrorMessage).ToArray()
            );
        
        if (failures.Count > 0)
        {
            throw new ValidationException(failures);
        }
        
        return await next();
    }
}
```

## Client-Side Error Handling

### API Response Wrapper

All API responses follow a consistent format:

```typescript
interface ApiResponse<T> {
  success: boolean;
  data?: T;
  error?: {
    code: string;
    message: string;
    details?: any;
    traceId: string;
  };
}
```

### Error Interceptor

An HTTP interceptor handles API errors:

```typescript
// Angular example
@Injectable()
export class ErrorInterceptor implements HttpInterceptor {
  constructor(private toastr: ToastrService, private router: Router) {}

  intercept(request: HttpRequest<any>, next: HttpHandler): Observable<HttpEvent<any>> {
    return next.handle(request).pipe(
      catchError((error: HttpErrorResponse) => {
        if (error.status === 0) {
          // Network error
          this.toastr.error('Unable to connect to the server. Please check your internet connection.');
        } else if (error.status === 401) {
          // Unauthorized
          this.toastr.error('Your session has expired. Please log in again.');
          this.router.navigate(['/login']);
        } else if (error.status === 403) {
          // Forbidden
          this.toastr.error('You do not have permission to perform this action.');
        } else if (error.status === 404) {
          // Not found
          this.toastr.error('The requested resource was not found.');
        } else if (error.status === 422) {
          // Validation error
          const response = error.error as ApiResponse<any>;
          if (response?.error?.details) {
            // Handle validation errors
            const validationErrors = response.error.details;
            for (const key in validationErrors) {
              if (validationErrors.hasOwnProperty(key)) {
                validationErrors[key].forEach((message: string) => {
                  this.toastr.error(message);
                });
              }
            }
          } else {
            this.toastr.error(response?.error?.message || 'Validation failed.');
          }
        } else {
          // Other errors
          const response = error.error as ApiResponse<any>;
          this.toastr.error(response?.error?.message || 'An unexpected error occurred.');
        }
        
        return throwError(() => error);
      })
    );
  }
}
```

## Error Logging Strategy

### Structured Logging

All errors are logged with structured data:

```csharp
// Configure Serilog
Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Information()
    .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
    .Enrich.FromLogContext()
    .Enrich.WithProperty("Application", "WendoverHOA")
    .Enrich.WithMachineName()
    .Enrich.WithEnvironmentName()
    .WriteTo.Console(outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss} [{Level:u3}] {Message:lj} {Properties:j}{NewLine}{Exception}")
    .WriteTo.File(
        path: "logs/wendoverhoa-.log",
        rollingInterval: RollingInterval.Day,
        outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss} [{Level:u3}] {Message:lj} {Properties:j}{NewLine}{Exception}")
    .CreateLogger();

// Log with context
_logger.LogError(
    exception,
    "Error processing payment for property {PropertyId}. Amount: {Amount}, User: {UserId}",
    propertyId,
    amount,
    userId);
```

### Log Enrichment

Logs are enriched with contextual information:

```csharp
// Add correlation ID to logs
app.Use(async (context, next) =>
{
    var correlationId = context.Request.Headers["X-Correlation-ID"].FirstOrDefault() ?? Guid.NewGuid().ToString();
    context.Response.Headers["X-Correlation-ID"] = correlationId;
    
    using (LogContext.PushProperty("CorrelationId", correlationId))
    using (LogContext.PushProperty("UserName", context.User.Identity?.Name ?? "Anonymous"))
    using (LogContext.PushProperty("IPAddress", context.Connection.RemoteIpAddress))
    {
        await next();
    }
});
```

## Security Considerations

### Preventing Information Disclosure

Sensitive information is never exposed in error messages:

```csharp
// Production error response
{
  "success": false,
  "error": {
    "code": "INTERNAL_SERVER_ERROR",
    "message": "An unexpected error occurred. Please try again later.",
    "traceId": "00-1234567890abcdef1234567890abcdef-1234567890abcdef-00"
  }
}

// Development error response (additional details)
{
  "success": false,
  "error": {
    "code": "INTERNAL_SERVER_ERROR",
    "message": "An unexpected error occurred. Please try again later.",
    "details": {
      "exception": "System.NullReferenceException: Object reference not set to an instance of an object.\n   at WendoverHOA.Application.DuesTracking.Commands.ProcessPaymentCommandHandler.Handle(ProcessPaymentCommand request, CancellationToken cancellationToken) in /src/WendoverHOA.Application/DuesTracking/Commands/ProcessPaymentCommand.cs:line 42"
    },
    "traceId": "00-1234567890abcdef1234567890abcdef-1234567890abcdef-00"
  }
}
```

### Error Sanitization

Error messages are sanitized to remove sensitive information:

```csharp
public static string SanitizeException(Exception exception)
{
    var sanitized = exception.ToString();
    
    // Remove connection strings
    sanitized = Regex.Replace(sanitized, @"(connection string|connectionstring)=([^;])*", "connection string=[REDACTED]", RegexOptions.IgnoreCase);
    
    // Remove API keys
    sanitized = Regex.Replace(sanitized, @"(api[_-]?key|apikey)=([^&\s]*)", "api_key=[REDACTED]", RegexOptions.IgnoreCase);
    
    // Remove passwords
    sanitized = Regex.Replace(sanitized, @"(password|pwd)=([^;])*", "password=[REDACTED]", RegexOptions.IgnoreCase);
    
    return sanitized;
}
```

## Error Monitoring and Alerting

### Application Insights Integration

```csharp
services.AddApplicationInsightsTelemetry(Configuration["ApplicationInsights:InstrumentationKey"]);

// Track exceptions
try
{
    // Operation that might fail
}
catch (Exception ex)
{
    _telemetryClient.TrackException(ex, new Dictionary<string, string>
    {
        ["PropertyId"] = propertyId.ToString(),
        ["UserId"] = userId.ToString()
    });
    throw;
}
```

### Error Alerting

Critical errors trigger alerts:

```csharp
// Configure error alerting
services.AddErrorAlerts(options =>
{
    options.CriticalErrorCodes = new[] { "PAYMENT_PROCESSING_ERROR", "DATABASE_CONNECTION_ERROR" };
    options.AlertEndpoint = Configuration["Alerts:Endpoint"];
    options.AlertThreshold = 5; // Alert after 5 occurrences
    options.AlertInterval = TimeSpan.FromMinutes(15);
});
```

## Error Recovery Strategies

### Retry Pattern

Transient errors are handled with retries:

```csharp
// Configure Polly retry policies
services.AddHttpClient<IPaymentGatewayClient, PaymentGatewayClient>()
    .AddTransientHttpErrorPolicy(builder => builder
        .WaitAndRetryAsync(
            3, 
            retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)),
            onRetry: (outcome, timespan, retryAttempt, context) =>
            {
                _logger.LogWarning(
                    "Retrying payment gateway request after {Delay}ms (Attempt {Attempt})",
                    timespan.TotalMilliseconds,
                    retryAttempt);
            }));
```

### Circuit Breaker Pattern

Persistent failures trigger circuit breaking:

```csharp
// Configure Polly circuit breaker
services.AddHttpClient<IPaymentGatewayClient, PaymentGatewayClient>()
    .AddTransientHttpErrorPolicy(builder => builder
        .CircuitBreakerAsync(
            5, // Number of exceptions before breaking circuit
            TimeSpan.FromMinutes(1), // Duration circuit stays open
            onBreak: (outcome, timespan) =>
            {
                _logger.LogError(
                    "Circuit breaker opened for payment gateway. Duration: {BreakDuration}",
                    timespan);
            },
            onReset: () =>
            {
                _logger.LogInformation("Circuit breaker reset for payment gateway.");
            }));
```

### Fallback Pattern

Critical operations have fallback mechanisms:

```csharp
// Configure Polly fallback
services.AddHttpClient<IPaymentGatewayClient, PaymentGatewayClient>()
    .AddTransientHttpErrorPolicy(builder => builder
        .FallbackAsync(
            fallbackAction: async (cancellationToken) => 
            {
                // Return fallback response
                return new PaymentResponse
                {
                    Success = false,
                    ErrorCode = "GATEWAY_UNAVAILABLE",
                    Message = "Payment gateway is temporarily unavailable. Your payment has been queued for processing."
                };
            },
            onFallback: (outcome, context) =>
            {
                _logger.LogWarning("Falling back to queued payment processing due to gateway unavailability.");
                
                // Queue payment for later processing
                _backgroundJobClient.Enqueue<IPaymentProcessor>(x => 
                    x.ProcessPaymentAsync(context["PaymentRequest"] as PaymentRequest));
                
                return Task.CompletedTask;
            }));
```

## Testing Error Handling

### Unit Testing

```csharp
[Fact]
public async Task Handle_ValidationFailure_ThrowsValidationException()
{
    // Arrange
    var command = new CreatePropertyCommand(); // Missing required fields
    var validator = new CreatePropertyCommandValidator();
    var validators = new List<IValidator<CreatePropertyCommand>> { validator };
    var behavior = new ValidationBehavior<CreatePropertyCommand, int>(validators);
    var next = new Mock<RequestHandlerDelegate<int>>();
    
    // Act & Assert
    await Assert.ThrowsAsync<ValidationException>(() => 
        behavior.Handle(command, next.Object, CancellationToken.None));
    
    next.Verify(n => n(), Times.Never);
}
```

### Integration Testing

```csharp
[Fact]
public async Task CreateProperty_InvalidData_ReturnsValidationError()
{
    // Arrange
    var client = _factory.CreateClient();
    var command = new CreatePropertyCommand(); // Missing required fields
    
    // Act
    var response = await client.PostAsJsonAsync("/api/v1/properties", command);
    var content = await response.Content.ReadAsStringAsync();
    var result = JsonSerializer.Deserialize<ApiResponse<int>>(content, _jsonOptions);
    
    // Assert
    Assert.Equal(HttpStatusCode.UnprocessableEntity, response.StatusCode);
    Assert.False(result.Success);
    Assert.NotNull(result.Error);
    Assert.Equal("VALIDATION_ERROR", result.Error.Code);
    Assert.NotNull(result.Error.Details);
}
```

## Error Documentation

### Error Codes Reference

A comprehensive error code reference is maintained:

| Code | Description | HTTP Status | Example |
|------|-------------|-------------|---------|
| `VALIDATION_ERROR` | Validation failed | 422 | Missing required field |
| `NOT_FOUND` | Resource not found | 404 | Property with ID 123 not found |
| `FORBIDDEN` | Access denied | 403 | User lacks required permission |
| `AUTHENTICATION_FAILED` | Authentication failed | 401 | Invalid credentials |
| `PAYMENT_PROCESSING_ERROR` | Payment processing failed | 400 | Invalid card number |
| `EXTERNAL_SERVICE_ERROR` | External service error | 502 | Payment gateway unavailable |
| `INTERNAL_SERVER_ERROR` | Unexpected error | 500 | Unhandled exception |

### API Error Documentation

Error responses are documented in Swagger:

```csharp
[HttpPost]
[ProducesResponseType(typeof(ApiResponse<int>), StatusCodes.Status201Created)]
[ProducesResponseType(typeof(ApiResponse<ErrorDetails>), StatusCodes.Status422UnprocessableEntity)]
[SwaggerOperation(
    Summary = "Create a new property",
    Description = "Creates a new property with the specified details.",
    OperationId = "Properties_Create")]
[SwaggerResponse(201, "Property created successfully", typeof(ApiResponse<int>))]
[SwaggerResponse(422, "Validation error", typeof(ApiResponse<ErrorDetails>))]
public async Task<ActionResult<ApiResponse<int>>> CreateProperty(CreatePropertyCommand command)
{
    var propertyId = await _mediator.Send(command);
    return CreatedAtAction(nameof(GetProperty), new { id = propertyId }, new ApiResponse<int> { Success = true, Data = propertyId });
}
```

## Conclusion

This error handling strategy provides a comprehensive approach to managing errors across all layers of the Wendover HOA application. By following these guidelines, the application will provide a consistent, secure, and user-friendly error handling experience while maintaining the principles of Clean Architecture.

The strategy ensures:

1. **Consistency**: All errors are handled in a consistent manner
2. **Security**: Sensitive information is never exposed in error messages
3. **Usability**: Users receive clear, actionable error messages
4. **Maintainability**: Errors are properly logged and monitored
5. **Resilience**: The application can recover from transient errors

By implementing this strategy, the Wendover HOA application will provide a robust and reliable user experience even when errors occur.
