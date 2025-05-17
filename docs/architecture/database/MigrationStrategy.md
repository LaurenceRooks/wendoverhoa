# Migration Strategy Documentation

This document outlines the migration strategy for the Wendover HOA application's database, detailing the process for creating, managing, and deploying database schema changes. Following these guidelines ensures consistent and reliable database evolution throughout the application lifecycle.

## Migration Creation Process

### Overview

The Wendover HOA application uses Entity Framework Core migrations to manage database schema changes. Migrations provide a way to incrementally update the database schema to keep it in sync with the application's data model while preserving existing data.

### Prerequisites

Before creating migrations, ensure:

1. The Entity Framework Core tools are installed:
   ```bash
   dotnet tool install --global dotnet-ef
   ```

2. Your model changes are complete and properly configured in the entity configuration classes
3. The application builds successfully
4. You have appropriate database access permissions

### Creating a New Migration

#### Step 1: Make Model Changes

1. Update entity classes in the Domain layer
2. Update entity configurations in the Infrastructure layer
3. Ensure all relationships are properly configured
4. Build the solution to verify there are no compilation errors

#### Step 2: Generate the Migration

Run the following command from the solution root directory:

```bash
dotnet ef migrations add MigrationName --project src/WendoverHOA.Infrastructure --startup-project src/WendoverHOA.Web
```

Replace `MigrationName` with a descriptive name that summarizes the changes (e.g., `AddPropertyFeatures`, `UpdateResidentFields`, `CreateExpenseEntities`).

#### Step 3: Review the Generated Migration

Carefully review the generated migration files:

1. **Migration class** (e.g., `20250513_AddPropertyFeatures.cs`): Contains `Up()` and `Down()` methods defining the schema changes
2. **Migration designer file** (e.g., `20250513_AddPropertyFeatures.Designer.cs`): Contains migration metadata
3. **DbContext model snapshot** (e.g., `ApplicationDbContextModelSnapshot.cs`): Contains the current state of the model

Check for:
- Correct table and column names
- Proper data types and constraints
- Appropriate indexes
- Correct foreign key relationships
- Data loss concerns

#### Step 4: Customize the Migration (if needed)

If the generated migration doesn't fully meet your requirements, you can customize it:

```csharp
public partial class AddPropertyFeatures : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        // Generated code...
        
        // Custom SQL for complex changes
        migrationBuilder.Sql(@"
            UPDATE Properties 
            SET SquareFootage = 2000 
            WHERE SquareFootage IS NULL
        ");
        
        // Add additional indexes
        migrationBuilder.CreateIndex(
            name: "IX_Properties_YearBuilt",
            table: "Properties",
            column: "YearBuilt");
    }
    
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        // Generated code...
        
        // Custom SQL for reverting changes
        migrationBuilder.Sql(@"
            -- Revert custom changes
        ");
        
        // Drop additional indexes
        migrationBuilder.DropIndex(
            name: "IX_Properties_YearBuilt",
            table: "Properties");
    }
}
```

#### Step 5: Test the Migration

Test the migration in a development environment:

```bash
dotnet ef database update --project src/WendoverHOA.Infrastructure --startup-project src/WendoverHOA.Web
```

Verify:
- The migration applies successfully
- The database schema matches your expectations
- Existing data is preserved
- The application works correctly with the updated schema

### Handling Special Migration Scenarios

#### Data Migrations

For migrations that need to modify data:

```csharp
protected override void Up(MigrationBuilder migrationBuilder)
{
    // Schema changes
    migrationBuilder.AddColumn<string>(
        name: "FullName",
        table: "Residents",
        type: "nvarchar(200)",
        nullable: true);
    
    // Data migration
    migrationBuilder.Sql(@"
        UPDATE Residents
        SET FullName = FirstName + ' ' + LastName
        WHERE FullName IS NULL
    ");
    
    // Make column required after populating data
    migrationBuilder.AlterColumn<string>(
        name: "FullName",
        table: "Residents",
        type: "nvarchar(200)",
        nullable: false,
        oldClrType: typeof(string),
        oldType: "nvarchar(200)",
        oldNullable: true);
}
```

#### Renaming Columns or Tables

Use the `RenameColumn` and `RenameTable` methods instead of dropping and recreating:

```csharp
protected override void Up(MigrationBuilder migrationBuilder)
{
    migrationBuilder.RenameColumn(
        name: "Address",
        table: "Properties",
        newName: "StreetAddress");
        
    migrationBuilder.RenameTable(
        name: "HOADues",
        newName: "DuesTransactions");
}
```

#### Adding Computed Columns

```csharp
protected override void Up(MigrationBuilder migrationBuilder)
{
    migrationBuilder.AddColumn<decimal>(
        name: "TotalDue",
        table: "DuesTransactions",
        type: "decimal(18,2)",
        computedColumnSql: "[Amount] + [LateFees]",
        stored: true);
}
```

