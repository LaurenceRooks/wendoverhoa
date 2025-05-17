# AI Implementation Guide

This document provides comprehensive instructions for AI models to implement the Wendover HOA application from beginning to end, following Clean Architecture principles, SOLID principles, and other project requirements.

## Project Overview

The Wendover HOA project is a modern web application for the Wendover Homeowners Association in Bedford, Texas. The application will help manage community information, resident communications, board activities, and financial operations.

### Key Requirements

1. **Technology Stack**:
   - Backend: .NET 9 Core (9.0.5), ASP.NET Core, Entity Framework Core 9, SQL Server 2022
   - Frontend: Bootstrap 5.3.6, Bootstrap Icons, Bootswatch Theme (Cosmo)
   - Authentication: .NET Core Identity with external providers (Microsoft, Google, Apple)

2. **Architecture**:
   - Clean Architecture with proper separation of concerns
   - CQRS pattern with MediatR for command/query separation
   - Role-based access control
   - RESTful APIs with proper documentation

3. **User Experience**:
   - Mobile-first, responsive design
   - Consistent UI following the UI Style Guide
   - Intuitive user flows following the UX Design Guidelines
   - Accessibility compliance (WCAG 2.1 AA)

4. **Quality Standards**:
   - Comprehensive test coverage
   - Secure coding practices
   - Performance optimization
   - Detailed documentation

## Implementation Approach

### Phase-by-Phase Development

Follow the Development Roadmap document for a detailed breakdown of tasks by phase. The high-level approach is:

1. **Phase 1: Core Infrastructure**
   - Authentication and user management
   - Site layout and navigation

2. **Phase 2: Community Features**
   - Announcements
   - Community calendar
   - Document repository

3. **Phase 3: Administrative Features**
   - Directory (properties and residents)
   - Board management
   - Meeting minutes
   - User feedback
   - Vendor suggestions

4. **Phase 4: Financial Features**
   - Dues tracking
   - Payment processing
   - Financial reporting
   - Expense tracking

### Layer-by-Layer Implementation

For each feature, implement in this order:

1. **Domain Layer**:
   - Define entities, value objects, and domain events
   - Implement domain logic and business rules
   - Create domain exceptions for business rule violations

2. **Application Layer**:
   - Define interfaces for repositories and services
   - Create DTOs for data transfer
   - Implement CQRS commands and queries with MediatR
   - Add validation using FluentValidation

3. **Infrastructure Layer**:
   - Implement repositories and services
   - Configure Entity Framework and create migrations
   - Implement external service integrations
   - Add logging, caching, and other cross-cutting concerns

4. **Web Layer**:
   - Create API controllers for backend services
   - Implement MVC controllers and views for frontend
   - Add client-side validation and JavaScript enhancements
   - Ensure responsive design and accessibility

## Step-by-Step Implementation Guide

### Initial Setup

1. **Create Solution Structure**:
   - Follow the Project Initialization Template document
   - Set up the solution with all required projects
   - Add initial project references and dependencies
   - Create basic configuration files

2. **Configure Development Environment**:
   - Set up the database connection
   - Configure logging with Serilog
   - Add authentication and authorization services
   - Configure Swagger for API documentation

3. **Implement Base Classes and Interfaces**:
   - Create base entity classes in the Domain layer
   - Define core interfaces in the Application layer
   - Implement the database context in the Infrastructure layer
   - Set up the base controller in the Web layer

### Phase 1 Implementation

#### Authentication and User Management

1. **Domain Layer**:
   ```csharp
   // Domain/Entities/ApplicationUser.cs
   public class ApplicationUser
   {
       public int Id { get; set; }
       public string Email { get; set; }
       public string FirstName { get; set; }
       public string LastName { get; set; }
       // Additional properties
   }
   ```

