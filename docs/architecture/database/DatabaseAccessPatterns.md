# Database Access Patterns

This document outlines the database access patterns used in the Wendover HOA application, including repository implementations, query optimization strategies, and Entity Framework Core configurations. These patterns ensure efficient data access while maintaining the Clean Architecture principles of the application.

## Repository Pattern Implementation

The Wendover HOA application implements the repository pattern to abstract data access logic from business logic, following Clean Architecture principles. This approach provides a clean separation of concerns and makes the application more testable and maintainable.

### Repository Interfaces

Repository interfaces are defined in the Domain layer and implemented in the Infrastructure layer. Each entity has its own repository interface that defines the operations available for that entity.

```csharp
// Domain Layer
public interface IPropertyRepository
{
    Task<Property> GetByIdAsync(int id);
    Task<IEnumerable<Property>> GetAllAsync();
    Task<IEnumerable<Property>> GetByZipCodeAsync(string zipCode);
    Task<Property> AddAsync(Property property);
    Task UpdateAsync(Property property);
    Task DeleteAsync(int id);
    Task<bool> ExistsAsync(int id);
    Task<int> CountAsync();
}
```

### Generic Repository

A generic repository provides common CRUD operations for all entities, reducing code duplication:

```csharp
// Infrastructure Layer
public class GenericRepository<T> : IGenericRepository<T> where T : class
{
    protected readonly ApplicationDbContext _context;
    protected readonly DbSet<T> _dbSet;

    public GenericRepository(ApplicationDbContext context)
    {
        _context = context;
        _dbSet = context.Set<T>();
    }

    public virtual async Task<T> GetByIdAsync(int id)
    {
        return await _dbSet.FindAsync(id);
    }

    public virtual async Task<IEnumerable<T>> GetAllAsync()
    {
        return await _dbSet.ToListAsync();
    }

    public virtual async Task<T> AddAsync(T entity)
    {
        await _dbSet.AddAsync(entity);
        await _context.SaveChangesAsync();
        return entity;
    }

    public virtual async Task UpdateAsync(T entity)
    {
        _dbSet.Update(entity);
        await _context.SaveChangesAsync();
    }

    public virtual async Task DeleteAsync(int id)
    {
        var entity = await GetByIdAsync(id);
        _dbSet.Remove(entity);
        await _context.SaveChangesAsync();
    }

    public virtual async Task<bool> ExistsAsync(int id)
    {
        var entity = await GetByIdAsync(id);
        return entity != null;
    }

    public virtual async Task<int> CountAsync()
    {
        return await _dbSet.CountAsync();
    }
}
```

### Specific Repository Implementations

Entity-specific repositories extend the generic repository to provide specialized operations:

```csharp
// Infrastructure Layer
public class PropertyRepository : GenericRepository<Property>, IPropertyRepository
{
    public PropertyRepository(ApplicationDbContext context) : base(context)
    {
    }

    public async Task<IEnumerable<Property>> GetByZipCodeAsync(string zipCode)
    {
        return await _dbSet
            .Where(p => p.ZipCode == zipCode)
            .ToListAsync();
    }

    public override async Task<Property> GetByIdAsync(int id)
    {
        return await _dbSet
            .Include(p => p.Residents)
            .FirstOrDefaultAsync(p => p.Id == id);
    }

    // Additional specialized methods...
}
```

### Unit of Work Pattern

The Unit of Work pattern coordinates operations across multiple repositories and ensures transaction consistency:

```csharp
// Infrastructure Layer
public class UnitOfWork : IUnitOfWork
{
    private readonly ApplicationDbContext _context;
    private readonly Dictionary<Type, object> _repositories = new Dictionary<Type, object>();

    public UnitOfWork(ApplicationDbContext context)
    {
        _context = context;
    }

    public IGenericRepository<T> Repository<T>() where T : class
    {
        var type = typeof(T);
        if (!_repositories.ContainsKey(type))
        {
            _repositories[type] = new GenericRepository<T>(_context);
        }
        return (IGenericRepository<T>)_repositories[type];
    }

    public async Task<int> CompleteAsync()
    {
        return await _context.SaveChangesAsync();
    }

    public void Dispose()
    {
        _context.Dispose();
    }
}
```

