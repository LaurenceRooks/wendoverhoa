# Clean Architecture Overview

## Introduction

This document outlines the Clean Architecture implementation for the Wendover HOA web application. The application follows the principles of Clean Architecture as defined by Robert C. Martin, ensuring separation of concerns, testability, and maintainability.

## Architecture Layers

The Wendover HOA application is structured into four distinct layers, with dependencies flowing inward:

![Clean Architecture Diagram](../assets/clean-architecture-diagram.png)

### 1. Domain Layer (Core)

The Domain Layer is the innermost layer and contains:

- **Domain Entities**: Core business objects that represent the fundamental concepts of the application (e.g., Resident, Property, Announcement)
- **Domain Exceptions**: Custom exceptions specific to domain rules
- **Value Objects**: Immutable objects that represent concepts with no identity
- **Enumerations**: Strongly-typed enums that represent domain concepts
- **Domain Events**: Events that occur within the domain
- **Domain Services**: Services that contain domain logic that doesn't naturally fit within entities

**Key Characteristics**:
- Has no dependencies on other layers or external frameworks
- Contains pure C# code with no infrastructure concerns
- Defines interfaces that will be implemented by outer layers
- Encapsulates all business rules and validation logic

**Example Domain Entity**:
```csharp
namespace WendoverHOA.Domain.Entities
{
    public class Announcement
    {
        public int Id { get; private set; }
        public string Title { get; private set; }
        public string Content { get; private set; }
        public DateTime PublishDate { get; private set; }
        public DateTime? ExpiryDate { get; private set; }
        public ImportanceLevel ImportanceLevel { get; private set; }
        public int CreatedById { get; private set; }
        public DateTime CreatedAt { get; private set; }
        public int? LastModifiedById { get; private set; }
        public DateTime? LastModifiedAt { get; private set; }

        // Constructor ensures entity is always in valid state
        public Announcement(string title, string content, DateTime publishDate, 
                           DateTime? expiryDate, ImportanceLevel importanceLevel, 
                           int createdById)
        {
            if (string.IsNullOrWhiteSpace(title))
                throw new DomainException("Announcement title cannot be empty");
                
            if (string.IsNullOrWhiteSpace(content))
                throw new DomainException("Announcement content cannot be empty");
                
            Title = title;
            Content = content;
            PublishDate = publishDate;
            ExpiryDate = expiryDate;
            ImportanceLevel = importanceLevel;
            CreatedById = createdById;
            CreatedAt = DateTime.UtcNow;
        }

        // Domain methods encapsulate business rules
        public void Update(string title, string content, DateTime publishDate, 
                          DateTime? expiryDate, ImportanceLevel importanceLevel, 
                          int modifiedById)
        {
            if (string.IsNullOrWhiteSpace(title))
                throw new DomainException("Announcement title cannot be empty");
                
            if (string.IsNullOrWhiteSpace(content))
                throw new DomainException("Announcement content cannot be empty");
                
            Title = title;
            Content = content;
            PublishDate = publishDate;
            ExpiryDate = expiryDate;
            ImportanceLevel = importanceLevel;
            LastModifiedById = modifiedById;
            LastModifiedAt = DateTime.UtcNow;
        }
    }
}
```

### 2. Application Layer

The Application Layer contains:

- **Commands and Queries**: CQRS implementation with MediatR
- **Command/Query Handlers**: Process commands and queries
- **DTOs (Data Transfer Objects)**: Objects used to transfer data between layers
- **Interfaces for Infrastructure Services**: Defines contracts for services implemented in outer layers
- **Validation Logic**: Validates commands and queries
- **Mapping Profiles**: Maps between domain entities and DTOs

**Key Characteristics**:
- Depends only on the Domain Layer
- Orchestrates the flow of data to and from domain entities
- Contains no business rules (these belong in the Domain Layer)
- Defines interfaces for infrastructure concerns (e.g., IRepository, IEmailService)

**Example Application Service**:
```csharp
namespace WendoverHOA.Application.Announcements.Commands.CreateAnnouncement
{
    public class CreateAnnouncementCommand : IRequest<int>
    {
        public string Title { get; set; }
        public string Content { get; set; }
        public DateTime PublishDate { get; set; }
        public DateTime? ExpiryDate { get; set; }
        public ImportanceLevel ImportanceLevel { get; set; }
    }

    public class CreateAnnouncementCommandValidator : AbstractValidator<CreateAnnouncementCommand>
    {
        public CreateAnnouncementCommandValidator()
        {
            RuleFor(x => x.Title).NotEmpty().MaximumLength(200);
            RuleFor(x => x.Content).NotEmpty();
            RuleFor(x => x.PublishDate).NotEmpty();
            RuleFor(x => x.ImportanceLevel).IsInEnum();
        }
    }

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
}
```

### 3. Infrastructure Layer

The Infrastructure Layer contains:

- **Repository Implementations**: Implements repository interfaces defined in the Application Layer
- **Database Context**: Entity Framework Core DbContext
- **External Service Implementations**: Implementations of service interfaces defined in the Application Layer
- **Authentication/Authorization**: Identity implementation
- **Logging**: Logging implementation
- **File Storage**: File storage implementation
- **Email Service**: Email service implementation
- **Migrations**: Database migrations

**Key Characteristics**:
- Implements interfaces defined in the Application Layer
- Contains all external dependencies and framework-specific code
- Handles all data access and external service communication
- Configures dependency injection for infrastructure services

