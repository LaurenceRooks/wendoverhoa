# CQRS and MediatR Implementation

## Introduction

This document details the implementation of the Command Query Responsibility Segregation (CQRS) pattern and the MediatR library in the Wendover HOA application. CQRS separates read and write operations, allowing for optimized data access patterns, while MediatR provides a mediator implementation for in-process messaging.

## CQRS Pattern Overview

CQRS separates the application's operations into two categories:

1. **Commands**: Operations that change state (create, update, delete)
2. **Queries**: Operations that return data without changing state (read)

This separation provides several benefits:

- **Optimized Data Models**: Read and write models can be optimized independently
- **Scalability**: Read and write operations can be scaled independently
- **Security**: Fine-grained control over which operations can be performed
- **Performance**: Read operations can be optimized for specific use cases
- **Simplicity**: Each operation has a single responsibility

## MediatR Implementation

The Wendover HOA application uses the MediatR library to implement the mediator pattern, which decouples request senders from request handlers. This approach:

1. Reduces direct dependencies between components
2. Simplifies the implementation of cross-cutting concerns
3. Provides a consistent approach to handling commands and queries

### Project Structure

The CQRS implementation is organized within the Application Layer as follows:

```
WendoverHOA.Application/
├── Common/
│   ├── Behaviors/
│   │   ├── LoggingBehavior.cs
│   │   ├── ValidationBehavior.cs
│   │   ├── AuthorizationBehavior.cs
│   │   └── PerformanceBehavior.cs
│   ├── Exceptions/
│   │   ├── ValidationException.cs
│   │   ├── NotFoundException.cs
│   │   └── ForbiddenAccessException.cs
│   ├── Interfaces/
│   │   ├── ICurrentUserService.cs
│   │   ├── IDateTime.cs
│   │   └── IIdentityService.cs
│   └── Models/
│       ├── PaginatedList.cs
│       └── Result.cs
├── Feature1/
│   ├── Commands/
│   │   ├── CreateFeature1/
│   │   │   ├── CreateFeature1Command.cs
│   │   │   ├── CreateFeature1CommandValidator.cs
│   │   │   └── CreateFeature1CommandHandler.cs
│   │   ├── UpdateFeature1/
│   │   │   ├── UpdateFeature1Command.cs
│   │   │   ├── UpdateFeature1CommandValidator.cs
│   │   │   └── UpdateFeature1CommandHandler.cs
│   │   └── DeleteFeature1/
│   │       ├── DeleteFeature1Command.cs
│   │       └── DeleteFeature1CommandHandler.cs
│   └── Queries/
│       ├── GetFeature1List/
│       │   ├── GetFeature1ListQuery.cs
│       │   ├── GetFeature1ListQueryHandler.cs
│       │   └── Feature1ListDto.cs
│       └── GetFeature1Detail/
│           ├── GetFeature1DetailQuery.cs
│           ├── GetFeature1DetailQueryHandler.cs
│           └── Feature1DetailDto.cs
└── Feature2/
    ├── Commands/
    └── Queries/
```

### Command Implementation

Commands represent operations that change the state of the system. Each command consists of:

1. **Command Class**: Defines the command parameters
2. **Command Validator**: Validates the command parameters
3. **Command Handler**: Processes the command and returns a result

**Example Command Implementation**:

```csharp
// Command
public class CreateAnnouncementCommand : IRequest<int>
{
    public string Title { get; set; }
    public string Content { get; set; }
    public DateTime PublishDate { get; set; }
    public DateTime? ExpiryDate { get; set; }
    public ImportanceLevel ImportanceLevel { get; set; }
}

// Validator
public class CreateAnnouncementCommandValidator 
    : AbstractValidator<CreateAnnouncementCommand>
{
    public CreateAnnouncementCommandValidator()
    {
        RuleFor(x => x.Title)
            .NotEmpty().WithMessage("Title is required")
            .MaximumLength(200).WithMessage("Title must not exceed 200 characters");
            
        RuleFor(x => x.Content)
            .NotEmpty().WithMessage("Content is required");
            
        RuleFor(x => x.PublishDate)
            .NotEmpty().WithMessage("Publish date is required");
            
        RuleFor(x => x.ImportanceLevel)
            .IsInEnum().WithMessage("Importance level is invalid");
    }
}

// Handler
public class CreateAnnouncementCommandHandler 
    : IRequestHandler<CreateAnnouncementCommand, int>
{
    private readonly IAnnouncementRepository _announcementRepository;
    private readonly ICurrentUserService _currentUserService;
    private readonly IUnitOfWork _unitOfWork;

    public CreateAnnouncementCommandHandler(
        IAnnouncementRepository announcementRepository,
        ICurrentUserService currentUserService,
        IUnitOfWork unitOfWork)
    {
        _announcementRepository = announcementRepository;
        _currentUserService = currentUserService;
        _unitOfWork = unitOfWork;
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
        
        return announcement.Id;
    }
}
```

