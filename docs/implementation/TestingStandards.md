# Testing Standards

This document outlines the testing standards for the Wendover HOA application, providing comprehensive guidelines for unit testing, integration testing, and UI testing across all layers of the application in alignment with Clean Architecture principles.

## Testing Principles

The Wendover HOA application follows these core testing principles:

1. **Test-Driven Development**: Write tests before implementing features when possible
2. **Comprehensive Coverage**: Test all layers of the application
3. **Isolation**: Unit tests should be isolated from external dependencies
4. **Readability**: Tests should be clear and serve as documentation
5. **Maintainability**: Tests should be easy to maintain as the application evolves
6. **Automation**: All tests should be automated and run in the CI/CD pipeline
7. **Fast Feedback**: Tests should run quickly to provide rapid feedback

## Testing Pyramid

The application follows the testing pyramid approach:

```
    /\
   /  \
  /    \
 / E2E  \
/--------\
/  UI     \
/----------\
/ Integration\
/------------\
/    Unit     \
----------------
```

- **Unit Tests**: Many small, focused tests for individual components
- **Integration Tests**: Tests that verify interactions between components
- **UI Tests**: Tests that verify the user interface behaves correctly
- **End-to-End Tests**: Tests that verify complete user workflows

## Unit Testing

### Unit Test Structure

All unit tests follow the Arrange-Act-Assert (AAA) pattern:

```csharp
[Fact]
public void MethodName_Scenario_ExpectedBehavior()
{
    // Arrange
    var sut = new SystemUnderTest();
    var input = new TestInput();
    
    // Act
    var result = sut.MethodToTest(input);
    
    // Assert
    Assert.Equal(expectedValue, result);
}
```

### Naming Convention

Unit tests follow this naming convention:

```
[MethodName]_[Scenario]_[ExpectedBehavior]
```

Examples:
- `GetProperty_WithValidId_ReturnsProperty`
- `CreateResident_WithInvalidData_ThrowsValidationException`
- `CalculateDues_WithPastDueAccount_IncludesLateFees`

### Unit Test Templates

#### Domain Layer Unit Test

```csharp
using System;
using WendoverHOA.Domain.Entities;
using WendoverHOA.Domain.Exceptions;
using Xunit;

namespace WendoverHOA.Domain.Tests.Entities
{
    public class PropertyTests
    {
        [Fact]
        public void AssignResident_WithVacantProperty_SetsResidentAndChangesStatus()
        {
            // Arrange
            var property = new Property { Status = PropertyStatus.Vacant };
            var resident = new Resident { Id = 1, Name = "John Doe" };
            
            // Act
            property.AssignResident(resident);
            
            // Assert
            Assert.Equal(resident.Id, property.ResidentId);
            Assert.Equal(PropertyStatus.Occupied, property.Status);
        }
        
        [Fact]
        public void AssignResident_WithOccupiedProperty_ThrowsDomainException()
        {
            // Arrange
            var property = new Property 
            { 
                Status = PropertyStatus.Occupied,
                ResidentId = 2
            };
            var resident = new Resident { Id = 1, Name = "John Doe" };
            
            // Act & Assert
            var exception = Assert.Throws<DomainException>(() => 
                property.AssignResident(resident));
                
            Assert.Contains("Cannot assign resident to a non-vacant property", exception.Message);
        }
    }
}
```

#### Application Layer Unit Test