**Example Repository Implementation**:
```csharp
namespace WendoverHOA.Infrastructure.Persistence.Repositories
{
    public class AnnouncementRepository : IAnnouncementRepository
    {
        private readonly ApplicationDbContext _context;

        public AnnouncementRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<Announcement> GetByIdAsync(int id)
        {
            return await _context.Announcements.FindAsync(id);
        }

        public async Task<IEnumerable<Announcement>> GetAllAsync()
        {
            return await _context.Announcements
                .OrderByDescending(a => a.PublishDate)
                .ToListAsync();
        }

        public void Add(Announcement announcement)
        {
            _context.Announcements.Add(announcement);
        }

        public void Update(Announcement announcement)
        {
            _context.Entry(announcement).State = EntityState.Modified;
        }

        public void Delete(Announcement announcement)
        {
            _context.Announcements.Remove(announcement);
        }
    }
}
```

### 4. Presentation Layer

The Presentation Layer contains:

- **Controllers**: API controllers that handle HTTP requests
- **Views**: Blazor components and MVC views
- **View Models**: Models specific to views
- **Filters**: Action filters for cross-cutting concerns
- **Middleware**: Custom middleware for request processing

**Key Characteristics**:
- Depends on the Application Layer
- Handles HTTP requests and responses
- Maps between DTOs and ViewModels
- Manages user interface concerns
- Implements user authentication and authorization checks

**Example API Controller**:
```csharp
namespace WendoverHOA.Web.Controllers.API
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class AnnouncementsController : ControllerBase
    {
        private readonly IMediator _mediator;

        public AnnouncementsController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpGet]
        public async Task<ActionResult<List<AnnouncementDto>>> GetAnnouncements()
        {
            var query = new GetAnnouncementsQuery();
            var result = await _mediator.Send(query);
            return Ok(result);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<AnnouncementDto>> GetAnnouncement(int id)
        {
            var query = new GetAnnouncementByIdQuery { Id = id };
            var result = await _mediator.Send(query);
            
            if (result == null)
                return NotFound();
                
            return Ok(result);
        }

        [HttpPost]
        [Authorize(Roles = "Administrator,BoardMember")]
        public async Task<ActionResult<int>> CreateAnnouncement(CreateAnnouncementCommand command)
        {
            var result = await _mediator.Send(command);
            return CreatedAtAction(nameof(GetAnnouncement), new { id = result }, result);
        }

        [HttpPut("{id}")]
        [Authorize(Roles = "Administrator,BoardMember")]
        public async Task<ActionResult> UpdateAnnouncement(int id, UpdateAnnouncementCommand command)
        {
            if (id != command.Id)
                return BadRequest();
                
            await _mediator.Send(command);
            return NoContent();
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "Administrator,BoardMember")]
        public async Task<ActionResult> DeleteAnnouncement(int id)
        {
            var command = new DeleteAnnouncementCommand { Id = id };
            await _mediator.Send(command);
            return NoContent();
        }
    }
}
```

## Dependency Flow

In Clean Architecture, dependencies flow inward. This means that:

1. **Domain Layer** has no dependencies on other layers
2. **Application Layer** depends only on the Domain Layer
3. **Infrastructure Layer** depends on the Application and Domain Layers
4. **Presentation Layer** depends on the Application Layer (and indirectly on the Domain Layer)

This dependency rule is enforced through:

- **Project References**: Each project only references projects in inner layers
- **Dependency Injection**: Interfaces defined in inner layers are implemented in outer layers
- **Dependency Inversion Principle**: High-level modules do not depend on low-level modules; both depend on abstractions

## Inversion of Control

The Wendover HOA application uses dependency injection to implement the Inversion of Control principle:

1. **Interface Definition**: Interfaces are defined in the Application Layer
2. **Implementation**: Implementations are provided in the Infrastructure Layer
3. **Registration**: Services are registered in the DI container during application startup
4. **Resolution**: Dependencies are resolved and injected by the DI container

**Example DI Configuration**:
```csharp
// In Program.cs or Startup.cs
public void ConfigureServices(IServiceCollection services)
{
    // Register application services
    services.AddApplication();
    
    // Register infrastructure services
    services.AddInfrastructure(Configuration);
    
    // Register presentation services
    services.AddControllers();
    services.AddRazorPages();
    services.AddServerSideBlazor();
}

// In Infrastructure project
public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services, 
        IConfiguration configuration)
    {
        services.AddDbContext<ApplicationDbContext>(options =>
            options.UseSqlServer(
                configuration.GetConnectionString("DefaultConnection")));
                
        services.AddScoped<IUnitOfWork, UnitOfWork>();
        services.AddScoped<IAnnouncementRepository, AnnouncementRepository>();
        services.AddScoped<IEmailService, EmailService>();
        services.AddScoped<IFileStorageService, FileStorageService>();
        
        return services;
    }
}
```

## Benefits of Clean Architecture in Wendover HOA

1. **Testability**: Each layer can be tested in isolation
2. **Maintainability**: Changes in one layer don't affect other layers
3. **Flexibility**: Infrastructure implementations can be changed without affecting business logic
4. **Independence from Frameworks**: The core business logic is independent of frameworks
5. **Separation of Concerns**: Each layer has a specific responsibility
6. **Domain-Centric**: The architecture emphasizes the domain model and business rules

## Conclusion

The Clean Architecture approach provides a solid foundation for the Wendover HOA application, ensuring that it remains maintainable, testable, and adaptable to changing requirements over time. By strictly adhering to the dependency rule and separation of concerns, the application achieves a high degree of modularity and flexibility.