### Query Implementation

Queries represent operations that retrieve data without changing the state of the system. Each query consists of:

1. **Query Class**: Defines the query parameters
2. **Query Handler**: Processes the query and returns a result
3. **DTO Class**: Defines the data transfer object returned by the query

**Example Query Implementation**:

```csharp
// Query
public class GetAnnouncementsQuery : IRequest<List<AnnouncementDto>>
{
    public bool IncludeExpired { get; set; } = false;
}

// Handler
public class GetAnnouncementsQueryHandler 
    : IRequestHandler<GetAnnouncementsQuery, List<AnnouncementDto>>
{
    private readonly IAnnouncementRepository _announcementRepository;
    private readonly IMapper _mapper;

    public GetAnnouncementsQueryHandler(
        IAnnouncementRepository announcementRepository,
        IMapper mapper)
    {
        _announcementRepository = announcementRepository;
        _mapper = mapper;
    }

    public async Task<List<AnnouncementDto>> Handle(
        GetAnnouncementsQuery request, 
        CancellationToken cancellationToken)
    {
        var announcements = await _announcementRepository.GetAllAsync();
        
        if (!request.IncludeExpired)
        {
            announcements = announcements
                .Where(a => a.ExpiryDate == null || a.ExpiryDate >= DateTime.UtcNow)
                .ToList();
        }
        
        return _mapper.Map<List<AnnouncementDto>>(announcements);
    }
}

// DTO
public class AnnouncementDto
{
    public int Id { get; set; }
    public string Title { get; set; }
    public string Content { get; set; }
    public DateTime PublishDate { get; set; }
    public DateTime? ExpiryDate { get; set; }
    public string ImportanceLevel { get; set; }
    public string CreatedBy { get; set; }
    public DateTime CreatedAt { get; set; }
    public string LastModifiedBy { get; set; }
    public DateTime? LastModifiedAt { get; set; }
}
```

## Validation Pipeline

The Wendover HOA application implements a validation pipeline using FluentValidation and MediatR's behavior pipeline. This ensures that all commands and queries are validated before they are processed.

### Validation Behavior

The validation behavior intercepts all requests and validates them using FluentValidation:

```csharp
public class ValidationBehavior<TRequest, TResponse> 
    : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
{
    private readonly IEnumerable<IValidator<TRequest>> _validators;

    public ValidationBehavior(IEnumerable<IValidator<TRequest>> validators)
    {
        _validators = validators;
    }

    public async Task<TResponse> Handle(
        TRequest request, 
        RequestHandlerDelegate<TResponse> next, 
        CancellationToken cancellationToken)
    {
        if (_validators.Any())
        {
            var context = new ValidationContext<TRequest>(request);

            var validationResults = await Task.WhenAll(
                _validators.Select(v => 
                    v.ValidateAsync(context, cancellationToken)));

            var failures = validationResults
                .SelectMany(r => r.Errors)
                .Where(f => f != null)
                .ToList();

            if (failures.Count != 0)
                throw new ValidationException(failures);
        }

        return await next();
    }
}
```

### Registering Validators

Validators are registered with the dependency injection container during application startup:

```csharp
// In Application project's DependencyInjection.cs
public static IServiceCollection AddApplication(this IServiceCollection services)
{
    services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(Assembly.GetExecutingAssembly()));
    services.AddValidatorsFromAssembly(Assembly.GetExecutingAssembly());
    
    services.AddTransient(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));
    services.AddTransient(typeof(IPipelineBehavior<,>), typeof(LoggingBehavior<,>));
    services.AddTransient(typeof(IPipelineBehavior<,>), typeof(PerformanceBehavior<,>));
    services.AddTransient(typeof(IPipelineBehavior<,>), typeof(AuthorizationBehavior<,>));
    
    return services;
}
```

