# Integration Testing Approach

This document outlines the integration testing approach for the Wendover HOA project, focusing on API testing, database integration testing, and test data management. The approach adheres to Clean Architecture principles and ensures comprehensive testing of component interactions.

## Integration Testing Principles

The Wendover HOA project follows these core integration testing principles:

1. **Test Real Interactions**: Test actual interactions between components
2. **Minimize Mocking**: Use real dependencies where practical
3. **Isolated Test Environment**: Use dedicated test databases and environments
4. **Repeatable Tests**: Tests should be deterministic and repeatable
5. **Comprehensive Coverage**: Test all critical integration points
6. **Performance Awareness**: Integration tests should be reasonably performant

## Testing Frameworks and Tools

### Primary Testing Framework

The Wendover HOA project uses **xUnit** as the primary testing framework for integration tests:

```xml
<PackageReference Include="xunit" Version="2.6.2" />
<PackageReference Include="xunit.runner.visualstudio" Version="2.5.4" />
<PackageReference Include="Microsoft.NET.Test.Sdk" Version="17.8.0" />
```

### API Testing Tools

**Microsoft.AspNetCore.Mvc.Testing** is used for testing the API endpoints:

```xml
<PackageReference Include="Microsoft.AspNetCore.Mvc.Testing" Version="9.0.0" />
```

### Database Testing Tools

**Testcontainers** is used for database integration testing with Docker containers:

```xml
<PackageReference Include="Testcontainers" Version="3.6.0" />
<PackageReference Include="Testcontainers.MsSql" Version="3.6.0" />
```

### Additional Testing Tools

- **Respawn**: For database reset between tests
  ```xml
  <PackageReference Include="Respawn" Version="6.1.0" />
  ```

- **FluentAssertions**: For more readable assertions
  ```xml
  <PackageReference Include="FluentAssertions" Version="6.12.0" />
  ```

- **Bogus**: For generating realistic test data
  ```xml
  <PackageReference Include="Bogus" Version="35.0.1" />
  ```

## Project Structure

Integration tests are organized in a separate solution folder:

```
WendoverHOA.sln
├── src/
│   ├── WendoverHOA.Domain/
│   ├── WendoverHOA.Application/
│   ├── WendoverHOA.Infrastructure/
│   └── WendoverHOA.Web/
├── tests/
│   ├── WendoverHOA.UnitTests/
│   └── WendoverHOA.IntegrationTests/
│       ├── API/
│       │   ├── Controllers/
│       │   │   ├── PropertiesControllerTests.cs
│       │   │   ├── ResidentsControllerTests.cs
│       │   │   └── DuesControllerTests.cs
│       │   └── Middleware/
│       │       ├── ExceptionHandlingMiddlewareTests.cs
│       │       └── AuthenticationMiddlewareTests.cs
│       ├── Infrastructure/
│       │   ├── Repositories/
│       │   │   ├── PropertyRepositoryTests.cs
│       │   │   └── ResidentRepositoryTests.cs
│       │   └── Services/
│       │       ├── EmailServiceTests.cs
│       │       └── PaymentServiceTests.cs
│       ├── TestFixtures/
│       │   ├── WebApplicationFactory.cs
│       │   ├── DatabaseFixture.cs
│       │   └── TestAuthHandler.cs
│       └── TestHelpers/
│           ├── TestDataGenerator.cs
│           └── HttpClientExtensions.cs
└── tools/
    └── TestDataSeeder/
        ├── Program.cs
        └── SeedData.cs
```

## API Testing Strategy

### Web Application Factory

A custom `WebApplicationFactory` is used to create a test server for API testing:

```csharp
public class CustomWebApplicationFactory<TStartup> : WebApplicationFactory<TStartup> where TStartup : class
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

            // Register test authentication handler
            services.AddAuthentication("Test")
                .AddScheme<AuthenticationSchemeOptions, TestAuthHandler>(
                    "Test", options => { });

            // Build the service provider
            var sp = services.BuildServiceProvider();

            // Create a scope to obtain a reference to the database context
            using (var scope = sp.CreateScope())
            {
                var scopedServices = scope.ServiceProvider;
                var db = scopedServices.GetRequiredService<ApplicationDbContext>();
                var logger = scopedServices.GetRequiredService<ILogger<CustomWebApplicationFactory<TStartup>>>();

                db.Database.EnsureCreated();

                try
                {
                    // Seed the database with test data
                    SeedData.InitializeDbForTests(db);
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, "An error occurred seeding the database. Error: {Message}", ex.Message);
                }
            }
        });
    }
}
```

