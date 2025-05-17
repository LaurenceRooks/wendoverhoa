# Data Audit and Versioning

This document outlines the data audit and versioning strategy for the Wendover HOA application, detailing how changes to data are tracked, logged, and accessed throughout the application lifecycle.

## Change Tracking Implementation

### Entity Base Classes

The application uses base classes to standardize audit fields across entities:

```csharp
public abstract class AuditableEntity
{
    public DateTime CreatedAt { get; set; }
    public string CreatedBy { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public string UpdatedBy { get; set; }
}

public abstract class SoftDeleteEntity : AuditableEntity
{
    public bool IsDeleted { get; set; }
    public DateTime? DeletedAt { get; set; }
    public string DeletedBy { get; set; }
}
```

### Automatic Audit Field Population

Audit fields are automatically populated using a SaveChanges interceptor:

```csharp
public class AuditSaveChangesInterceptor : SaveChangesInterceptor
{
    private readonly ICurrentUserService _currentUserService;
    private readonly IDateTime _dateTime;

    public AuditSaveChangesInterceptor(
        ICurrentUserService currentUserService,
        IDateTime dateTime)
    {
        _currentUserService = currentUserService;
        _dateTime = dateTime;
    }

    public override InterceptionResult<int> SavingChanges(
        DbContextEventData eventData, 
        InterceptionResult<int> result)
    {
        UpdateEntities(eventData.Context);
        return base.SavingChanges(eventData, result);
    }

    public override ValueTask<InterceptionResult<int>> SavingChangesAsync(
        DbContextEventData eventData, 
        InterceptionResult<int> result, 
        CancellationToken cancellationToken = default)
    {
        UpdateEntities(eventData.Context);
        return base.SavingChangesAsync(eventData, result, cancellationToken);
    }

    private void UpdateEntities(DbContext context)
    {
        if (context == null) return;

        var userId = _currentUserService.UserId;
        var now = _dateTime.Now;

        foreach (var entry in context.ChangeTracker.Entries<AuditableEntity>())
        {
            if (entry.State == EntityState.Added)
            {
                entry.Entity.CreatedBy = userId;
                entry.Entity.CreatedAt = now;
            }

            if (entry.State == EntityState.Added || entry.State == EntityState.Modified)
            {
                entry.Entity.UpdatedBy = userId;
                entry.Entity.UpdatedAt = now;
            }
        }

        foreach (var entry in context.ChangeTracker.Entries<SoftDeleteEntity>())
        {
            if (entry.State == EntityState.Deleted)
            {
                entry.State = EntityState.Modified;
                entry.Entity.IsDeleted = true;
                entry.Entity.DeletedBy = userId;
                entry.Entity.DeletedAt = now;
            }
        }
    }
}
```

### Entity-Specific History Tables

For entities requiring detailed change history, dedicated history tables are used:

```csharp
public class PropertyHistory
{
    public int Id { get; set; }
    public int PropertyId { get; set; }
    public Property Property { get; set; }
    public string ChangeType { get; set; } // Created, Updated, Deleted
    public DateTime ChangeDate { get; set; }
    public string ChangedBy { get; set; }
    public string OldValues { get; set; } // JSON serialized
    public string NewValues { get; set; } // JSON serialized
}
```

### Temporal Tables

For SQL Server 2022, system-versioned temporal tables provide built-in historical data tracking:

```csharp
public class PropertyConfiguration : IEntityTypeConfiguration<Property>
{
    public void Configure(EntityTypeBuilder<Property> builder)
    {
        // Other configuration...
        
        // Configure as temporal table
        builder.ToTable("Properties", b => b.IsTemporal(
            b =>
            {
                b.HasPeriodStart("ValidFrom");
                b.HasPeriodEnd("ValidTo");
                b.UseHistoryTable("PropertiesHistory");
            }));
    }
}
```

## Audit Logging for Data Modifications

### Centralized Audit Logging

The application implements a centralized audit logging system:

```csharp
public class AuditLog
{
    public int Id { get; set; }
    public string UserId { get; set; }
    public string UserName { get; set; }
    public string EntityType { get; set; }
    public string EntityId { get; set; }
    public string Action { get; set; } // Create, Update, Delete, Read
    public DateTime Timestamp { get; set; }
    public string OldValues { get; set; }
    public string NewValues { get; set; }
    public string ClientIp { get; set; }
    public string UserAgent { get; set; }
}
```

### Audit Log Service