```csharp
using System;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using FluentAssertions;
using MediatR;
using Moq;
using WendoverHOA.Application.Common.Exceptions;
using WendoverHOA.Application.Common.Interfaces;
using WendoverHOA.Application.Features.Properties.Commands;
using WendoverHOA.Domain.Entities;
using Xunit;

namespace WendoverHOA.Application.Tests.Features.Properties.Commands
{
    public class CreatePropertyCommandHandlerTests
    {
        private readonly Mock<IPropertyRepository> _mockRepository;
        private readonly Mock<IUnitOfWork> _mockUnitOfWork;
        private readonly Mock<IMapper> _mockMapper;
        
        public CreatePropertyCommandHandlerTests()
        {
            _mockRepository = new Mock<IPropertyRepository>();
            _mockUnitOfWork = new Mock<IUnitOfWork>();
            _mockMapper = new Mock<IMapper>();
        }
        
        [Fact]
        public async Task Handle_WithValidData_ReturnsPropertyId()
        {
            // Arrange
            var command = new CreatePropertyCommand
            {
                Address = "123 Main St",
                City = "Bedford",
                State = "TX",
                ZipCode = "76021"
            };
            
            var property = new Property
            {
                Id = 1,
                Address = command.Address,
                City = command.City,
                State = command.State,
                ZipCode = command.ZipCode,
                Status = PropertyStatus.Vacant
            };
            
            _mockRepository
                .Setup(r => r.AddAsync(It.IsAny<Property>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(property);
                
            var handler = new CreatePropertyCommandHandler(
                _mockRepository.Object,
                _mockUnitOfWork.Object,
                _mockMapper.Object);
                
            // Act
            var result = await handler.Handle(command, CancellationToken.None);
            
            // Assert
            result.Should().Be(1);
            _mockRepository.Verify(r => 
                r.AddAsync(It.IsAny<Property>(), It.IsAny<CancellationToken>()), 
                Times.Once);
            _mockUnitOfWork.Verify(u => 
                u.SaveChangesAsync(It.IsAny<CancellationToken>()), 
                Times.Once);
        }
        
        [Fact]
        public async Task Handle_WithDuplicateAddress_ThrowsValidationException()
        {
            // Arrange
            var command = new CreatePropertyCommand
            {
                Address = "123 Main St",
                City = "Bedford",
                State = "TX",
                ZipCode = "76021"
            };
            
            _mockRepository
                .Setup(r => r.AddAsync(It.IsAny<Property>(), It.IsAny<CancellationToken>()))
                .ThrowsAsync(new DbUpdateException("Duplicate key", new Exception()));
                
            var handler = new CreatePropertyCommandHandler(
                _mockRepository.Object,
                _mockUnitOfWork.Object,
                _mockMapper.Object);
                
            // Act & Assert
            await Assert.ThrowsAsync<ValidationException>(() => 
                handler.Handle(command, CancellationToken.None));
        }
    }
}
```

#### Infrastructure Layer Unit Test

```csharp
using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using WendoverHOA.Application.Common.Exceptions;
using WendoverHOA.Domain.Entities;
using WendoverHOA.Infrastructure.Persistence;
using WendoverHOA.Infrastructure.Persistence.Repositories;
using Xunit;

namespace WendoverHOA.Infrastructure.Tests.Persistence.Repositories
{
    public class PropertyRepositoryTests
    {
        private readonly DbContextOptions<ApplicationDbContext> _options;
        
        public PropertyRepositoryTests()
        {
            _options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;
        }
        
        [Fact]
        public async Task GetByIdAsync_WithExistingProperty_ReturnsProperty()
        {
            // Arrange
            using var context = new ApplicationDbContext(_options);
            var property = new Property
            {
                Id = 1,
                Address = "123 Main St",
                City = "Bedford",
                State = "TX",
                ZipCode = "76021",
                Status = PropertyStatus.Vacant
            };
            
            context.Properties.Add(property);
            await context.SaveChangesAsync();
            
            var repository = new PropertyRepository(context);
            
            // Act
            var result = await repository.GetByIdAsync(1, CancellationToken.None);
            
            // Assert
            Assert.NotNull(result);
            Assert.Equal("123 Main St", result.Address);
        }
        
        [Fact]
        public async Task GetByIdAsync_WithNonExistingProperty_ThrowsNotFoundException()
        {
            // Arrange
            using var context = new ApplicationDbContext(_options);
            var repository = new PropertyRepository(context);
            
            // Act & Assert
            await Assert.ThrowsAsync<NotFoundException>(() => 
                repository.GetByIdAsync(999, CancellationToken.None));
        }
    }
}
```

#### Web Layer Unit Test