### API Test Structure

API tests follow this structure:

```csharp
public class PropertiesControllerTests : IClassFixture<CustomWebApplicationFactory<Program>>
{
    private readonly HttpClient _client;
    private readonly CustomWebApplicationFactory<Program> _factory;

    public PropertiesControllerTests(CustomWebApplicationFactory<Program> factory)
    {
        _factory = factory;
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task GetProperties_ReturnsSuccessAndCorrectContentType()
    {
        // Arrange
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Test");

        // Act
        var response = await _client.GetAsync("/api/v1/properties");

        // Assert
        response.EnsureSuccessStatusCode();
        response.Content.Headers.ContentType.ToString().Should().Be("application/json; charset=utf-8");
    }

    [Fact]
    public async Task GetProperties_ReturnsExpectedProperties()
    {
        // Arrange
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Test");

        // Act
        var response = await _client.GetAsync("/api/v1/properties");
        var content = await response.Content.ReadAsStringAsync();
        var result = JsonSerializer.Deserialize<ApiResponse<List<PropertyDto>>>(content, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });

        // Assert
        result.Should().NotBeNull();
        result.Success.Should().BeTrue();
        result.Data.Should().NotBeNull();
        result.Data.Should().HaveCountGreaterThan(0);
    }

    [Fact]
    public async Task GetPropertyById_WithValidId_ReturnsProperty()
    {
        // Arrange
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Test");
        var propertyId = 1; // Known test property ID

        // Act
        var response = await _client.GetAsync($"/api/v1/properties/{propertyId}");
        var content = await response.Content.ReadAsStringAsync();
        var result = JsonSerializer.Deserialize<ApiResponse<PropertyDto>>(content, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });

        // Assert
        response.EnsureSuccessStatusCode();
        result.Should().NotBeNull();
        result.Success.Should().BeTrue();
        result.Data.Should().NotBeNull();
        result.Data.Id.Should().Be(propertyId);
    }

    [Fact]
    public async Task CreateProperty_WithValidData_ReturnsCreatedProperty()
    {
        // Arrange
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Test");
        var newProperty = new CreatePropertyCommand
        {
            Address = "789 Pine St",
            City = "Bedford",
            State = "TX",
            ZipCode = "76021"
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/v1/properties", newProperty);
        var content = await response.Content.ReadAsStringAsync();
        var result = JsonSerializer.Deserialize<ApiResponse<PropertyDto>>(content, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);
        result.Should().NotBeNull();
        result.Success.Should().BeTrue();
        result.Data.Should().NotBeNull();
        result.Data.Address.Should().Be("789 Pine St");
    }
}
```

### Testing Authentication and Authorization

Authentication and authorization are tested using a custom `TestAuthHandler`:

```csharp
public class TestAuthHandler : AuthenticationHandler<AuthenticationSchemeOptions>
{
    public TestAuthHandler(
        IOptionsMonitor<AuthenticationSchemeOptions> options,
        ILoggerFactory logger,
        UrlEncoder encoder,
        ISystemClock clock)
        : base(options, logger, encoder, clock)
    {
    }

    protected override Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        var claims = new[]
        {
            new Claim(ClaimTypes.Name, "Test User"),
            new Claim(ClaimTypes.NameIdentifier, "1"),
            new Claim(ClaimTypes.Email, "test@example.com"),
            new Claim(ClaimTypes.Role, "Administrator"),
            new Claim("Permission", "property.view"),
            new Claim("Permission", "property.edit")
        };

        var identity = new ClaimsIdentity(claims, "Test");
        var principal = new ClaimsPrincipal(identity);
        var ticket = new AuthenticationTicket(principal, "Test");

        return Task.FromResult(AuthenticateResult.Success(ticket));
    }
}
```

## Database Integration Testing

### Database Test Fixture

A database test fixture is used to manage the test database:

```csharp
public class DatabaseFixture : IAsyncLifetime
{
    private readonly MsSqlContainer _sqlContainer;
    private Respawner _respawner;
    public string ConnectionString { get; private set; }

    public DatabaseFixture()
    {
        _sqlContainer = new MsSqlBuilder()
            .WithImage("mcr.microsoft.com/mssql/server:2022-latest")
            .WithPassword("StrongP@ssw0rd!")
            .Build();
    }

    public async Task InitializeAsync()
    {
        await _sqlContainer.StartAsync();
        ConnectionString = _sqlContainer.GetConnectionString();

        // Apply migrations
        var optionsBuilder = new DbContextOptionsBuilder<ApplicationDbContext>();
        optionsBuilder.UseSqlServer(ConnectionString);

        using (var context = new ApplicationDbContext(optionsBuilder.Options))
        {
            await context.Database.MigrateAsync();
        }

        // Initialize Respawner
        _respawner = await Respawner.CreateAsync(ConnectionString, new RespawnerOptions
        {
            TablesToIgnore = new[] { "__EFMigrationsHistory" }
        });
    }

    public async Task ResetDatabaseAsync()
    {
        await _respawner.ResetAsync(ConnectionString);
    }

    public async Task DisposeAsync()
    {
        await _sqlContainer.DisposeAsync();
    }
}
```

### Repository Tests

Repository tests use the database fixture:

```csharp
public class PropertyRepositoryTests : IClassFixture<DatabaseFixture>, IAsyncLifetime
{
    private readonly DatabaseFixture _fixture;
    private ApplicationDbContext _context;
    private PropertyRepository _repository;

    public PropertyRepositoryTests(DatabaseFixture fixture)
    {
        _fixture = fixture;
    }

    public async Task InitializeAsync()
    {
        await _fixture.ResetDatabaseAsync();

        var optionsBuilder = new DbContextOptionsBuilder<ApplicationDbContext>();
        optionsBuilder.UseSqlServer(_fixture.ConnectionString);
        _context = new ApplicationDbContext(optionsBuilder.Options);
        _repository = new PropertyRepository(_context);

        // Seed test data
        var properties = TestDataGenerator.GenerateProperties(5);
        await _context.Properties.AddRangeAsync(properties);
        await _context.SaveChangesAsync();
    }

    public async Task DisposeAsync()
    {
        await _context.DisposeAsync();
    }

    [Fact]
    public async Task GetAllAsync_ReturnsAllProperties()
    {
        // Act
        var properties = await _repository.GetAllAsync();

        // Assert
        properties.Should().NotBeNull();
        properties.Should().HaveCount(5);
    }

    [Fact]
    public async Task GetByIdAsync_WithValidId_ReturnsProperty()
    {
        // Arrange
        var expectedProperty = await _context.Properties.FirstAsync();

        // Act
        var property = await _repository.GetByIdAsync(expectedProperty.Id);

        // Assert
        property.Should().NotBeNull();
        property.Id.Should().Be(expectedProperty.Id);
        property.Address.Should().Be(expectedProperty.Address);
    }

    [Fact]
    public async Task AddAsync_ValidProperty_AddsToDatabase()
    {
        // Arrange
        var newProperty = new Property
        {
            Address = "789 Pine St",
            City = "Bedford",
            State = "TX",
            ZipCode = "76021"
        };

        // Act
        var addedProperty = await _repository.AddAsync(newProperty);
        await _context.SaveChangesAsync();

        // Assert
        addedProperty.Should().NotBeNull();
        addedProperty.Id.Should().BeGreaterThan(0);

        var dbProperty = await _context.Properties.FindAsync(addedProperty.Id);
        dbProperty.Should().NotBeNull();
        dbProperty.Address.Should().Be("789 Pine St");
    }
}
```

## Test Data Management

### Test Data Seeding

Test data is seeded using a dedicated seeder:

```csharp
public static class SeedData
{
    public static void InitializeDbForTests(ApplicationDbContext context)
    {
        // Clear existing data
        context.Properties.RemoveRange(context.Properties);
        context.Residents.RemoveRange(context.Residents);
        context.SaveChanges();

        // Seed properties
        var properties = new List<Property>
        {
            new Property
            {
                Address = "123 Main St",
                City = "Bedford",
                State = "TX",
                ZipCode = "76021",
                LotNumber = "A-123",
                Status = PropertyStatus.Occupied,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            },
            new Property
            {
                Address = "456 Oak Ave",
                City = "Bedford",
                State = "TX",
                ZipCode = "76021",
                LotNumber = "A-124",
                Status = PropertyStatus.Occupied,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            }
        };

        context.Properties.AddRange(properties);

        // Seed residents
        var residents = new List<Resident>
        {
            new Resident
            {
                FirstName = "John",
                LastName = "Doe",
                Email = "john.doe@example.com",
                PhoneNumber = "555-123-4567",
                Property = properties[0],
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            },
            new Resident
            {
                FirstName = "Jane",
                LastName = "Smith",
                Email = "jane.smith@example.com",
                PhoneNumber = "555-987-6543",
                Property = properties[1],
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            }
        };

        context.Residents.AddRange(residents);
        context.SaveChanges();
    }
}
```