## Versioning Approach

### Migration Naming Convention

Migrations follow a consistent naming convention:

```
{Timestamp}_{PurposeOfMigration}
```

Examples:
- `20250513143022_InitialCreate`
- `20250514091534_AddPropertyFeatures`
- `20250515162145_UpdateResidentFields`
- `20250520103012_CreateExpenseEntities`

### Migration Grouping

Migrations are grouped by feature or related changes:

1. **Core Domain Migrations**: Changes to core domain entities
2. **Feature-Specific Migrations**: Changes related to specific features
3. **Cross-Cutting Migrations**: Changes that affect multiple parts of the system
4. **Performance Migrations**: Changes focused on performance improvements (e.g., adding indexes)
5. **Data Migrations**: Changes that primarily modify data rather than schema

### Version Control Integration

Migrations are committed to version control with the following guidelines:

1. Include both the migration files and the updated model snapshot
2. Group migrations with their corresponding model changes in the same commit
3. Include detailed commit messages explaining the purpose of the migration
4. For complex migrations, include additional documentation in the commit message

### Migration History

The migration history is maintained in the `__EFMigrationsHistory` table in the database. This table tracks:

1. The migration ID (combination of timestamp and name)
2. The product version (Entity Framework Core version)
3. The timestamp when the migration was applied

## Deployment Considerations

### Development to Production Workflow

The migration deployment workflow follows these stages:

1. **Development**: Migrations are created and tested locally
2. **Integration**: Migrations are applied to the integration environment and tested with integrated components
3. **Staging**: Migrations are applied to the staging environment, which mirrors production
4. **Production**: Migrations are applied to the production environment during a scheduled maintenance window

### Automated Deployment

Migrations are deployed automatically as part of the CI/CD pipeline:

```yaml
# GitHub Actions workflow excerpt
jobs:
  deploy-database:
    runs-on: ubuntu-latest
    steps:
      - uses: actions/checkout@v3
      
      - name: Setup .NET
        uses: actions/setup-dotnet@v3
        with:
          dotnet-version: '9.0.x'
          
      - name: Apply database migrations
        run: |
          dotnet ef database update --project src/WendoverHOA.Infrastructure --startup-project src/WendoverHOA.Web --connection "${{ secrets.CONNECTION_STRING }}"
```

### Rollback Strategy

If a migration causes issues in production, the following rollback strategy is implemented:

1. **Immediate Rollback**: Revert to the previous migration
   ```bash
   dotnet ef database update PreviousMigrationName --project src/WendoverHOA.Infrastructure --startup-project src/WendoverHOA.Web
   ```

2. **Fix and Redeploy**: Create a new migration that fixes the issues and deploy it

3. **Data Recovery**: If data loss occurred, restore from the most recent backup

### Database Backup Before Migration

Before applying migrations to production:

1. Create a full database backup
2. Verify the backup is valid and can be restored
3. Store the backup in a secure location with appropriate retention policy

### Handling Long-Running Migrations

For migrations that may take a long time to apply:

1. **Staged Migrations**: Break large changes into smaller, incremental migrations
2. **Off-Peak Deployment**: Schedule migration deployment during off-peak hours
3. **Online Schema Changes**: Use techniques that minimize downtime (e.g., adding nullable columns first, then populating data)
4. **Monitoring**: Implement monitoring to track migration progress and performance impact

## Migration Testing

### Pre-Deployment Testing

Before deploying migrations to production:

1. **Local Testing**: Apply migrations to a local development database
2. **Integration Testing**: Apply migrations to an integration environment and run integration tests
3. **Performance Testing**: Measure the time taken to apply migrations and their impact on database performance
4. **Data Integrity Testing**: Verify that existing data is preserved and remains consistent

### Migration Verification Script

Use a verification script to validate migrations:

```csharp
public static class MigrationVerifier
{
    public static async Task VerifyMigrationsAsync(string connectionString)
    {
        // Create a new DbContext with the connection string
        var optionsBuilder = new DbContextOptionsBuilder<ApplicationDbContext>();
        optionsBuilder.UseSqlServer(connectionString);
        
        using var context = new ApplicationDbContext(optionsBuilder.Options);
        
        // Check if there are pending migrations
        var pendingMigrations = await context.Database.GetPendingMigrationsAsync();
        if (pendingMigrations.Any())
        {
            Console.WriteLine("Pending migrations:");
            foreach (var migration in pendingMigrations)
            {
                Console.WriteLine($"  {migration}");
            }
        }
        else
        {
            Console.WriteLine("No pending migrations.");
        }
        
        // Check if the current model is compatible with the database
        var modelDifferences = context.GetService<IMigrationsModelDiffer>()
            .GetDifferences(
                context.GetService<IDesignTimeModel>().Model,
                context.GetService<IRelationalModel>());
            
        if (modelDifferences.Any())
        {
            Console.WriteLine("Model differences detected:");
            // Log differences
        }
        else
        {
            Console.WriteLine("Model is compatible with the database.");
        }
    }
}
```