2. **Application Layer**:
   ```csharp
   // Application/Features/Authentication/Commands/RegisterUserCommand.cs
   public class RegisterUserCommand : IRequest<int>
   {
       public string Email { get; set; }
       public string Password { get; set; }
       public string FirstName { get; set; }
       public string LastName { get; set; }
   }
   
   public class RegisterUserCommandHandler : IRequestHandler<RegisterUserCommand, int>
   {
       private readonly IIdentityService _identityService;
       
       public RegisterUserCommandHandler(IIdentityService identityService)
       {
           _identityService = identityService;
       }
       
       public async Task<int> Handle(RegisterUserCommand request, CancellationToken cancellationToken)
       {
           // Implementation
       }
   }
   ```

3. **Infrastructure Layer**:
   ```csharp
   // Infrastructure/Identity/IdentityService.cs
   public class IdentityService : IIdentityService
   {
       private readonly UserManager<ApplicationUser> _userManager;
       private readonly RoleManager<ApplicationRole> _roleManager;
       
       public IdentityService(UserManager<ApplicationUser> userManager, RoleManager<ApplicationRole> roleManager)
       {
           _userManager = userManager;
           _roleManager = roleManager;
       }
       
       // Implementation of IIdentityService methods
   }
   ```

4. **Web Layer**:
   ```csharp
   // Web/Controllers/AccountController.cs
   [Route("api/[controller]")]
   [ApiController]
   public class AccountController : ControllerBase
   {
       private readonly IMediator _mediator;
       
       public AccountController(IMediator mediator)
       {
           _mediator = mediator;
       }
       
       [HttpPost("register")]
       public async Task<ActionResult<ApiResponse<int>>> Register(RegisterUserCommand command)
       {
           var userId = await _mediator.Send(command);
           return Ok(new ApiResponse<int> { Success = true, Data = userId });
       }
       
       // Additional endpoints
   }
   ```

#### Site Layout and Navigation

1. **Web Layer**:
   ```html
   <!-- Web/Views/Shared/_Layout.cshtml -->
   <!DOCTYPE html>
   <html lang="en">
   <head>
       <meta charset="utf-8" />
       <meta name="viewport" content="width=device-width, initial-scale=1.0" />
       <title>@ViewData["Title"] - Wendover HOA</title>
       <link rel="stylesheet" href="~/lib/bootstrap/dist/css/bootstrap.min.css" />
       <link rel="stylesheet" href="~/css/site.css" asp-append-version="true" />
   </head>
   <body>
       <header>
           <nav class="navbar navbar-expand-sm navbar-toggleable-sm navbar-dark bg-primary border-bottom box-shadow mb-3">
               <!-- Navigation implementation -->
           </nav>
       </header>
       <div class="container">
           <main role="main" class="pb-3">
               @RenderBody()
           </main>
       </div>
       <footer class="border-top footer text-muted">
           <!-- Footer implementation -->
       </footer>
       <script src="~/lib/jquery/dist/jquery.min.js"></script>
       <script src="~/lib/bootstrap/dist/js/bootstrap.bundle.min.js"></script>
       <script src="~/js/site.js" asp-append-version="true"></script>
       @await RenderSectionAsync("Scripts", required: false)
   </body>
   </html>
   ```

### Phase 2 Implementation

#### Announcements

1. **Domain Layer**:
   ```csharp
   // Domain/Entities/Announcement.cs
   public class Announcement : BaseEntity
   {
       public string Title { get; set; }
       public string Content { get; set; }
       public DateTime PublishDate { get; set; }
       public DateTime? ExpiryDate { get; set; }
       public bool IsPublished { get; set; }
       public int CreatedById { get; set; }
       
       // Domain logic methods
       public void Publish()
       {
           if (string.IsNullOrEmpty(Title) || string.IsNullOrEmpty(Content))
           {
               throw new DomainException("Cannot publish announcement with empty title or content");
           }
           
           IsPublished = true;
           PublishDate = DateTime.UtcNow;
       }
       
       public void Unpublish()
       {
           IsPublished = false;
       }
   }
   ```

