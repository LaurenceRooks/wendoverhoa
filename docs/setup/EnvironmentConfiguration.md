# Environment Configuration Guide

This guide explains how to configure different environments for the Wendover HOA application, including development, staging, and production settings. It covers configuration file structure, secret management, and environment-specific settings.

## Configuration Overview

The Wendover HOA application uses ASP.NET Core's configuration system, which provides a flexible way to handle settings across different environments. The configuration is built from multiple sources in a specific order, with later sources overriding earlier ones:

1. Default settings in code
2. `appsettings.json` (base settings)
3. `appsettings.{Environment}.json` (environment-specific settings)
4. User secrets (development only)
5. Environment variables
6. Command-line arguments

## Configuration File Structure

### Base Configuration

The `appsettings.json` file contains base settings that apply to all environments:

```json
{
  "AllowedHosts": "*",
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning"
    }
  },
  "ApplicationSettings": {
    "ApplicationName": "Wendover HOA",
    "SupportEmail": "support@wendoverhoa.org",
    "EnabledFeatures": {
      "Announcements": true,
      "CommunityCalendar": true,
      "DocumentRepository": true,
      "Directory": true,
      "BoardManagement": true,
      "MeetingMinutes": true,
      "UserFeedback": true,
      "VendorSuggestions": true,
      "DuesTracking": true,
      "PaymentProcessing": true,
      "FinancialReporting": true,
      "ExpenseTracking": true
    }
  },
  "Caching": {
    "DefaultExpirationMinutes": 60
  },
  "Cors": {
    "AllowedOrigins": []
  }
}
```

### Environment-Specific Configuration

Environment-specific settings are stored in `appsettings.{Environment}.json` files:

#### Development Environment (`appsettings.Development.json`)

```json
{
  "DetailedErrors": true,
  "Logging": {
    "LogLevel": {
      "Default": "Debug",
      "Microsoft.AspNetCore": "Information",
      "Microsoft.EntityFrameworkCore.Database.Command": "Information"
    }
  },
  "ConnectionStrings": {
    "DefaultConnection": "Server=(localdb)\\mssqllocaldb;Database=WendoverHOA;Trusted_Connection=True;MultipleActiveResultSets=true"
  },
  "Email": {
    "SendGridApiKey": "use-user-secrets",
    "FromEmail": "dev@wendoverhoa.org",
    "FromName": "Wendover HOA (Development)"
  },
  "Authentication": {
    "Google": {
      "ClientId": "use-user-secrets",
      "ClientSecret": "use-user-secrets"
    },
    "Microsoft": {
      "ClientId": "use-user-secrets",
      "ClientSecret": "use-user-secrets"
    },
    "Apple": {
      "ClientId": "use-user-secrets",
      "ClientSecret": "use-user-secrets"
    }
  },
  "FeatureFlags": {
    "EnableSwagger": true,
    "EnableDeveloperExceptionPage": true,
    "UseInMemoryDatabase": false
  }
}
```

#### Staging Environment (`appsettings.Staging.json`)

```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning"
    }
  },
  "ConnectionStrings": {
    "DefaultConnection": "Server=db-server;Database=WendoverHOA;User Id=wendover_app;Password=use-environment-variables;TrustServerCertificate=True;MultipleActiveResultSets=true"
  },
  "Email": {
    "FromEmail": "no-reply@dev.wendoverhoa.org",
    "FromName": "Wendover HOA (Staging)"
  },
  "FeatureFlags": {
    "EnableSwagger": true,
    "EnableDeveloperExceptionPage": false,
    "UseInMemoryDatabase": false
  }
}
```

#### Production Environment (`appsettings.Production.json`)

```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Warning",
      "Microsoft.AspNetCore": "Error"
    }
  },
  "ConnectionStrings": {
    "DefaultConnection": "Server=db-server;Database=WendoverHOA;User Id=wendover_app;Password=use-environment-variables;TrustServerCertificate=True;MultipleActiveResultSets=true"
  },
  "Email": {
    "FromEmail": "no-reply@wendoverhoa.org",
    "FromName": "Wendover HOA"
  },
  "FeatureFlags": {
    "EnableSwagger": false,
    "EnableDeveloperExceptionPage": false,
    "UseInMemoryDatabase": false
  },
  "Cors": {
    "AllowedOrigins": [
      "https://wendoverhoa.org"
    ]
  }
}
```

## Secret Management

### Development Environment

For local development, use the .NET Secret Manager tool to store sensitive information:

```bash
# Initialize user secrets
dotnet user-secrets init --project src/WendoverHOA.Web

# Add a secret
dotnet user-secrets set "Email:SendGridApiKey" "your-sendgrid-api-key" --project src/WendoverHOA.Web
dotnet user-secrets set "Authentication:Google:ClientId" "your-google-client-id" --project src/WendoverHOA.Web
dotnet user-secrets set "Authentication:Google:ClientSecret" "your-google-client-secret" --project src/WendoverHOA.Web

# List all secrets
dotnet user-secrets list --project src/WendoverHOA.Web

# Remove a secret
dotnet user-secrets remove "Email:SendGridApiKey" --project src/WendoverHOA.Web

# Clear all secrets
dotnet user-secrets clear --project src/WendoverHOA.Web
```

User secrets are stored in:
- Windows: `%APPDATA%\Microsoft\UserSecrets\<user_secrets_id>\secrets.json`
- macOS/Linux: `~/.microsoft/usersecrets/<user_secrets_id>/secrets.json`

### Staging and Production Environments

For staging and production environments, use environment variables or a secure vault service:

#### Using Environment Variables