```csharp
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Moq;
using WendoverHOA.Application.Features.Properties.Commands;
using WendoverHOA.Application.Features.Properties.Queries;
using WendoverHOA.Web.Controllers;
using Xunit;

namespace WendoverHOA.Web.Tests.Controllers
{
    public class PropertiesControllerTests
    {
        private readonly Mock<IMediator> _mockMediator;
        
        public PropertiesControllerTests()
        {
            _mockMediator = new Mock<IMediator>();
        }
        
        [Fact]
        public async Task GetProperty_WithValidId_ReturnsOkResult()
        {
            // Arrange
            var propertyDto = new PropertyDto { Id = 1, Address = "123 Main St" };
            
            _mockMediator
                .Setup(m => m.Send(It.IsAny<GetPropertyByIdQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(propertyDto);
                
            var controller = new PropertiesController(_mockMediator.Object);
            
            // Act
            var result = await controller.GetProperty(1);
            
            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var returnValue = Assert.IsType<ApiResponse<PropertyDto>>(okResult.Value);
            Assert.True(returnValue.Success);
            Assert.Equal(1, returnValue.Data.Id);
        }
        
        [Fact]
        public async Task CreateProperty_WithValidCommand_ReturnsCreatedAtAction()
        {
            // Arrange
            var command = new CreatePropertyCommand
            {
                Address = "123 Main St",
                City = "Bedford",
                State = "TX",
                ZipCode = "76021"
            };
            
            _mockMediator
                .Setup(m => m.Send(It.IsAny<CreatePropertyCommand>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(1);
                
            var controller = new PropertiesController(_mockMediator.Object);
            
            // Act
            var result = await controller.CreateProperty(command);
            
            // Assert
            var createdAtActionResult = Assert.IsType<CreatedAtActionResult>(result.Result);
            var returnValue = Assert.IsType<ApiResponse<int>>(createdAtActionResult.Value);
            Assert.True(returnValue.Success);
            Assert.Equal(1, returnValue.Data);
            Assert.Equal(nameof(controller.GetProperty), createdAtActionResult.ActionName);
        }
    }
}
```

### Mocking

The application uses Moq for mocking dependencies in unit tests:

```csharp
// Example of mocking a repository
var mockRepository = new Mock<IPropertyRepository>();
mockRepository
    .Setup(r => r.GetByIdAsync(1, It.IsAny<CancellationToken>()))
    .ReturnsAsync(new Property { Id = 1, Address = "123 Main St" });
```

### Test Data Builders

The application uses the builder pattern for creating test data:

```csharp
public class PropertyBuilder
{
    private readonly Property _property;
    
    public PropertyBuilder()
    {
        _property = new Property
        {
            Id = 1,
            Address = "123 Main St",
            City = "Bedford",
            State = "TX",
            ZipCode = "76021",
            Status = PropertyStatus.Vacant
        };
    }
    
    public PropertyBuilder WithId(int id)
    {
        _property.Id = id;
        return this;
    }
    
    public PropertyBuilder WithAddress(string address)
    {
        _property.Address = address;
        return this;
    }
    
    public PropertyBuilder WithStatus(PropertyStatus status)
    {
        _property.Status = status;
        return this;
    }
    
    public PropertyBuilder WithResident(Resident resident)
    {
        _property.Resident = resident;
        _property.ResidentId = resident.Id;
        return this;
    }
    
    public Property Build()
    {
        return _property;
    }
}
```

## Integration Testing

### Integration Test Structure

Integration tests verify the interaction between components:

```csharp
[Fact]
public async Task CreateProperty_WithValidData_CreatesPropertyInDatabase()
{
    // Arrange
    await using var application = new WebApplicationFactory<Program>()
        .WithWebHostBuilder(builder =>
        {
            builder.ConfigureServices(services =>
            {
                // Configure test services
            });
        });
        
    var client = application.CreateClient();
    var command = new CreatePropertyCommand
    {
        Address = "123 Main St",
        City = "Bedford",
        State = "TX",
        ZipCode = "76021"
    };
    
    // Act
    var response = await client.PostAsJsonAsync("/api/v1/properties", command);
    
    // Assert
    response.EnsureSuccessStatusCode();
    var content = await response.Content.ReadFromJsonAsync<ApiResponse<int>>();
    Assert.True(content.Success);
    Assert.True(content.Data > 0);
    
    // Verify in database
    using var scope = application.Services.CreateScope();
    var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    var property = await dbContext.Properties.FindAsync(content.Data);
    Assert.NotNull(property);
    Assert.Equal("123 Main St", property.Address);
}
```

### Test Database

Integration tests use an in-memory database or a dedicated test database:

```csharp
public class CustomWebApplicationFactory<TProgram> : WebApplicationFactory<TProgram> where TProgram : class
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureServices(services =>
        {
            // Remove the app's ApplicationDbContext registration
            var descriptor = services.SingleOrDefault(
                d => d.ServiceType == typeof(DbContextOptions<ApplicationDbContext>));
                
            if (descriptor != null)
            {
                services.Remove(descriptor);
            }
            
            // Add ApplicationDbContext using an in-memory database for testing
            services.AddDbContext<ApplicationDbContext>(options =>
            {
                options.UseInMemoryDatabase("InMemoryDbForTesting");
            });
            
            // Build the service provider
            var sp = services.BuildServiceProvider();
            
            // Create a scope to obtain a reference to the database
            using var scope = sp.CreateScope();
            var scopedServices = scope.ServiceProvider;
            var db = scopedServices.GetRequiredService<ApplicationDbContext>();
            var logger = scopedServices.GetRequiredService<ILogger<CustomWebApplicationFactory<TProgram>>>();
            
            // Ensure the database is created
            db.Database.EnsureCreated();
            
            try
            {
                // Seed the database with test data
                SeedTestData(db);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "An error occurred seeding the database. Error: {Message}", ex.Message);
            }
        });
    }
    
    private void SeedTestData(ApplicationDbContext context)
    {
        // Add test data here
        context.Properties.Add(new Property
        {
            Id = 1,
            Address = "123 Test St",
            City = "Bedford",
            State = "TX",
            ZipCode = "76021",
            Status = PropertyStatus.Vacant
        });
        
        context.SaveChanges();
    }
}
```

### API Integration Tests

API integration tests verify the behavior of API endpoints:

```csharp
public class PropertiesApiTests : IClassFixture<CustomWebApplicationFactory<Program>>
{
    private readonly CustomWebApplicationFactory<Program> _factory;
    private readonly HttpClient _client;
    
    public PropertiesApiTests(CustomWebApplicationFactory<Program> factory)
    {
        _factory = factory;
        _client = factory.CreateClient();
    }
    
    [Fact]
    public async Task GetProperties_ReturnsSuccessAndProperties()
    {
        // Act
        var response = await _client.GetAsync("/api/v1/properties");
        
        // Assert
        response.EnsureSuccessStatusCode();
        var content = await response.Content.ReadFromJsonAsync<ApiResponse<List<PropertyDto>>>();
        Assert.True(content.Success);
        Assert.NotEmpty(content.Data);
    }
    
    [Fact]
    public async Task GetProperty_WithValidId_ReturnsProperty()
    {
        // Act
        var response = await _client.GetAsync("/api/v1/properties/1");
        
        // Assert
        response.EnsureSuccessStatusCode();
        var content = await response.Content.ReadFromJsonAsync<ApiResponse<PropertyDto>>();
        Assert.True(content.Success);
        Assert.Equal(1, content.Data.Id);
        Assert.Equal("123 Test St", content.Data.Address);
    }
    
    [Fact]
    public async Task GetProperty_WithInvalidId_ReturnsNotFound()
    {
        // Act
        var response = await _client.GetAsync("/api/v1/properties/999");
        
        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }
}
```

### Database Integration Tests

Database integration tests verify the behavior of repositories and the database context:

```csharp
public class PropertyRepositoryIntegrationTests : IDisposable
{
    private readonly ApplicationDbContext _context;
    private readonly PropertyRepository _repository;
    
    public PropertyRepositoryIntegrationTests()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
            
        _context = new ApplicationDbContext(options);
        _repository = new PropertyRepository(_context);
        
        // Seed the database
        _context.Properties.Add(new Property
        {
            Id = 1,
            Address = "123 Test St",
            City = "Bedford",
            State = "TX",
            ZipCode = "76021",
            Status = PropertyStatus.Vacant
        });
        
        _context.SaveChanges();
    }
    
    [Fact]
    public async Task GetAllAsync_ReturnsAllProperties()
    {
        // Act
        var properties = await _repository.GetAllAsync(CancellationToken.None);
        
        // Assert
        Assert.Single(properties);
        Assert.Equal("123 Test St", properties.First().Address);
    }
    
    [Fact]
    public async Task AddAsync_AddsPropertyToDatabase()
    {
        // Arrange
        var property = new Property
        {
            Address = "456 New St",
            City = "Bedford",
            State = "TX",
            ZipCode = "76021",
            Status = PropertyStatus.Vacant
        };
        
        // Act
        var result = await _repository.AddAsync(property, CancellationToken.None);
        await _context.SaveChangesAsync();
        
        // Assert
        Assert.NotEqual(0, result.Id);
        var dbProperty = await _context.Properties.FindAsync(result.Id);
        Assert.NotNull(dbProperty);
        Assert.Equal("456 New St", dbProperty.Address);
    }
    
    public void Dispose()
    {
        _context.Database.EnsureDeleted();
        _context.Dispose();
    }
}
```

## UI Testing

### UI Testing Framework