2. **Application Layer**:
   ```csharp
   // Application/Features/Announcements/Commands/CreateAnnouncementCommand.cs
   public class CreateAnnouncementCommand : IRequest<int>
   {
       public string Title { get; set; }
       public string Content { get; set; }
       public DateTime? ExpiryDate { get; set; }
       public bool Publish { get; set; }
   }
   
   public class CreateAnnouncementCommandHandler : IRequestHandler<CreateAnnouncementCommand, int>
   {
       private readonly IAnnouncementRepository _announcementRepository;
       private readonly IUnitOfWork _unitOfWork;
       private readonly ICurrentUserService _currentUserService;
       
       // Constructor and Handle method implementation
   }
   ```

3. **Infrastructure Layer**:
   ```csharp
   // Infrastructure/Persistence/Repositories/AnnouncementRepository.cs
   public class AnnouncementRepository : IAnnouncementRepository
   {
       private readonly ApplicationDbContext _dbContext;
       
       public AnnouncementRepository(ApplicationDbContext dbContext)
       {
           _dbContext = dbContext;
       }
       
       // Implementation of repository methods
   }
   ```

4. **Web Layer**:
   ```csharp
   // Web/Controllers/AnnouncementsController.cs
   [Route("api/[controller]")]
   [ApiController]
   public class AnnouncementsController : ControllerBase
   {
       private readonly IMediator _mediator;
       
       public AnnouncementsController(IMediator mediator)
       {
           _mediator = mediator;
       }
       
       [HttpGet]
       public async Task<ActionResult<ApiResponse<List<AnnouncementDto>>>> GetAnnouncements()
       {
           var announcements = await _mediator.Send(new GetAnnouncementsQuery());
           return Ok(new ApiResponse<List<AnnouncementDto>> { Success = true, Data = announcements });
       }
       
       // Additional endpoints
   }
   ```

### Phase 3 Implementation

#### Directory (Properties and Residents)

1. **Domain Layer**:
   ```csharp
   // Domain/Entities/Property.cs
   public class Property : BaseEntity
   {
       public string Address { get; set; }
       public string City { get; set; }
       public string State { get; set; }
       public string ZipCode { get; set; }
       public PropertyStatus Status { get; set; }
       public int? ResidentId { get; set; }
       public Resident Resident { get; set; }
       
       // Domain logic methods
       public void AssignResident(Resident resident)
       {
           if (Status != PropertyStatus.Vacant)
           {
               throw new DomainException("Cannot assign resident to a non-vacant property");
           }
           
           Resident = resident;
           ResidentId = resident.Id;
           Status = PropertyStatus.Occupied;
       }
       
       public void RemoveResident()
       {
           if (Status != PropertyStatus.Occupied)
           {
               throw new DomainException("Cannot remove resident from a vacant property");
           }
           
           Resident = null;
           ResidentId = null;
           Status = PropertyStatus.Vacant;
       }
   }
   ```

2. **Application Layer**:
   ```csharp
   // Application/Features/Properties/Commands/AssignResidentCommand.cs
   public class AssignResidentCommand : IRequest<Unit>
   {
       public int PropertyId { get; set; }
       public int ResidentId { get; set; }
   }
   
   public class AssignResidentCommandHandler : IRequestHandler<AssignResidentCommand, Unit>
   {
       private readonly IPropertyRepository _propertyRepository;
       private readonly IResidentRepository _residentRepository;
       private readonly IUnitOfWork _unitOfWork;
       
       // Constructor and Handle method implementation
   }
   ```

3. **Infrastructure Layer**:
   ```csharp
   // Infrastructure/Persistence/Repositories/PropertyRepository.cs
   public class PropertyRepository : IPropertyRepository
   {
       private readonly ApplicationDbContext _dbContext;
       
       public PropertyRepository(ApplicationDbContext dbContext)
       {
           _dbContext = dbContext;
       }
       
       // Implementation of repository methods
   }
   ```

