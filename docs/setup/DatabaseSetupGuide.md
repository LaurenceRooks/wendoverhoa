# Database Setup Guide

This guide provides detailed instructions for setting up and configuring the Microsoft SQL Server database for local development of the Wendover HOA application. It covers installation, configuration, initial migration application, and seed data creation.

## SQL Server Installation

### Windows Installation

1. **Download SQL Server 2022**
   - Go to [Microsoft SQL Server Downloads](https://www.microsoft.com/en-us/sql-server/sql-server-downloads)
   - Download SQL Server 2022 Developer Edition (free for development use)

2. **Run the Installer**
   - Launch the downloaded executable
   - Select "Basic" installation for simplicity
   - Accept the license terms
   - Choose an installation location
   - Click "Install"

3. **Install SQL Server Management Studio (SSMS)**
   - After SQL Server installation completes, you'll be prompted to install SSMS
   - Click "Install SSMS" which will redirect you to the download page
   - Download and install SSMS

4. **Verify Installation**
   - Launch SQL Server Management Studio
   - Connect to your local instance (typically `localhost` or `(localdb)\MSSQLLocalDB`)
   - Verify you can create a new database

### macOS/Linux Installation (Using Docker)

1. **Install Docker**
   - Download and install [Docker Desktop](https://www.docker.com/products/docker-desktop)
   - Start Docker Desktop

2. **Pull and Run SQL Server Container**
   ```bash
   # Pull the SQL Server 2022 image
   docker pull mcr.microsoft.com/mssql/server:2022-latest

   # Run the container
   docker run -e "ACCEPT_EULA=Y" -e "MSSQL_SA_PASSWORD=YourStrongPassword!" \
      -p 1433:1433 --name wendoverhoa-sqlserver \
      -v wendoverhoa-sqlvolume:/var/opt/mssql \
      -d mcr.microsoft.com/mssql/server:2022-latest
   ```

3. **Install Azure Data Studio**
   - Download [Azure Data Studio](https://docs.microsoft.com/en-us/sql/azure-data-studio/download-azure-data-studio)
   - Install and launch Azure Data Studio
   - Connect to your SQL Server container:
     - Server: `localhost,1433`
     - Authentication type: SQL Login
     - User name: `sa`
     - Password: `YourStrongPassword!`

4. **Verify Installation**
   - Create a new query and run:
     ```sql
     SELECT @@VERSION
     ```
   - Verify you can create a new database

## Database Configuration

### Creating the Database

#### Using SQL Server Management Studio (Windows)

1. Open SQL Server Management Studio
2. Connect to your SQL Server instance
3. Right-click on "Databases" and select "New Database"
4. Enter "WendoverHOA" as the database name
5. Click "OK" to create the database

#### Using Azure Data Studio (macOS/Linux)

1. Open Azure Data Studio
2. Connect to your SQL Server instance
3. Create a new query and run:
   ```sql
   CREATE DATABASE WendoverHOA;
   GO
   ```

### Configuring Connection String

Update your connection string in `appsettings.Development.json`:

#### For Windows (SQL Server Authentication)

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost;Database=WendoverHOA;User Id=sa;Password=YourStrongPassword!;TrustServerCertificate=True;MultipleActiveResultSets=true"
  }
}
```

#### For Windows (Windows Authentication)

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost;Database=WendoverHOA;Trusted_Connection=True;TrustServerCertificate=True;MultipleActiveResultSets=true"
  }
}
```

#### For Windows (LocalDB)

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=(localdb)\\mssqllocaldb;Database=WendoverHOA;Trusted_Connection=True;MultipleActiveResultSets=true"
  }
}
```

#### For macOS/Linux (Docker)

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost,1433;Database=WendoverHOA;User Id=sa;Password=YourStrongPassword!;TrustServerCertificate=True;MultipleActiveResultSets=true"
  }
}
```

## Entity Framework Migrations

### Installing EF Core Tools

If you haven't already installed the Entity Framework Core tools:

```bash
dotnet tool install --global dotnet-ef
```

Or update to the latest version:

```bash
dotnet tool update --global dotnet-ef
```

### Applying Initial Migrations

To create and apply the initial database schema:

```bash
# Navigate to the project root directory
cd /path/to/wendoverhoa

# Apply migrations
dotnet ef database update --project src/WendoverHOA.Infrastructure --startup-project src/WendoverHOA.Web
```

### Creating New Migrations

When you make changes to the data model, you need to create a new migration:

```bash
dotnet ef migrations add MigrationName --project src/WendoverHOA.Infrastructure --startup-project src/WendoverHOA.Web
```

