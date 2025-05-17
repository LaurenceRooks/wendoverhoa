# Dependency Management Strategy

This document outlines the comprehensive dependency management strategy for the Wendover HOA application, ensuring consistent, secure, and maintainable dependencies across all components of the application in alignment with Clean Architecture principles.

## Dependency Management Principles

The Wendover HOA application follows these core dependency management principles:

1. **Explicit Dependencies**: All dependencies are explicitly declared and versioned
2. **Minimal Dependencies**: Only include dependencies that provide significant value
3. **Secure Dependencies**: Regularly scan and update dependencies for security vulnerabilities
4. **Stable Dependencies**: Prefer stable, well-maintained dependencies with active communities
5. **Compatible Dependencies**: Ensure all dependencies are compatible with each other
6. **Isolated Dependencies**: Minimize dependency impact through proper architecture
7. **Documented Dependencies**: Document all dependencies and their purposes

## NuGet Package Management

### Package Sources

The application uses the following NuGet package sources:

1. **Primary Source**: nuget.org (https://api.nuget.org/v3/index.json)
2. **Internal Source**: Azure DevOps Artifacts (for internal shared packages)

```xml
<!-- NuGet.config -->
<?xml version="1.0" encoding="utf-8"?>
<configuration>
  <packageSources>
    <clear />
    <add key="nuget.org" value="https://api.nuget.org/v3/index.json" protocolVersion="3" />
    <add key="WendoverHOA" value="https://pkgs.dev.azure.com/WendoverHOA/_packaging/WendoverHOA/nuget/v3/index.json" />
  </packageSources>
</configuration>
```

### Package Versioning

The application uses semantic versioning (SemVer) for all dependencies:

- **Major Version**: Incompatible API changes
- **Minor Version**: Functionality added in a backward-compatible manner
- **Patch Version**: Backward-compatible bug fixes

```xml
<!-- Example of versioning in .csproj file -->
<PackageReference Include="Microsoft.AspNetCore.Authentication.JwtBearer" Version="9.0.5" />
```

### Version Pinning Strategy

The application follows these version pinning strategies:

1. **Runtime and Framework**: Pin to exact version (9.0.5)
2. **Major Libraries**: Pin to minor version (9.0.*)
3. **Utility Libraries**: Pin to major version (9.*)
4. **Development Dependencies**: Pin to minor version (9.0.*)

```xml
<!-- Example of version pinning in .csproj file -->
<PackageReference Include="Microsoft.AspNetCore.App" Version="9.0.5" /> <!-- Exact version -->
<PackageReference Include="Microsoft.EntityFrameworkCore" Version="9.0.*" /> <!-- Minor version -->
<PackageReference Include="Newtonsoft.Json" Version="13.*" /> <!-- Major version -->
```

### Central Package Management

The application uses .NET's Central Package Management to ensure consistent versions across all projects:

```xml
<!-- Directory.Packages.props -->
<Project>
  <PropertyGroup>
    <ManagePackageVersionsCentrally>true</ManagePackageVersionsCentrally>
    <CentralPackageTransitivePinningEnabled>true</CentralPackageTransitivePinningEnabled>
  </PropertyGroup>
  <ItemGroup>
    <!-- ASP.NET Core -->
    <PackageVersion Include="Microsoft.AspNetCore.Authentication.JwtBearer" Version="9.0.5" />
    <PackageVersion Include="Microsoft.AspNetCore.Identity.EntityFrameworkCore" Version="9.0.5" />
    
    <!-- Entity Framework Core -->
    <PackageVersion Include="Microsoft.EntityFrameworkCore" Version="9.0.5" />
    <PackageVersion Include="Microsoft.EntityFrameworkCore.SqlServer" Version="9.0.5" />
    <PackageVersion Include="Microsoft.EntityFrameworkCore.Tools" Version="9.0.5" />
    
    <!-- MediatR -->
    <PackageVersion Include="MediatR" Version="12.3.0" />
    <PackageVersion Include="MediatR.Extensions.Microsoft.DependencyInjection" Version="12.3.0" />
    
    <!-- FluentValidation -->
    <PackageVersion Include="FluentValidation" Version="11.9.0" />
    <PackageVersion Include="FluentValidation.AspNetCore" Version="11.4.0" />
    
    <!-- Serilog -->
    <PackageVersion Include="Serilog.AspNetCore" Version="8.0.1" />
    <PackageVersion Include="Serilog.Sinks.Console" Version="5.0.1" />
    <PackageVersion Include="Serilog.Sinks.File" Version="5.0.0" />
    
    <!-- Swagger -->
    <PackageVersion Include="Swashbuckle.AspNetCore" Version="6.6.0" />
    
    <!-- Testing -->
    <PackageVersion Include="Microsoft.NET.Test.Sdk" Version="17.10.0" />
    <PackageVersion Include="xunit" Version="2.7.0" />
    <PackageVersion Include="xunit.runner.visualstudio" Version="2.6.0" />
    <PackageVersion Include="Moq" Version="4.20.70" />
    <PackageVersion Include="FluentAssertions" Version="6.12.0" />
    <PackageVersion Include="coverlet.collector" Version="6.0.1" />
  </ItemGroup>
</Project>
```

## Frontend Dependency Management

### NPM Package Management

The application uses npm for frontend dependency management:

```json
// package.json
{
  "name": "wendoverhoa-frontend",
  "version": "1.0.0",
  "private": true,
  "dependencies": {
    "bootstrap": "5.3.6",
    "bootstrap-icons": "1.11.1",
    "@popperjs/core": "2.11.8",
    "jquery": "3.7.1"
  },
  "devDependencies": {
    "sass": "1.69.5",
    "autoprefixer": "10.4.16",
    "postcss": "8.4.31",
    "postcss-cli": "10.1.0"
  },
  "scripts": {
    "build:css": "sass ./Styles/main.scss ./wwwroot/css/site.css --style compressed",
    "watch:css": "sass --watch ./Styles/main.scss ./wwwroot/css/site.css"
  }
}
```

### Asset Management

The application manages frontend assets using the built-in ASP.NET Core static file middleware:

```csharp
// Startup.cs
public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
{
    // Other middleware configuration...
    
    app.UseStaticFiles();
    
    // Other middleware configuration...
}
```

### CDN Integration

The application uses Content Delivery Networks (CDNs) for common libraries with local fallbacks:

```html
<!-- _Layout.cshtml -->
<link rel="stylesheet" href="https://cdn.jsdelivr.net/npm/bootstrap@5.3.6/dist/css/bootstrap.min.css" 
      integrity="sha384-QWTKZyjpPEjISv5WaRU9OFeRpok6YctnYmDr5pNlyT2bRjXh0JMhjY6hW+ALEwIH" 
      crossorigin="anonymous"
      asp-fallback-href="~/lib/bootstrap/dist/css/bootstrap.min.css"
      asp-fallback-test-class="d-none"
      asp-fallback-test-property="display"
      asp-fallback-test-value="none">

<script src="https://cdn.jsdelivr.net/npm/bootstrap@5.3.6/dist/js/bootstrap.bundle.min.js" 
        integrity="sha384-Z3DP8Q82fsj9XQYCUzHmPLxclrzEPfsxYaYAZ/hgNHZGQ+0j8gqbC4eoVbkFxh2" 
        crossorigin="anonymous"
        asp-fallback-src="~/lib/bootstrap/dist/js/bootstrap.bundle.min.js"
        asp-fallback-test="window.jQuery && window.jQuery.fn && window.jQuery.fn.modal"></script>
```

## Security Scanning

### Vulnerability Scanning

The application uses GitHub's security features for vulnerability scanning:

1. **Dependency Scanning**: Automatically scan dependencies for known vulnerabilities
2. **CodeQL Analysis**: Scan code for security issues
3. **Secret Scanning**: Detect and prevent secrets from being committed

```yaml
# .github/workflows/security-scan.yml
name: Security Scan

on:
  push:
    branches: [ main, develop ]
  pull_request:
    branches: [ main, develop ]
  schedule:
    - cron: '0 0 * * 0'  # Weekly scan on Sundays

jobs:
  dependency-scan:
    runs-on: ubuntu-latest
    steps:
      - uses: actions/checkout@v3
      - name: Setup .NET
        uses: actions/setup-dotnet@v3
        with:
          dotnet-version: 9.0.x
      - name: Restore dependencies
        run: dotnet restore
      - name: Dependency Review
        uses: actions/dependency-review-action@v3
        
  code-scan:
    runs-on: ubuntu-latest
    permissions:
      security-events: write
    steps:
      - uses: actions/checkout@v3
      - name: Initialize CodeQL
        uses: github/codeql-action/init@v2
        with:
          languages: csharp, javascript
      - name: Autobuild
        uses: github/codeql-action/autobuild@v2
      - name: Perform CodeQL Analysis
        uses: github/codeql-action/analyze@v2
```

### Automated Updates

The application uses Dependabot for automated dependency updates:

```yaml
# .github/dependabot.yml
version: 2
updates:
  # NuGet packages
  - package-ecosystem: "nuget"
    directory: "/"
    schedule:
      interval: "weekly"
    open-pull-requests-limit: 10
    target-branch: "develop"
    labels:
      - "dependencies"
      - "nuget"
    
  # npm packages
  - package-ecosystem: "npm"
    directory: "/"
    schedule:
      interval: "weekly"
    open-pull-requests-limit: 10
    target-branch: "develop"
    labels:
      - "dependencies"
      - "npm"
    
  # GitHub Actions
  - package-ecosystem: "github-actions"
    directory: "/"
    schedule:
      interval: "monthly"
    open-pull-requests-limit: 10
    target-branch: "develop"
    labels:
      - "dependencies"
      - "github-actions"
```

## Dependency Injection

### DI Container

The application uses the built-in ASP.NET Core dependency injection container:

```csharp
// Program.cs
var builder = WebApplication.CreateBuilder(args);

// Register services
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddIdentity<ApplicationUser, ApplicationRole>()
    .AddEntityFrameworkStores<ApplicationDbContext>()
    .AddDefaultTokenProviders();

// Register application services
builder.Services.AddScoped<IPropertyRepository, PropertyRepository>();
builder.Services.AddScoped<IResidentRepository, ResidentRepository>();
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();

// Register MediatR
builder.Services.AddMediatR(cfg => {
    cfg.RegisterServicesFromAssembly(typeof(Program).Assembly);
    // Add behaviors
    cfg.AddBehavior(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));
    cfg.AddBehavior(typeof(IPipelineBehavior<,>), typeof(LoggingBehavior<,>));
    cfg.AddBehavior(typeof(IPipelineBehavior<,>), typeof(CachingBehavior<,>));
});

// Register validators
builder.Services.AddValidatorsFromAssembly(typeof(Program).Assembly);

var app = builder.Build();

// Configure middleware
// ...

app.Run();
```

### Service Lifetimes

The application follows these service lifetime guidelines:

1. **Singleton**: Services that are stateless and thread-safe
2. **Scoped**: Services that should be created once per request
3. **Transient**: Services that should be created each time they are requested

```csharp
// Service registration examples
// Singleton services
builder.Services.AddSingleton<ICacheService, CacheService>();
builder.Services.AddSingleton<IEmailTemplateProvider, EmailTemplateProvider>();

// Scoped services
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();
builder.Services.AddScoped<IPropertyRepository, PropertyRepository>();
builder.Services.AddScoped<ICurrentUserService, CurrentUserService>();

// Transient services
builder.Services.AddTransient<INotificationService, NotificationService>();
builder.Services.AddTransient<IFileValidator, FileValidator>();
```

### Decorator Pattern

The application uses the Scrutor library for implementing the decorator pattern:

```csharp
// Install Scrutor
// dotnet add package Scrutor

// Register services with decorators
builder.Services.AddScoped<IPropertyRepository, PropertyRepository>();
builder.Services.Decorate<IPropertyRepository, CachedPropertyRepository>();

builder.Services.AddScoped<IEmailService, EmailService>();
builder.Services.Decorate<IEmailService, LoggingEmailService>();
```

## Clean Architecture Dependencies

### Layer Dependencies

The application follows Clean Architecture principles for dependencies:

1. **Domain Layer**: No dependencies on other layers or external packages
2. **Application Layer**: Dependencies on Domain layer only
3. **Infrastructure Layer**: Dependencies on Domain and Application layers
4. **Presentation Layer**: Dependencies on all other layers

```
Domain Layer
↑
Application Layer
↑
Infrastructure Layer   Presentation Layer
```

### Project Structure

The application is organized into projects that reflect the Clean Architecture layers:

```
WendoverHOA.Domain
WendoverHOA.Application
WendoverHOA.Infrastructure
WendoverHOA.Web
WendoverHOA.Tests
```

### Project Dependencies

Each project has specific dependencies aligned with Clean Architecture:

#### Domain Project

```xml
<!-- WendoverHOA.Domain.csproj -->
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <TargetFramework>net9.0</TargetFramework>
  </PropertyGroup>
  <!-- No external dependencies -->
</Project>
```

#### Application Project

```xml
<!-- WendoverHOA.Application.csproj -->
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <TargetFramework>net9.0</TargetFramework>
  </PropertyGroup>
  <ItemGroup>
    <ProjectReference Include="..\WendoverHOA.Domain\WendoverHOA.Domain.csproj" />
  </ItemGroup>
  <ItemGroup>
    <PackageReference Include="MediatR" />
    <PackageReference Include="FluentValidation" />
    <PackageReference Include="AutoMapper" />
  </ItemGroup>
</Project>
```

#### Infrastructure Project

```xml
<!-- WendoverHOA.Infrastructure.csproj -->
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <TargetFramework>net9.0</TargetFramework>
  </PropertyGroup>
  <ItemGroup>
    <ProjectReference Include="..\WendoverHOA.Domain\WendoverHOA.Domain.csproj" />
    <ProjectReference Include="..\WendoverHOA.Application\WendoverHOA.Application.csproj" />
  </ItemGroup>
  <ItemGroup>
    <PackageReference Include="Microsoft.EntityFrameworkCore" />
    <PackageReference Include="Microsoft.EntityFrameworkCore.SqlServer" />
    <PackageReference Include="Microsoft.AspNetCore.Identity.EntityFrameworkCore" />
    <PackageReference Include="Serilog.AspNetCore" />
  </ItemGroup>
</Project>
```

#### Web Project

```xml
<!-- WendoverHOA.Web.csproj -->
<Project Sdk="Microsoft.NET.Sdk.Web">
  <PropertyGroup>
    <TargetFramework>net9.0</TargetFramework>
  </PropertyGroup>
  <ItemGroup>
    <ProjectReference Include="..\WendoverHOA.Domain\WendoverHOA.Domain.csproj" />
    <ProjectReference Include="..\WendoverHOA.Application\WendoverHOA.Application.csproj" />
    <ProjectReference Include="..\WendoverHOA.Infrastructure\WendoverHOA.Infrastructure.csproj" />
  </ItemGroup>
  <ItemGroup>
    <PackageReference Include="Microsoft.AspNetCore.Authentication.JwtBearer" />
    <PackageReference Include="Swashbuckle.AspNetCore" />
  </ItemGroup>
</Project>
```

## Dependency Documentation

### Package Documentation

All dependencies are documented with their purpose and usage:

| Package | Version | Purpose | Layer |
|---------|---------|---------|-------|
| Microsoft.AspNetCore.Authentication.JwtBearer | 9.0.5 | JWT authentication for API endpoints | Infrastructure |
| Microsoft.AspNetCore.Identity.EntityFrameworkCore | 9.0.5 | User identity management | Infrastructure |
| Microsoft.EntityFrameworkCore | 9.0.5 | Object-relational mapping | Infrastructure |
| Microsoft.EntityFrameworkCore.SqlServer | 9.0.5 | SQL Server database provider | Infrastructure |
| MediatR | 12.3.0 | CQRS implementation | Application |
| FluentValidation | 11.9.0 | Request validation | Application |
| AutoMapper | 13.0.1 | Object mapping | Application |
| Serilog.AspNetCore | 8.0.1 | Structured logging | Infrastructure |
| Swashbuckle.AspNetCore | 6.6.0 | API documentation | Web |

### Dependency Graph

The application maintains a dependency graph to visualize relationships:

```
dotnet-depends graph -o dependency-graph.png
```

## Dependency Upgrade Process

### Upgrade Workflow

The application follows this process for dependency upgrades:

1. **Identify**: Dependabot creates PR with upgrade
2. **Evaluate**: Review changes and potential impact
3. **Test**: Run automated tests to verify compatibility
4. **Integrate**: Merge changes to development branch
5. **Monitor**: Watch for issues in development environment
6. **Release**: Include upgrades in regular release cycle

### Breaking Changes

When handling breaking changes:

1. Review release notes and migration guides
2. Create a dedicated branch for the upgrade
3. Make necessary code changes to accommodate breaking changes
4. Update tests to verify new behavior
5. Document migration steps in PR description
6. Conduct thorough testing before merging

## Conclusion

This dependency management strategy provides a comprehensive approach to managing dependencies across the Wendover HOA application. By following these guidelines, the application will maintain secure, up-to-date, and compatible dependencies while adhering to Clean Architecture principles.

The strategy ensures:

1. **Security**: Regular scanning and updates for vulnerabilities
2. **Consistency**: Uniform versions across all projects
3. **Maintainability**: Clear documentation and upgrade processes
4. **Architectural Integrity**: Dependencies aligned with Clean Architecture

By implementing this strategy, the Wendover HOA application will maintain a robust and secure dependency management system that supports the application's long-term maintainability and security goals.