## Additional Behaviors

The Wendover HOA application implements several additional behaviors in the MediatR pipeline:

### Logging Behavior

The logging behavior logs information about requests and responses:

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

### Performance Behavior

The performance behavior measures the execution time of requests and logs warnings for slow requests:

```csharp
public class PerformanceBehavior<TRequest, TResponse> 
    : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
{
    private readonly Stopwatch _timer;
    private readonly ILogger<PerformanceBehavior<TRequest, TResponse>> _logger;
    private readonly ICurrentUserService _currentUserService;

    public PerformanceBehavior(
        ILogger<PerformanceBehavior<TRequest, TResponse>> logger,
        ICurrentUserService currentUserService)
    {
        _timer = new Stopwatch();
        _logger = logger;
        _currentUserService = currentUserService;
    }

    public async Task<TResponse> Handle(
        TRequest request, 
        RequestHandlerDelegate<TResponse> next, 
        CancellationToken cancellationToken)
    {
        _timer.Start();

        var response = await next();

        _timer.Stop();

        var elapsedMilliseconds = _timer.ElapsedMilliseconds;

        if (elapsedMilliseconds > 500)
        {
            var requestName = typeof(TRequest).Name;
            var userId = _currentUserService.UserId;

            _logger.LogWarning(
                "Long running request: {RequestName} ({ElapsedMilliseconds} milliseconds) for user {UserId}", 
                requestName, elapsedMilliseconds, userId);
        }

        return response;
    }
}
```

### Authorization Behavior

The authorization behavior checks if the current user is authorized to execute the request:

```csharp
public class AuthorizationBehavior<TRequest, TResponse> 
    : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
{
    private readonly ICurrentUserService _currentUserService;
    private readonly IIdentityService _identityService;

    public AuthorizationBehavior(
        ICurrentUserService currentUserService,
        IIdentityService identityService)
    {
        _currentUserService = currentUserService;
        _identityService = identityService;
    }

    public async Task<TResponse> Handle(
        TRequest request, 
        RequestHandlerDelegate<TResponse> next, 
        CancellationToken cancellationToken)
    {
        var authorizeAttributes = request.GetType().GetCustomAttributes<AuthorizeAttribute>();

        if (authorizeAttributes.Any())
        {
            // Must be authenticated user
            if (_currentUserService.UserId == 0)
            {
                throw new UnauthorizedAccessException();
            }

            // Role-based authorization
            var authorizeAttributesWithRoles = authorizeAttributes
                .Where(a => !string.IsNullOrWhiteSpace(a.Roles));

            if (authorizeAttributesWithRoles.Any())
            {
                foreach (var roles in authorizeAttributesWithRoles.Select(a => a.Roles.Split(',')))
                {
                    var authorized = false;
                    foreach (var role in roles)
                    {
                        var isInRole = await _identityService.IsInRoleAsync(
                            _currentUserService.UserId, role.Trim());
                            
                        if (isInRole)
                        {
                            authorized = true;
                            break;
                        }
                    }

                    // Must be a member of at least one role in roles
                    if (!authorized)
                    {
                        throw new ForbiddenAccessException();
                    }
                }
            }
        }

        // User is authorized / authorization not required
        return await next();
    }
}
```

## Benefits of CQRS and MediatR in Wendover HOA

1. **Separation of Concerns**: Read and write operations are separated
2. **Simplified API Controllers**: Controllers delegate to MediatR handlers
3. **Consistent Validation**: All commands and queries are validated
4. **Cross-Cutting Concerns**: Logging, performance monitoring, and authorization are applied consistently
5. **Testability**: Commands and queries can be tested in isolation
6. **Maintainability**: Each operation has a single responsibility
7. **Scalability**: Read and write operations can be scaled independently

## Conclusion

The CQRS pattern and MediatR library provide a solid foundation for the Wendover HOA application's business logic. By separating read and write operations and implementing a consistent approach to handling requests, the application achieves a high degree of maintainability, testability, and scalability.