Environment variables override settings in configuration files. Set them in your deployment environment:

```bash
# Windows PowerShell
$Env:ConnectionStrings__DefaultConnection = "Server=production-db;Database=WendoverHOA;User Id=app_user;Password=strong-password;TrustServerCertificate=True"
$Env:Email__SendGridApiKey = "your-production-sendgrid-key"

# Linux/macOS
export ConnectionStrings__DefaultConnection="Server=production-db;Database=WendoverHOA;User Id=app_user;Password=strong-password;TrustServerCertificate=True"
export Email__SendGridApiKey="your-production-sendgrid-key"
```

In GitHub Actions, use GitHub Secrets to store sensitive values and inject them as environment variables during deployment.

#### Using Azure Key Vault (Recommended for Production)

For production, consider using Azure Key Vault to store secrets:

1. Create an Azure Key Vault
2. Add your secrets to the vault
3. Configure your application to access the Key Vault
4. Install the required NuGet package:

```bash
dotnet add package Azure.Extensions.AspNetCore.Configuration.Secrets
```

5. Configure Key Vault in `Program.cs`:

```csharp
if (builder.Environment.IsProduction())
{
    var keyVaultName = builder.Configuration["KeyVaultName"];
    var keyVaultUri = new Uri($"https://{keyVaultName}.vault.azure.net/");
    builder.Configuration.AddAzureKeyVault(keyVaultUri, new DefaultAzureCredential());
}
```

## Environment-Specific Settings

### Setting the Environment

#### Development

The environment is automatically set to `Development` in Visual Studio and Visual Studio Code.

To manually set the environment:

```bash
# Windows PowerShell
$Env:ASPNETCORE_ENVIRONMENT = "Development"

# macOS/Linux
export ASPNETCORE_ENVIRONMENT="Development"
```

#### Staging and Production

In staging and production environments, set the environment variable in your hosting platform:

```bash
ASPNETCORE_ENVIRONMENT=Production
```

In GitHub Actions, set the environment in your workflow file:

```yaml
env:
  ASPNETCORE_ENVIRONMENT: Production
```

### Feature Flags

Feature flags allow you to enable or disable features in different environments:

```json
"FeatureFlags": {
  "EnableSwagger": true,
  "EnableBetaFeatures": false,
  "UseInMemoryDatabase": false
}
```

Access feature flags in code:

```csharp
if (configuration.GetValue<bool>("FeatureFlags:EnableSwagger"))
{
    app.UseSwagger();
    app.UseSwaggerUI();
}
```

## Configuration in Code

### Strongly Typed Configuration

Use strongly typed configuration classes to access settings:

```csharp
// Create a configuration class
public class ApplicationSettings
{
    public string ApplicationName { get; set; }
    public string SupportEmail { get; set; }
    public EnabledFeaturesSettings EnabledFeatures { get; set; }
}

public class EnabledFeaturesSettings
{
    public bool Announcements { get; set; }
    public bool CommunityCalendar { get; set; }
    // Other features...
}

// Register in Program.cs
builder.Services.Configure<ApplicationSettings>(
    builder.Configuration.GetSection("ApplicationSettings"));

// Inject and use in a service
public class SomeService
{
    private readonly ApplicationSettings _settings;

    public SomeService(IOptions<ApplicationSettings> options)
    {
        _settings = options.Value;
    }

    public void DoSomething()
    {
        var appName = _settings.ApplicationName;
        // Use settings...
    }
}
```

### Reloading Configuration

To support configuration reloading without application restart:

```csharp
// Register in Program.cs
builder.Services.Configure<ApplicationSettings>(
    builder.Configuration.GetSection("ApplicationSettings"));

// Inject and use in a service
public class SomeService
{
    private readonly IOptionsMonitor<ApplicationSettings> _settings;

    public SomeService(IOptionsMonitor<ApplicationSettings> options)
    {
        _settings = options;
        _settings.OnChange(settings => {
            // Handle configuration changes
        });
    }

    public void DoSomething()
    {
        var appName = _settings.CurrentValue.ApplicationName;
        // Use settings...
    }
}
```

## Environment-Specific Code

Use environment checks to run code only in specific environments:

```csharp
if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
    app.UseSwagger();
    app.UseSwaggerUI();
}
else
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}
```

## Configuration Best Practices

1. **Never store secrets in configuration files** that are committed to source control
2. **Use environment variables or secret management** for sensitive information
3. **Follow the principle of least privilege** when configuring database connections
4. **Use strongly typed configuration** to catch configuration errors at compile time
5. **Validate configuration** at application startup
6. **Document all configuration options** for other developers
7. **Use different settings for different environments** to match their requirements
8. **Keep production configuration simple** to reduce the risk of misconfiguration
9. **Regularly audit configuration** for security issues
10. **Use feature flags** to enable/disable features in different environments

## Troubleshooting

### Common Configuration Issues

1. **Missing configuration**: Check that the configuration file exists and is correctly named
2. **Configuration not loading**: Verify the configuration provider order
3. **Environment not set correctly**: Check the `ASPNETCORE_ENVIRONMENT` variable
4. **Secrets not available**: Ensure secrets are correctly set in the appropriate store
5. **Connection string issues**: Verify the connection string format and credentials

### Debugging Configuration

To debug configuration issues:

```csharp
// In Program.cs or Startup.cs
if (app.Environment.IsDevelopment())
{
    var configDebug = ((IConfigurationRoot)builder.Configuration).GetDebugView();
    System.IO.File.WriteAllText("config-debug.txt", configDebug);
}
```

This will output all configuration values to a file for inspection.