4. **Web Layer**:
   ```csharp
   // Web/Controllers/PropertiesController.cs
   [Route("api/[controller]")]
   [ApiController]
   public class PropertiesController : ControllerBase
   {
       private readonly IMediator _mediator;
       
       public PropertiesController(IMediator mediator)
       {
           _mediator = mediator;
       }
       
       // API endpoints
   }
   ```

### Phase 4 Implementation

#### Dues Tracking

1. **Domain Layer**:
   ```csharp
   // Domain/Entities/DuesAccount.cs
   public class DuesAccount : BaseEntity
   {
       public int PropertyId { get; set; }
       public Property Property { get; set; }
       public decimal Balance { get; set; }
       public DateTime LastPaymentDate { get; set; }
       public List<Transaction> Transactions { get; set; } = new List<Transaction>();
       
       // Domain logic methods
       public void AddTransaction(Transaction transaction)
       {
           Transactions.Add(transaction);
           
           if (transaction.Type == TransactionType.Payment)
           {
               Balance -= transaction.Amount;
               LastPaymentDate = transaction.Date;
           }
           else if (transaction.Type == TransactionType.Charge)
           {
               Balance += transaction.Amount;
           }
       }
       
       public bool IsOverdue(int gracePeriodDays = 30)
       {
           var dueDate = new DateTime(DateTime.UtcNow.Year, DateTime.UtcNow.Month, 1);
           return Balance > 0 && LastPaymentDate < dueDate.AddDays(-gracePeriodDays);
       }
   }
   ```

2. **Application Layer**:
   ```csharp
   // Application/Features/Dues/Commands/ProcessPaymentCommand.cs
   public class ProcessPaymentCommand : IRequest<int>
   {
       public int PropertyId { get; set; }
       public decimal Amount { get; set; }
       public string PaymentMethod { get; set; }
       public string ReferenceNumber { get; set; }
   }
   
   public class ProcessPaymentCommandHandler : IRequestHandler<ProcessPaymentCommand, int>
   {
       private readonly IDuesRepository _duesRepository;
       private readonly IPropertyRepository _propertyRepository;
       private readonly IUnitOfWork _unitOfWork;
       
       // Constructor and Handle method implementation
   }
   ```

3. **Infrastructure Layer**:
   ```csharp
   // Infrastructure/Persistence/Repositories/DuesRepository.cs
   public class DuesRepository : IDuesRepository
   {
       private readonly ApplicationDbContext _dbContext;
       
       public DuesRepository(ApplicationDbContext dbContext)
       {
           _dbContext = dbContext;
       }
       
       // Implementation of repository methods
   }
   ```

4. **Web Layer**:
   ```csharp
   // Web/Controllers/DuesController.cs
   [Route("api/[controller]")]
   [ApiController]
   public class DuesController : ControllerBase
   {
       private readonly IMediator _mediator;
       
       public DuesController(IMediator mediator)
       {
           _mediator = mediator;
       }
       
       // API endpoints
   }
   ```

## Implementation Best Practices

### Clean Architecture

1. **Domain Layer**:
   - Keep the domain layer free of dependencies on other layers
   - Implement domain logic within entities and value objects
   - Use domain events for cross-entity coordination
   - Create domain exceptions for business rule violations

2. **Application Layer**:
   - Define interfaces for infrastructure concerns
   - Implement CQRS with MediatR for command/query separation
   - Use DTOs for data transfer between layers
   - Add validation using FluentValidation

3. **Infrastructure Layer**:
   - Implement interfaces defined in the application layer
   - Keep infrastructure concerns separate from domain logic
   - Use repository pattern for data access
   - Implement cross-cutting concerns like logging and caching

4. **Web Layer**:
   - Keep controllers thin by delegating to application services
   - Use view models for presentation concerns
   - Implement proper error handling and validation
   - Follow the UI Style Guide for consistent design

### SOLID Principles