```csharp
public interface IAuditLogService
{
    Task LogAsync(string entityType, string entityId, string action, 
        object oldValues = null, object newValues = null);
}

public class AuditLogService : IAuditLogService
{
    private readonly ApplicationDbContext _context;
    private readonly ICurrentUserService _currentUserService;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly IDateTime _dateTime;

    public AuditLogService(
        ApplicationDbContext context,
        ICurrentUserService currentUserService,
        IHttpContextAccessor httpContextAccessor,
        IDateTime dateTime)
    {
        _context = context;
        _currentUserService = currentUserService;
        _httpContextAccessor = httpContextAccessor;
        _dateTime = dateTime;
    }

    public async Task LogAsync(string entityType, string entityId, string action, 
        object oldValues = null, object newValues = null)
    {
        var httpContext = _httpContextAccessor.HttpContext;
        var clientIp = httpContext?.Connection?.RemoteIpAddress?.ToString();
        var userAgent = httpContext?.Request?.Headers["User-Agent"].ToString();

        var auditLog = new AuditLog
        {
            UserId = _currentUserService.UserId,
            UserName = _currentUserService.UserName,
            EntityType = entityType,
            EntityId = entityId,
            Action = action,
            Timestamp = _dateTime.Now,
            OldValues = oldValues != null ? JsonSerializer.Serialize(oldValues) : null,
            NewValues = newValues != null ? JsonSerializer.Serialize(newValues) : null,
            ClientIp = clientIp,
            UserAgent = userAgent
        };

        _context.AuditLogs.Add(auditLog);
        await _context.SaveChangesAsync();
    }
}
```

### MediatR Behavior for Automatic Audit Logging

```csharp
public class AuditLoggingBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
{
    private readonly IAuditLogService _auditLogService;
    private readonly ILogger<AuditLoggingBehavior<TRequest, TResponse>> _logger;

    public AuditLoggingBehavior(
        IAuditLogService auditLogService,
        ILogger<AuditLoggingBehavior<TRequest, TResponse>> logger)
    {
        _auditLogService = auditLogService;
        _logger = logger;
    }

    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
    {
        // Skip logging for queries
        if (request is IQuery)
        {
            return await next();
        }

        string entityType = null;
        string entityId = null;
        string action = null;
        object oldValues = null;
        object newValues = request;

        // Determine entity type and action from request
        if (request is ICreateCommand createCommand)
        {
            entityType = createCommand.GetType().Name.Replace("CreateCommand", "");
            action = "Create";
        }
        else if (request is IUpdateCommand updateCommand)
        {
            entityType = updateCommand.GetType().Name.Replace("UpdateCommand", "");
            entityId = updateCommand.Id.ToString();
            action = "Update";
        }
        else if (request is IDeleteCommand deleteCommand)
        {
            entityType = deleteCommand.GetType().Name.Replace("DeleteCommand", "");
            entityId = deleteCommand.Id.ToString();
            action = "Delete";
        }

        if (entityType != null && action != null)
        {
            try
            {
                await _auditLogService.LogAsync(entityType, entityId, action, oldValues, newValues);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error logging audit for {EntityType} {EntityId} {Action}", entityType, entityId, action);
            }
        }

        return await next();
    }
}
```

## Historical Data Access Patterns

### Querying Temporal Tables

```csharp
public async Task<IEnumerable<Property>> GetPropertyHistoryAsync(int propertyId, DateTime? asOf = null)
{
    if (asOf.HasValue)
    {
        // Get property as of a specific point in time
        return await _context.Properties
            .TemporalAsOf(asOf.Value)
            .Where(p => p.Id == propertyId)
            .ToListAsync();
    }
    else
    {
        // Get all historical versions
        return await _context.Properties
            .TemporalAll()
            .Where(p => p.Id == propertyId)
            .OrderBy(p => EF.Property<DateTime>(p, "ValidFrom"))
            .ToListAsync();
    }
}
```

### Accessing Entity History

```csharp
public async Task<IEnumerable<PropertyHistory>> GetPropertyChangeHistoryAsync(int propertyId)
{
    return await _context.PropertyHistory
        .Where(h => h.PropertyId == propertyId)
        .OrderByDescending(h => h.ChangeDate)
        .ToListAsync();
}
```

### Querying Audit Logs

```csharp
public async Task<IEnumerable<AuditLog>> GetEntityAuditLogsAsync(string entityType, string entityId)
{
    return await _context.AuditLogs
        .Where(a => a.EntityType == entityType && a.EntityId == entityId)
        .OrderByDescending(a => a.Timestamp)
        .ToListAsync();
}

public async Task<IEnumerable<AuditLog>> GetUserAuditLogsAsync(string userId)
{
    return await _context.AuditLogs
        .Where(a => a.UserId == userId)
        .OrderByDescending(a => a.Timestamp)
        .ToListAsync();
}
```