Replace `MigrationName` with a descriptive name for your changes (e.g., `AddPropertyTable`).

### Viewing Pending Migrations

To see which migrations haven't been applied to the database:

```bash
dotnet ef migrations list --project src/WendoverHOA.Infrastructure --startup-project src/WendoverHOA.Web
```

### Rolling Back Migrations

To roll back to a specific migration:

```bash
dotnet ef database update MigrationName --project src/WendoverHOA.Infrastructure --startup-project src/WendoverHOA.Web
```

Where `MigrationName` is the name of the migration you want to roll back to.

## Seed Data

### Understanding Seed Data

Seed data provides initial records for the database to ensure the application has necessary data to function correctly. In the Wendover HOA application, we use seed data for:

1. Default user roles
2. Admin user account
3. Basic configuration settings
4. Initial expense categories
5. Test properties for development

### Applying Seed Data

The application automatically applies seed data when you run migrations. The seed data is defined in:

- `src/WendoverHOA.Infrastructure/Persistence/Seed/ApplicationDbContextSeed.cs`

To manually trigger seed data application:

```bash
# Run the application with the SEED_DATA environment variable
ASPNETCORE_ENVIRONMENT=Development SEED_DATA=true dotnet run --project src/WendoverHOA.Web
```

### Customizing Seed Data

To modify or add seed data:

1. Open `src/WendoverHOA.Infrastructure/Persistence/Seed/ApplicationDbContextSeed.cs`
2. Modify the seed methods for the relevant entities
3. Run the application with the `SEED_DATA` environment variable set to `true`

### Default Admin Credentials

The default administrator account created by the seed data:

- **Email**: admin@wendoverhoa.org
- **Password**: Admin123!

**Important**: Change these credentials immediately in a production environment.

## Database Maintenance

### Backup and Restore

#### Creating a Backup

Using SQL Server Management Studio:
1. Right-click the WendoverHOA database
2. Select Tasks → Backup
3. Choose a backup location
4. Click OK

Using T-SQL:
```sql
BACKUP DATABASE WendoverHOA
TO DISK = 'C:\Backups\WendoverHOA.bak'
WITH FORMAT, COMPRESSION;
```

#### Restoring a Backup

Using SQL Server Management Studio:
1. Right-click on Databases
2. Select Restore Database
3. Choose the backup file
4. Click OK

Using T-SQL:
```sql
RESTORE DATABASE WendoverHOA
FROM DISK = 'C:\Backups\WendoverHOA.bak'
WITH REPLACE;
```

### Database Indexing

The application's Entity Framework configurations include appropriate indexes for performance. To view existing indexes:

```sql
SELECT 
    i.name AS IndexName,
    OBJECT_NAME(ic.OBJECT_ID) AS TableName,
    COL_NAME(ic.OBJECT_ID,ic.column_id) AS ColumnName
FROM sys.indexes AS i
INNER JOIN sys.index_columns AS ic ON i.OBJECT_ID = ic.OBJECT_ID AND i.index_id = ic.index_id
WHERE OBJECT_NAME(ic.OBJECT_ID) LIKE 'Wendover%'
ORDER BY TableName, IndexName, ic.key_ordinal;
```

## Troubleshooting

### Common Database Issues

#### Cannot Connect to Database

1. Verify SQL Server is running
2. Check connection string in `appsettings.Development.json`
3. Ensure firewall allows SQL Server connections
4. Verify SQL Server authentication mode (Windows/Mixed)

#### Migration Errors

1. Ensure you're using the correct EF Core version
2. Check for pending changes that conflict with migrations
3. Try removing the last migration if it hasn't been applied:
   ```bash
   dotnet ef migrations remove --project src/WendoverHOA.Infrastructure --startup-project src/WendoverHOA.Web
   ```

#### Performance Issues

1. Check for missing indexes
2. Verify that statistics are up-to-date
3. Look for long-running queries
4. Ensure the database has enough resources

### SQL Server Logs

To view SQL Server logs:

1. In SQL Server Management Studio, expand Management
2. Open SQL Server Logs
3. Review logs for errors or warnings

## Security Best Practices

1. **Never commit connection strings with passwords** to source control
2. **Use Windows Authentication** when possible (more secure than SQL authentication)
3. **Regularly update SQL Server** with security patches
4. **Use the principle of least privilege** for database users
5. **Enable TLS encryption** for database connections
6. **Regularly audit database access** in production environments
7. **Use parameterized queries** to prevent SQL injection
8. **Encrypt sensitive data** in the database