## Query Optimization Strategies

### Eager Loading vs. Lazy Loading

The application primarily uses eager loading to avoid the N+1 query problem:

```csharp
// Eager loading example
public async Task<IEnumerable<Property>> GetPropertiesWithResidentsAsync()
{
    return await _context.Properties
        .Include(p => p.Residents)
        .ToListAsync();
}
```

Lazy loading is disabled by default but can be enabled for specific scenarios:

```csharp
// In ApplicationDbContext configuration
protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
{
    optionsBuilder.UseLazyLoadingProxies(false); // Disabled by default
}
```

### Projection Queries

For read-only operations, projection queries are used to retrieve only the required data:

```csharp
public async Task<IEnumerable<PropertySummaryDto>> GetPropertySummariesAsync()
{
    return await _context.Properties
        .Select(p => new PropertySummaryDto
        {
            Id = p.Id,
            Address = p.Address,
            ResidentCount = p.Residents.Count
        })
        .ToListAsync();
}
```

### Paging and Filtering

All list operations support paging and filtering to limit the amount of data retrieved:

```csharp
public async Task<PagedResult<Property>> GetPagedPropertiesAsync(
    int pageNumber, 
    int pageSize, 
    string searchTerm = null)
{
    var query = _context.Properties.AsQueryable();

    if (!string.IsNullOrEmpty(searchTerm))
    {
        query = query.Where(p => 
            p.Address.Contains(searchTerm) || 
            p.LotNumber.Contains(searchTerm));
    }

    var totalCount = await query.CountAsync();
    var items = await query
        .Skip((pageNumber - 1) * pageSize)
        .Take(pageSize)
        .ToListAsync();

    return new PagedResult<Property>
    {
        Items = items,
        TotalCount = totalCount,
        PageNumber = pageNumber,
        PageSize = pageSize
    };
}
```

### Compiled Queries

For frequently executed queries, compiled queries improve performance:

```csharp
private static readonly Func<ApplicationDbContext, int, Task<Property>> GetPropertyByIdQuery =
    EF.CompileAsyncQuery((ApplicationDbContext context, int id) =>
        context.Properties
            .Include(p => p.Residents)
            .FirstOrDefault(p => p.Id == id));

public async Task<Property> GetPropertyByIdOptimizedAsync(int id)
{
    return await GetPropertyByIdQuery(_context, id);
}
```

### Tracking vs. No-Tracking Queries

Read-only queries use no-tracking for better performance:

```csharp
public async Task<IEnumerable<Property>> GetAllPropertiesNoTrackingAsync()
{
    return await _context.Properties
        .AsNoTracking()
        .ToListAsync();
}
```

### Batch Operations

For bulk operations, Entity Framework Core's batch extensions are used:

```csharp
public async Task UpdatePropertyStatusBatchAsync(string zipCode, PropertyStatus newStatus)
{
    await _context.Properties
        .Where(p => p.ZipCode == zipCode)
        .ExecuteUpdateAsync(s => s
            .SetProperty(p => p.Status, newStatus)
            .SetProperty(p => p.UpdatedAt, DateTime.UtcNow));
}
```

## Entity Framework Core Configuration

### DbContext Configuration

The `ApplicationDbContext` is configured with appropriate options for performance and security:

```csharp
public class ApplicationDbContext : IdentityDbContext<ApplicationUser, ApplicationRole, int>
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        if (!optionsBuilder.IsConfigured)
        {
            optionsBuilder
                .UseSqlServer(
                    "Server=(localdb)\\mssqllocaldb;Database=WendoverHOA;Trusted_Connection=True;MultipleActiveResultSets=true",
                    sqlOptions =>
                    {
                        sqlOptions.EnableRetryOnFailure(
                            maxRetryCount: 5,
                            maxRetryDelay: TimeSpan.FromSeconds(30),
                            errorNumbersToAdd: null);
                        sqlOptions.CommandTimeout(60);
                    })
                .UseQueryTrackingBehavior(QueryTrackingBehavior.NoTracking);
        }
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Apply all configurations from the current assembly
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(ApplicationDbContext).Assembly);

        // Global query filters
        modelBuilder.Entity<Property>().HasQueryFilter(p => !p.IsDeleted);
        modelBuilder.Entity<Resident>().HasQueryFilter(r => !r.IsDeleted);
        
        // Configure decimal precision globally
        foreach (var property in modelBuilder.Model.GetEntityTypes()
            .SelectMany(t => t.GetProperties())
            .Where(p => p.ClrType == typeof(decimal) || p.ClrType == typeof(decimal?)))
        {
            property.SetPrecision(18);
            property.SetScale(2);
        }
    }

    // DbSet properties for all entities
    public DbSet<Property> Properties { get; set; }
    public DbSet<Resident> Residents { get; set; }
    public DbSet<DuesTransaction> DuesTransactions { get; set; }
    // Other DbSet properties...
}
```

### Entity Type Configurations

Entity configurations are separated into dedicated classes using the Fluent API:

```csharp
public class PropertyConfiguration : IEntityTypeConfiguration<Property>
{
    public void Configure(EntityTypeBuilder<Property> builder)
    {
        builder.ToTable("Properties");

        builder.HasKey(p => p.Id);
        
        builder.Property(p => p.Address)
            .IsRequired()
            .HasMaxLength(200);
            
        builder.Property(p => p.City)
            .IsRequired()
            .HasMaxLength(100);
            
        builder.Property(p => p.State)
            .IsRequired()
            .HasMaxLength(2);
            
        builder.Property(p => p.ZipCode)
            .IsRequired()
            .HasMaxLength(10);
            
        builder.Property(p => p.LotNumber)
            .IsRequired()
            .HasMaxLength(20);
            
        builder.Property(p => p.SquareFootage)
            .HasPrecision(10, 2);
            
        builder.Property(p => p.YearBuilt)
            .IsRequired();
            
        builder.Property(p => p.Status)
            .IsRequired()
            .HasConversion<string>();
            
        builder.Property(p => p.CreatedAt)
            .IsRequired()
            .HasDefaultValueSql("GETUTCDATE()");
            
        builder.Property(p => p.UpdatedAt)
            .IsRequired()
            .HasDefaultValueSql("GETUTCDATE()");
            
        builder.HasIndex(p => p.Address).IsUnique();
        builder.HasIndex(p => p.LotNumber);
        builder.HasIndex(p => p.ZipCode);
        
        // Relationships
        builder.HasMany(p => p.Residents)
            .WithOne(r => r.Property)
            .HasForeignKey(r => r.PropertyId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
```

### Value Conversions

Value conversions are used to map between domain types and database types:

```csharp
public class DuesTransactionConfiguration : IEntityTypeConfiguration<DuesTransaction>
{
    public void Configure(EntityTypeBuilder<DuesTransaction> builder)
    {
        // Other configuration...
        
        builder.Property(d => d.TransactionType)
            .IsRequired()
            .HasConversion(
                v => v.ToString(),
                v => (TransactionType)Enum.Parse(typeof(TransactionType), v));
                
        builder.Property(d => d.Amount)
            .HasColumnType("decimal(18,2)")
            .IsRequired();
            
        // Other configuration...
    }
}
```

### Owned Entity Types

Value objects are configured as owned entity types:

```csharp
public class ResidentConfiguration : IEntityTypeConfiguration<Resident>
{
    public void Configure(EntityTypeBuilder<Resident> builder)
    {
        // Other configuration...
        
        builder.OwnsOne(r => r.Address, address =>
        {
            address.Property(a => a.Street).HasMaxLength(200).IsRequired();
            address.Property(a => a.City).HasMaxLength(100).IsRequired();
            address.Property(a => a.State).HasMaxLength(2).IsRequired();
            address.Property(a => a.ZipCode).HasMaxLength(10).IsRequired();
        });
        
        // Other configuration...
    }
}
```

### Global Query Filters

Global query filters are used for soft delete and multi-tenancy:

```csharp
protected override void OnModelCreating(ModelBuilder modelBuilder)
{
    // Other configuration...
    
    // Soft delete filter
    modelBuilder.Entity<Property>().HasQueryFilter(p => !p.IsDeleted);
    modelBuilder.Entity<Resident>().HasQueryFilter(r => !r.IsDeleted);
    
    // Multi-tenancy filter (if applicable)
    if (_tenantProvider != null)
    {
        var tenantId = _tenantProvider.GetTenantId();
        modelBuilder.Entity<Property>().HasQueryFilter(p => !p.IsDeleted && p.TenantId == tenantId);
    }
    
    // Other configuration...
}
```

## Performance Optimization Techniques

### Indexing Strategy

The application uses a comprehensive indexing strategy:

1. **Primary Key Indexes**: Automatically created for all primary keys
2. **Foreign Key Indexes**: Created for all foreign key columns
3. **Lookup Indexes**: Created for columns frequently used in WHERE clauses
4. **Covering Indexes**: Created for queries that retrieve specific columns frequently
5. **Full-Text Indexes**: Used for text search operations on large text columns

Example of index creation in migrations:

```csharp
migrationBuilder.CreateIndex(
    name: "IX_Properties_Address",
    table: "Properties",
    column: "Address",
    unique: true);
    
migrationBuilder.CreateIndex(
    name: "IX_Properties_ZipCode",
    table: "Properties",
    column: "ZipCode");
    
migrationBuilder.CreateIndex(
    name: "IX_Residents_PropertyId_LastName_FirstName",
    table: "Residents",
    columns: new[] { "PropertyId", "LastName", "FirstName" });
```

### Query Caching

The application implements a multi-level caching strategy:

1. **In-Memory Cache**: For frequently accessed, relatively static data
2. **Distributed Cache**: For data that needs to be shared across multiple instances
3. **Second-Level Cache**: For Entity Framework query results

Example of caching implementation:

```csharp
public async Task<IEnumerable<Property>> GetAllPropertiesCachedAsync()
{
    string cacheKey = "AllProperties";
    
    // Try to get from cache first
    if (!_cache.TryGetValue(cacheKey, out IEnumerable<Property> properties))
    {
        // If not in cache, get from database
        properties = await _context.Properties
            .AsNoTracking()
            .ToListAsync();
            
        // Store in cache with expiration
        var cacheOptions = new MemoryCacheEntryOptions()
            .SetAbsoluteExpiration(TimeSpan.FromMinutes(10));
            
        _cache.Set(cacheKey, properties, cacheOptions);
    }
    
    return properties;
}
```

### Connection Resiliency

The application implements connection resiliency to handle transient database failures:

```csharp
services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(
        Configuration.GetConnectionString("DefaultConnection"),
        sqlOptions =>
        {
            sqlOptions.EnableRetryOnFailure(
                maxRetryCount: 5,
                maxRetryDelay: TimeSpan.FromSeconds(30),
                errorNumbersToAdd: null);
        }));
```

### Command Interception

Command interception is used for logging and performance monitoring:

```csharp
public class CommandInterceptor : DbCommandInterceptor
{
    private readonly ILogger<CommandInterceptor> _logger;
    
    public CommandInterceptor(ILogger<CommandInterceptor> logger)
    {
        _logger = logger;
    }
    
    public override InterceptionResult<DbDataReader> ReaderExecuting(
        DbCommand command, 
        CommandEventData eventData, 
        InterceptionResult<DbDataReader> result)
    {
        _logger.LogInformation($"Executing command: {command.CommandText}");
        return result;
    }
    
    public override async ValueTask<InterceptionResult<DbDataReader>> ReaderExecutingAsync(
        DbCommand command, 
        CommandEventData eventData, 
        InterceptionResult<DbDataReader> result, 
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation($"Executing command async: {command.CommandText}");
        return await ValueTask.FromResult(result);
    }
}
```

## CQRS Implementation with Repositories

The application implements the CQRS pattern using MediatR, with repositories handling the data access:

### Query Handler Example

```csharp
public class GetPropertyByIdQuery : IRequest<PropertyDto>
{
    public int Id { get; set; }
}

public class GetPropertyByIdQueryHandler : IRequestHandler<GetPropertyByIdQuery, PropertyDto>
{
    private readonly IPropertyRepository _propertyRepository;
    private readonly IMapper _mapper;
    
    public GetPropertyByIdQueryHandler(IPropertyRepository propertyRepository, IMapper mapper)
    {
        _propertyRepository = propertyRepository;
        _mapper = mapper;
    }
    
    public async Task<PropertyDto> Handle(GetPropertyByIdQuery request, CancellationToken cancellationToken)
    {
        var property = await _propertyRepository.GetByIdAsync(request.Id);
        return _mapper.Map<PropertyDto>(property);
    }
}
```

### Command Handler Example

```csharp
public class CreatePropertyCommand : IRequest<int>
{
    public string Address { get; set; }
    public string City { get; set; }
    public string State { get; set; }
    public string ZipCode { get; set; }
    public string LotNumber { get; set; }
    public decimal SquareFootage { get; set; }
    public int YearBuilt { get; set; }
}

public class CreatePropertyCommandHandler : IRequestHandler<CreatePropertyCommand, int>
{
    private readonly IPropertyRepository _propertyRepository;
    private readonly IMapper _mapper;
    
    public CreatePropertyCommandHandler(IPropertyRepository propertyRepository, IMapper mapper)
    {
        _propertyRepository = propertyRepository;
        _mapper = mapper;
    }
    
    public async Task<int> Handle(CreatePropertyCommand request, CancellationToken cancellationToken)
    {
        var property = _mapper.Map<Property>(request);
        property.CreatedAt = DateTime.UtcNow;
        property.UpdatedAt = DateTime.UtcNow;
        property.Status = PropertyStatus.Active;
        
        var result = await _propertyRepository.AddAsync(property);
        return result.Id;
    }
}
```

## Best Practices and Guidelines

### Repository Pattern Best Practices

1. **Keep repositories focused**: Each repository should handle a single entity or a closely related group of entities
2. **Use asynchronous methods**: All database operations should be asynchronous
3. **Return domain entities**: Repositories should return domain entities, not DTOs
4. **Use specification pattern**: For complex queries, use the specification pattern
5. **Implement proper error handling**: Handle database exceptions and provide meaningful error messages

### Query Optimization Best Practices

1. **Measure before optimizing**: Use SQL Server Profiler or EF Core logging to identify slow queries
2. **Use appropriate loading strategies**: Choose between eager loading, explicit loading, and projection based on the use case
3. **Limit result sets**: Always use paging for large result sets
4. **Use indexes effectively**: Create indexes based on query patterns
5. **Avoid N+1 query problems**: Use Include or explicit loading to avoid multiple database roundtrips

### Entity Framework Core Best Practices

1. **Use migrations**: Always use migrations to manage database schema changes
2. **Configure relationships explicitly**: Don't rely on convention for important relationships
3. **Use value conversions**: Map between domain types and database types using value conversions
4. **Set appropriate cascade delete behavior**: Configure cascade delete behavior explicitly
5. **Use global query filters**: Use global query filters for cross-cutting concerns like soft delete
6. **Configure appropriate transaction isolation levels**: Choose the right isolation level for your use case