### Dynamic Test Data Generation

Bogus is used for generating dynamic test data:

```csharp
public static class TestDataGenerator
{
    public static List<Property> GenerateProperties(int count = 10)
    {
        var faker = new Faker<Property>()
            .RuleFor(p => p.Address, f => f.Address.StreetAddress())
            .RuleFor(p => p.City, f => "Bedford")
            .RuleFor(p => p.State, f => "TX")
            .RuleFor(p => p.ZipCode, f => "76021")
            .RuleFor(p => p.LotNumber, f => $"A-{f.Random.Number(100, 999)}")
            .RuleFor(p => p.Status, f => f.PickRandom<PropertyStatus>())
            .RuleFor(p => p.CreatedAt, f => f.Date.Past(1))
            .RuleFor(p => p.UpdatedAt, f => f.Date.Recent(30));
        
        return faker.Generate(count);
    }
    
    public static List<Resident> GenerateResidents(int count = 10)
    {
        var faker = new Faker<Resident>()
            .RuleFor(r => r.FirstName, f => f.Name.FirstName())
            .RuleFor(r => r.LastName, f => f.Name.LastName())
            .RuleFor(r => r.Email, (f, r) => f.Internet.Email(r.FirstName, r.LastName))
            .RuleFor(r => r.PhoneNumber, f => f.Phone.PhoneNumber("###-###-####"))
            .RuleFor(r => r.CreatedAt, f => f.Date.Past(1))
            .RuleFor(r => r.UpdatedAt, f => f.Date.Recent(30));
        
        return faker.Generate(count);
    }
}
```

## Testing External Services

External services are tested using test doubles or real services in test mode:

```csharp
public class EmailServiceTests : IClassFixture<CustomWebApplicationFactory<Program>>
{
    private readonly IEmailService _emailService;
    private readonly Mock<ISmtpClient> _mockSmtpClient;

    public EmailServiceTests(CustomWebApplicationFactory<Program> factory)
    {
        _mockSmtpClient = new Mock<ISmtpClient>();
        _emailService = new EmailService(_mockSmtpClient.Object, Options.Create(new EmailSettings
        {
            FromEmail = "noreply@wendoverhoa.org",
            FromName = "Wendover HOA"
        }));
    }

    [Fact]
    public async Task SendEmailAsync_ValidEmail_CallsSmtpClient()
    {
        // Arrange
        var to = "user@example.com";
        var subject = "Test Email";
        var body = "This is a test email";

        _mockSmtpClient.Setup(x => x.SendAsync(It.IsAny<MimeMessage>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // Act
        await _emailService.SendEmailAsync(to, subject, body);

        // Assert
        _mockSmtpClient.Verify(x => x.SendAsync(
            It.Is<MimeMessage>(m => 
                m.To.Mailboxes.First().Address == to &&
                m.Subject == subject &&
                m.TextBody == body),
            It.IsAny<CancellationToken>()),
            Times.Once);
    }
}
```

## Testing API Endpoints

API endpoints are tested end-to-end:

```csharp
[Fact]
public async Task GetProperties_WithPagination_ReturnsPaginatedResults()
{
    // Arrange
    _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Test");

    // Act
    var response = await _client.GetAsync("/api/v1/properties?pageNumber=1&pageSize=5");
    var content = await response.Content.ReadAsStringAsync();
    var result = JsonSerializer.Deserialize<ApiResponse<List<PropertyDto>>>(content, new JsonSerializerOptions
    {
        PropertyNameCaseInsensitive = true
    });

    // Assert
    response.EnsureSuccessStatusCode();
    result.Should().NotBeNull();
    result.Success.Should().BeTrue();
    result.Data.Should().NotBeNull();
    result.Data.Should().HaveCountLessThanOrEqualTo(5);
    
    // Check pagination headers
    response.Headers.TryGetValues("X-Pagination", out var paginationHeaders);
    paginationHeaders.Should().NotBeNull();
    var paginationHeader = paginationHeaders.First();
    paginationHeader.Should().Contain("\"pageSize\":5");
    paginationHeader.Should().Contain("\"currentPage\":1");
}
```