The application uses bUnit for Blazor component testing and Playwright for end-to-end testing.

### bUnit Component Tests

```csharp
public class PropertyCardTests : TestContext
{
    [Fact]
    public void PropertyCard_DisplaysPropertyInformation()
    {
        // Arrange
        var property = new PropertyDto
        {
            Id = 1,
            Address = "123 Main St",
            City = "Bedford",
            State = "TX",
            ZipCode = "76021",
            Status = PropertyStatus.Vacant
        };
        
        // Act
        var cut = RenderComponent<PropertyCard>(parameters => parameters
            .Add(p => p.Property, property)
            .Add(p => p.ShowActions, true));
            
        // Assert
        cut.Find("h5").TextContent.Should().Be("123 Main St");
        cut.Find(".property-status").TextContent.Should().Contain("Vacant");
        cut.Find(".property-address").TextContent.Should().Contain("Bedford, TX 76021");
    }
    
    [Fact]
    public void PropertyCard_WhenEditClicked_InvokesOnEditCallback()
    {
        // Arrange
        var property = new PropertyDto { Id = 1, Address = "123 Main St" };
        var editInvoked = false;
        
        // Act
        var cut = RenderComponent<PropertyCard>(parameters => parameters
            .Add(p => p.Property, property)
            .Add(p => p.ShowActions, true)
            .Add(p => p.OnEdit, () => editInvoked = true));
            
        cut.Find(".btn-edit").Click();
        
        // Assert
        editInvoked.Should().BeTrue();
    }
}
```

### Playwright End-to-End Tests

```csharp
[Collection("PlaywrightCollection")]
public class PropertyPageTests : IAsyncLifetime
{
    private readonly PlaywrightFixture _fixture;
    private IPage _page;
    
    public PropertyPageTests(PlaywrightFixture fixture)
    {
        _fixture = fixture;
    }
    
    public async Task InitializeAsync()
    {
        _page = await _fixture.Browser.NewPageAsync();
        await _page.GotoAsync(_fixture.ServerAddress);
        
        // Login
        await _page.FillAsync("input[name='Email']", "admin@example.com");
        await _page.FillAsync("input[name='Password']", "Password123!");
        await _page.ClickAsync("button[type='submit']");
        
        // Navigate to properties page
        await _page.ClickAsync("text=Properties");
    }
    
    [Fact]
    public async Task PropertiesPage_DisplaysPropertiesList()
    {
        // Assert
        await _page.WaitForSelectorAsync(".property-card");
        var propertyCards = await _page.QuerySelectorAllAsync(".property-card");
        Assert.NotEmpty(propertyCards);
    }
    
    [Fact]
    public async Task CreateProperty_WithValidData_AddsNewProperty()
    {
        // Arrange
        await _page.ClickAsync("text=Add Property");
        
        // Act
        await _page.FillAsync("input[name='Address']", "789 New St");
        await _page.FillAsync("input[name='City']", "Bedford");
        await _page.FillAsync("input[name='State']", "TX");
        await _page.FillAsync("input[name='ZipCode']", "76021");
        await _page.ClickAsync("button[type='submit']");
        
        // Assert
        await _page.WaitForSelectorAsync("text=Property created successfully");
        var newProperty = await _page.QuerySelectorAsync("text=789 New St");
        Assert.NotNull(newProperty);
    }
    
    public async Task DisposeAsync()
    {
        await _page.CloseAsync();
    }
}
```

### Accessibility Testing

The application includes accessibility tests using axe-core:

```csharp
[Fact]
public async Task HomePage_IsAccessible()
{
    // Arrange
    await _page.GotoAsync(_fixture.ServerAddress);
    
    // Act
    var violations = await _page.EvaluateAsync<AxeResults>(@"
        async () => {
            const axe = await import('/axe.min.js');
            return await axe.run();
        }
    ");
    
    // Assert
    Assert.Empty(violations.Violations);
}
```

## Test Coverage

The application aims for high test coverage across all layers:

- **Domain Layer**: 90%+ coverage
- **Application Layer**: 85%+ coverage
- **Infrastructure Layer**: 80%+ coverage
- **Web Layer**: 75%+ coverage

### Coverage Reporting

The application uses Coverlet for coverage reporting:

```xml
<ItemGroup>
  <PackageReference Include="coverlet.collector" Version="6.0.0">
    <IncludeAssets>runtime; build; native; contentfiles; analyzers; buildtransitive</IncludeAssets>
    <PrivateAssets>all</PrivateAssets>
  </PackageReference>
  <PackageReference Include="coverlet.msbuild" Version="6.0.0">
    <IncludeAssets>runtime; build; native; contentfiles; analyzers; buildtransitive</IncludeAssets>
    <PrivateAssets>all</PrivateAssets>
  </PackageReference>
</ItemGroup>
```

### Coverage Commands

```bash
# Run tests with coverage
dotnet test /p:CollectCoverage=true /p:CoverletOutputFormat=cobertura

# Generate coverage report
reportgenerator -reports:"**/coverage.cobertura.xml" -targetdir:"coverage-report" -reporttypes:Html
```

## Continuous Integration

The application includes tests in the CI/CD pipeline:

```yaml
# .github/workflows/ci.yml
name: CI

on:
  push:
    branches: [ main, develop ]
  pull_request:
    branches: [ main, develop ]

jobs:
  build-and-test:
    runs-on: ubuntu-latest
    
    steps:
    - uses: actions/checkout@v3
    
    - name: Setup .NET
      uses: actions/setup-dotnet@v3
      with:
        dotnet-version: 9.0.x
        
    - name: Restore dependencies
      run: dotnet restore
      
    - name: Build
      run: dotnet build --no-restore
      
    - name: Test
      run: dotnet test --no-build --verbosity normal /p:CollectCoverage=true /p:CoverletOutputFormat=cobertura
      
    - name: Generate coverage report
      uses: danielpalme/ReportGenerator-GitHub-Action@5.2.0
      with:
        reports: '**/coverage.cobertura.xml'
        targetdir: 'coverage-report'
        reporttypes: 'HtmlInline;Cobertura'
        
    - name: Upload coverage report
      uses: actions/upload-artifact@v3
      with:
        name: coverage-report
        path: coverage-report
```

## Test Data Management

### Test Data Generation

The application uses a consistent approach to generating test data:

```csharp
public static class TestDataGenerator
{
    public static List<Property> GenerateProperties(int count = 10)
    {
        var properties = new List<Property>();
        
        for (int i = 1; i <= count; i++)
        {
            properties.Add(new Property
            {
                Id = i,
                Address = $"{i * 100} Main St",
                City = "Bedford",
                State = "TX",
                ZipCode = "76021",
                Status = i % 2 == 0 ? PropertyStatus.Vacant : PropertyStatus.Occupied
            });
        }
        
        return properties;
    }
    
    public static List<Resident> GenerateResidents(int count = 10)
    {
        var residents = new List<Resident>();
        
        for (int i = 1; i <= count; i++)
        {
            residents.Add(new Resident
            {
                Id = i,
                FirstName = $"FirstName{i}",
                LastName = $"LastName{i}",
                Email = $"resident{i}@example.com",
                Phone = $"555-555-{i:D4}"
            });
        }
        
        return residents;
    }
    
    // Additional methods for other entities
}
```

### Database Seeding

The application uses a consistent approach to seeding test databases:

```csharp
public static class TestDatabaseSeeder
{
    public static void SeedDatabase(ApplicationDbContext context)
    {
        // Clear existing data
        context.Properties.RemoveRange(context.Properties);
        context.Residents.RemoveRange(context.Residents);
        context.SaveChanges();
        
        // Add test properties
        var properties = TestDataGenerator.GenerateProperties();
        context.Properties.AddRange(properties);
        
        // Add test residents
        var residents = TestDataGenerator.GenerateResidents();
        context.Residents.AddRange(residents);
        
        // Associate residents with properties
        for (int i = 0; i < Math.Min(properties.Count, residents.Count); i++)
        {
            if (properties[i].Status == PropertyStatus.Occupied)
            {
                properties[i].ResidentId = residents[i].Id;
            }
        }
        
        context.SaveChanges();
    }
}
```

## Conclusion

These testing standards provide a comprehensive approach to testing the Wendover HOA application across all layers. By following these guidelines, the application will maintain high quality, reliability, and maintainability throughout its development lifecycle.

The standards ensure:

1. **Quality**: Comprehensive testing across all layers
2. **Consistency**: Uniform testing approaches and patterns
3. **Maintainability**: Clear, readable tests that serve as documentation
4. **Confidence**: High test coverage to support refactoring and enhancement

By implementing these testing standards, the Wendover HOA application will maintain a robust and reliable codebase that adheres to Clean Architecture principles and supports the application's long-term maintainability goals.