## Data Versioning Strategies

### Snapshot Versioning

For complex entities requiring full snapshots:

```csharp
public class FinancialReportVersion
{
    public int Id { get; set; }
    public int ReportId { get; set; }
    public FinancialReport Report { get; set; }
    public int VersionNumber { get; set; }
    public string ChangedBy { get; set; }
    public DateTime ChangeDate { get; set; }
    public string ChangeReason { get; set; }
    public string PreviousContent { get; set; } // JSON serialized
    public string CurrentContent { get; set; } // JSON serialized
}
```

### Incremental Versioning

For tracking specific changes:

```csharp
public class DocumentVersion
{
    public int Id { get; set; }
    public int DocumentId { get; set; }
    public Document Document { get; set; }
    public int VersionNumber { get; set; }
    public string FileName { get; set; }
    public string StoragePath { get; set; }
    public long FileSize { get; set; }
    public string UploadedBy { get; set; }
    public DateTime UploadedAt { get; set; }
    public string ChangeNotes { get; set; }
}
```

### Version Comparison

```csharp
public class VersionComparisonService
{
    public VersionComparisonResult CompareVersions<T>(T oldVersion, T newVersion)
    {
        var result = new VersionComparisonResult();
        var properties = typeof(T).GetProperties();

        foreach (var property in properties)
        {
            var oldValue = property.GetValue(oldVersion);
            var newValue = property.GetValue(newVersion);

            if (!Equals(oldValue, newValue))
            {
                result.Changes.Add(new PropertyChange
                {
                    PropertyName = property.Name,
                    OldValue = oldValue?.ToString(),
                    NewValue = newValue?.ToString()
                });
            }
        }

        return result;
    }
}

public class VersionComparisonResult
{
    public List<PropertyChange> Changes { get; set; } = new List<PropertyChange>();
}

public class PropertyChange
{
    public string PropertyName { get; set; }
    public string OldValue { get; set; }
    public string NewValue { get; set; }
}
```

## Security and Compliance

### Audit Log Security

1. **Access Control**: Audit logs are only accessible to administrators
2. **Immutability**: Audit logs cannot be modified or deleted
3. **Encryption**: Sensitive data in audit logs is encrypted
4. **Retention Policy**: Audit logs are retained according to the retention policy

### Compliance Requirements

The audit system supports compliance with:

1. **Data Protection**: Tracking who accessed and modified personal data
2. **Financial Accountability**: Tracking changes to financial records
3. **Operational Transparency**: Providing a complete history of system changes

### Sensitive Data Handling

1. **Data Masking**: Sensitive data is masked in audit logs
2. **Role-Based Access**: Access to historical data is restricted based on user roles
3. **Purpose Limitation**: Historical data is only used for legitimate purposes

## Implementation Checklist

- [ ] **Base Entity Classes**
  - [ ] Create AuditableEntity base class
  - [ ] Create SoftDeleteEntity base class
  - [ ] Apply base classes to domain entities

- [ ] **Audit Interceptor**
  - [ ] Implement SaveChangesInterceptor
  - [ ] Configure automatic audit field population
  - [ ] Register interceptor in DI container

- [ ] **History Tables**
  - [ ] Create history entities for key domain entities
  - [ ] Configure temporal tables in Entity Framework
  - [ ] Implement history repositories

- [ ] **Audit Logging**
  - [ ] Create AuditLog entity
  - [ ] Implement AuditLogService
  - [ ] Configure MediatR behavior for audit logging
  - [ ] Implement audit log queries

- [ ] **Data Versioning**
  - [ ] Implement version entities for complex domain objects
  - [ ] Create version comparison service
  - [ ] Implement version management APIs

- [ ] **Security**
  - [ ] Configure access control for audit data
  - [ ] Implement data masking for sensitive information
  - [ ] Configure audit log retention policy

## Best Practices

1. **Performance Considerations**
   - Use appropriate indexing on history tables
   - Consider the performance impact of audit logging
   - Use asynchronous logging where possible

2. **Storage Management**
   - Implement data archiving for older audit logs
   - Consider partitioning for large audit tables
   - Monitor storage growth and plan accordingly

3. **Query Optimization**
   - Create optimized views for common audit queries
   - Use appropriate filtering in audit log queries
   - Consider caching for frequently accessed audit data

4. **Maintenance**
   - Regularly review and clean up audit logs
   - Monitor audit log performance
   - Periodically validate audit log integrity