## Data Migration Considerations

### Handling Large Tables

For migrations involving large tables:

1. **Batched Updates**: Process data in smaller batches to reduce lock contention
   ```csharp
   migrationBuilder.Sql(@"
       DECLARE @batchSize INT = 1000;
       DECLARE @offset INT = 0;
       
       WHILE EXISTS (SELECT 1 FROM Properties WHERE NewField IS NULL OFFSET @offset ROWS FETCH NEXT @batchSize ROWS ONLY)
       BEGIN
           UPDATE p
           SET p.NewField = 'DefaultValue'
           FROM Properties p
           WHERE p.NewField IS NULL
           ORDER BY p.Id
           OFFSET @offset ROWS FETCH NEXT @batchSize ROWS ONLY;
           
           SET @offset = @offset + @batchSize;
       END
   ");
   ```

2. **Temporary Tables**: Use temporary tables for complex data transformations
3. **Parallel Processing**: Use parallel processing for independent data updates
4. **Deferred Constraints**: Disable constraints during migration and re-enable afterward

### Data Validation

Include data validation in migrations:

```csharp
migrationBuilder.Sql(@"
    -- Validate data before migration
    IF EXISTS (SELECT 1 FROM Properties WHERE Address IS NULL)
    BEGIN
        THROW 51000, 'Cannot proceed with migration: Some properties have NULL addresses', 1;
    END
    
    -- Proceed with migration if validation passes
    UPDATE Properties
    SET AddressNormalized = UPPER(Address)
    WHERE AddressNormalized IS NULL;
");
```

## Monitoring and Maintenance

### Migration Logging

Implement comprehensive logging for migrations:

```csharp
public class MigrationLogger : IMigrationLogger
{
    private readonly ILogger<MigrationLogger> _logger;
    
    public MigrationLogger(ILogger<MigrationLogger> logger)
    {
        _logger = logger;
    }
    
    public void LogMigrationStarted(string migrationId, string databaseName)
    {
        _logger.LogInformation("Starting migration {MigrationId} on database {DatabaseName}", migrationId, databaseName);
    }
    
    public void LogMigrationCompleted(string migrationId, string databaseName, TimeSpan duration)
    {
        _logger.LogInformation("Completed migration {MigrationId} on database {DatabaseName} in {Duration}", migrationId, databaseName, duration);
    }
    
    public void LogMigrationFailed(string migrationId, string databaseName, Exception exception)
    {
        _logger.LogError(exception, "Failed migration {MigrationId} on database {DatabaseName}", migrationId, databaseName);
    }
}
```

### Performance Monitoring

Monitor database performance before and after migrations:

1. **Query Performance**: Track execution time of key queries
2. **Index Usage**: Monitor index usage statistics
3. **Wait Statistics**: Monitor SQL Server wait statistics
4. **Resource Utilization**: Track CPU, memory, and I/O utilization

### Regular Maintenance

Implement regular database maintenance tasks:

1. **Index Maintenance**: Rebuild or reorganize fragmented indexes
2. **Statistics Updates**: Update statistics for optimal query plans
3. **Database File Management**: Monitor and manage database file growth
4. **Backup Verification**: Regularly test database backups

## Best Practices

### General Migration Best Practices

1. **Keep migrations small and focused**: Each migration should make a specific, well-defined change
2. **Test migrations thoroughly**: Test all migrations in development and staging environments before deploying to production
3. **Document complex migrations**: Include comments in migration code to explain complex changes
4. **Include both Up and Down methods**: Ensure both methods are properly implemented for bidirectional migration
5. **Use transactions**: Wrap migrations in transactions to ensure atomicity
6. **Consider performance impact**: Evaluate the performance impact of migrations, especially on large tables
7. **Maintain backward compatibility**: Design migrations to maintain backward compatibility with existing code when possible
8. **Review generated SQL**: Use `Script-Migration` to review the SQL that will be executed
9. **Version control**: Commit migrations to version control along with application code changes
10. **Coordinate with application deployment**: Ensure database migrations are coordinated with application deployments

### Avoiding Common Migration Pitfalls

1. **Avoid data loss**: Be careful with column drops or type changes that might cause data loss
2. **Avoid long-running transactions**: Break large changes into smaller, manageable migrations
3. **Avoid schema locks**: Consider the impact of schema locks on system availability
4. **Avoid direct table manipulation**: Use Entity Framework migrations rather than direct SQL when possible
5. **Avoid migration divergence**: Ensure all environments follow the same migration path
6. **Avoid circular dependencies**: Design migrations to avoid circular dependencies between tables
7. **Avoid inconsistent naming**: Follow consistent naming conventions for database objects