1. **Single Responsibility Principle**:
   - Each class should have only one reason to change
   - Keep classes focused on a single concern
   - Extract complex logic into separate classes

2. **Open/Closed Principle**:
   - Classes should be open for extension but closed for modification
   - Use interfaces and abstract classes for extensibility
   - Implement strategy pattern for varying behaviors

3. **Liskov Substitution Principle**:
   - Derived classes must be substitutable for their base classes
   - Ensure inheritance hierarchies are properly designed
   - Use composition over inheritance when appropriate

4. **Interface Segregation Principle**:
   - Clients should not depend on interfaces they don't use
   - Create focused interfaces with cohesive methods
   - Split large interfaces into smaller, more specific ones

5. **Dependency Inversion Principle**:
   - High-level modules should not depend on low-level modules
   - Both should depend on abstractions
   - Use dependency injection for loose coupling

### CQRS with MediatR

1. **Commands**:
   - Represent operations that change state
   - Should be named with verbs in imperative form
   - Should return minimal data (typically just an ID)
   - Should be validated before execution

2. **Queries**:
   - Represent operations that read state
   - Should be named with nouns or verbs in interrogative form
   - Should return DTOs with exactly the data needed
   - Can be optimized for read performance

3. **Handlers**:
   - Should be focused on a single command or query
   - Should delegate to domain entities for business logic
   - Should use repositories for data access
   - Should handle transactions appropriately

4. **Behaviors**:
   - Implement cross-cutting concerns like validation and logging
   - Should be registered in the dependency injection container
   - Should be composable and maintainable
   - Should not contain business logic

### Testing

1. **Unit Testing**:
   - Test each component in isolation
   - Mock dependencies using Moq
   - Follow the Arrange-Act-Assert pattern
   - Use descriptive test names

2. **Integration Testing**:
   - Test interactions between components
   - Use in-memory database for repository tests
   - Test API endpoints with WebApplicationFactory
   - Verify database state after operations

3. **UI Testing**:
   - Test components with bUnit
   - Test end-to-end flows with Playwright
   - Verify accessibility with axe-core
   - Test responsive design at different breakpoints

## Common Pitfalls to Avoid

1. **Anemic Domain Model**:
   - Don't create entities with only properties and no behavior
   - Implement domain logic within entities and value objects
   - Use domain services for logic that spans multiple entities
   - Ensure rich domain model with proper encapsulation

2. **Repository Anti-patterns**:
   - Don't expose IQueryable from repositories
   - Don't put business logic in repositories
   - Don't create a repository for each entity unnecessarily
   - Don't bypass the repository pattern for direct data access

3. **CQRS Anti-patterns**:
   - Don't mix command and query responsibilities
   - Don't return domain entities from queries
   - Don't put domain logic in handlers
   - Don't create overly complex commands or queries

4. **Clean Architecture Violations**:
   - Don't reference outer layers from inner layers
   - Don't expose infrastructure concerns to the domain
   - Don't bypass the application layer from the web layer
   - Don't put presentation logic in the application layer

5. **Performance Issues**:
   - Don't load unnecessary data
   - Don't use lazy loading in web applications
   - Don't perform multiple database roundtrips
   - Don't neglect caching for frequently accessed data

## Conclusion

This implementation guide provides a comprehensive approach to implementing the Wendover HOA application following Clean Architecture principles, SOLID principles, and other project requirements. By following this guide, you will create a maintainable, testable, and scalable application that meets the needs of the Wendover Homeowners Association.

Remember to:
- Follow the phase-by-phase approach outlined in the Development Roadmap
- Implement each feature layer by layer, starting with the domain
- Adhere to Clean Architecture and SOLID principles
- Write comprehensive tests for all components
- Follow the UI Style Guide and UX Design Guidelines
- Document your code and implementation decisions

With this approach, you will successfully implement the Wendover HOA application from beginning to end, delivering a high-quality product that meets all requirements and provides value to its users.