## Testing Error Handling

Error handling is tested by triggering error conditions:

```csharp
[Fact]
public async Task GetPropertyById_WithInvalidId_ReturnsNotFound()
{
    // Arrange
    _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Test");
    var invalidId = 9999; // Non-existent ID

    // Act
    var response = await _client.GetAsync($"/api/v1/properties/{invalidId}");
    var content = await response.Content.ReadAsStringAsync();
    var result = JsonSerializer.Deserialize<ApiResponse<object>>(content, new JsonSerializerOptions
    {
        PropertyNameCaseInsensitive = true
    });

    // Assert
    response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    result.Should().NotBeNull();
    result.Success.Should().BeFalse();
    result.Error.Should().NotBeNull();
    result.Error.Code.Should().Be("RESOURCE_NOT_FOUND");
}
```

## Testing Transactions

Database transactions are tested to ensure data integrity:

```csharp
[Fact]
public async Task CreatePropertyWithResident_Transaction_CommitsOrRollsBackCorrectly()
{
    // Arrange
    var optionsBuilder = new DbContextOptionsBuilder<ApplicationDbContext>();
    optionsBuilder.UseSqlServer(_fixture.ConnectionString);
    using var context = new ApplicationDbContext(optionsBuilder.Options);
    
    var propertyRepository = new PropertyRepository(context);
    var residentRepository = new ResidentRepository(context);
    var unitOfWork = new UnitOfWork(context);
    
    var newProperty = new Property
    {
        Address = "789 Pine St",
        City = "Bedford",
        State = "TX",
        ZipCode = "76021"
    };
    
    var newResident = new Resident
    {
        FirstName = "Invalid", // Will cause validation failure
        LastName = "", // Required field
        Email = "test@example.com"
    };
    
    // Act & Assert
    await using var transaction = await context.Database.BeginTransactionAsync();
    try
    {
        var addedProperty = await propertyRepository.AddAsync(newProperty);
        var addedResident = await residentRepository.AddAsync(newResident);
        addedResident.PropertyId = addedProperty.Id;
        
        await unitOfWork.SaveChangesAsync(); // Should throw validation exception
        await transaction.CommitAsync();
        
        // If we get here, the test should fail
        Assert.Fail("Expected validation exception was not thrown");
    }
    catch (Exception)
    {
        // Expected exception
        await transaction.RollbackAsync();
        
        // Verify property was not added
        var properties = await propertyRepository.GetAllAsync();
        properties.Should().NotContain(p => p.Address == "789 Pine St");
    }
}
```

## Integration Test Execution

Integration tests are executed as part of the CI/CD pipeline but separately from unit tests:

```yaml
- name: Run Integration Tests
  run: dotnet test ./tests/WendoverHOA.IntegrationTests/WendoverHOA.IntegrationTests.csproj --no-restore --verbosity normal
```

## Best Practices

1. **Isolate Test Environment**: Use dedicated test databases and environments
2. **Clean Up After Tests**: Reset the database between tests
3. **Use Realistic Data**: Test with realistic data that mimics production
4. **Test Happy and Unhappy Paths**: Test both successful and error scenarios
5. **Test Performance**: Ensure integration tests run within reasonable time limits
6. **Minimize External Dependencies**: Use test doubles for external services when appropriate
7. **Test Transactions**: Ensure transactions work correctly
8. **Test Concurrency**: Test concurrent access to resources
9. **Test Security**: Test authentication and authorization
10. **Document Tests**: Document the purpose and expectations of each test

## Continuous Improvement

The integration testing approach is subject to continuous improvement:

1. **Regular Reviews**: Review test coverage and quality regularly
2. **Performance Optimization**: Optimize test performance
3. **Automation**: Automate test execution and reporting
4. **Feedback**: Incorporate feedback from developers

## Conclusion

The integration testing approach for the Wendover HOA project ensures comprehensive testing of component interactions. By following Clean Architecture principles and using modern testing tools and techniques, the project maintains high quality and reliability in its integrated components.
