# Project Initialization Template

This document provides instructions for initializing the Wendover HOA project with the correct solution structure, dependencies, and configuration following Clean Architecture principles.

## Solution Structure

Create a new solution with the following structure:

```
WendoverHOA/
├── src/
│   ├── WendoverHOA.Domain/              # Core domain entities and business logic
│   ├── WendoverHOA.Application/         # Application services, interfaces, DTOs
│   ├── WendoverHOA.Infrastructure/      # External concerns implementation
│   └── WendoverHOA.Web/                 # ASP.NET Core MVC web application
└── tests/
    ├── WendoverHOA.Domain.Tests/
    ├── WendoverHOA.Application.Tests/
    ├── WendoverHOA.Infrastructure.Tests/
    └── WendoverHOA.Web.Tests/
```

## Initialization Commands

```bash
# Create solution
dotnet new sln -n WendoverHOA

# Create projects
dotnet new classlib -n WendoverHOA.Domain -o src/WendoverHOA.Domain
dotnet new classlib -n WendoverHOA.Application -o src/WendoverHOA.Application
dotnet new classlib -n WendoverHOA.Infrastructure -o src/WendoverHOA.Infrastructure
dotnet new mvc -n WendoverHOA.Web -o src/WendoverHOA.Web

# Create test projects
dotnet new xunit -n WendoverHOA.Domain.Tests -o tests/WendoverHOA.Domain.Tests
dotnet new xunit -n WendoverHOA.Application.Tests -o tests/WendoverHOA.Application.Tests
dotnet new xunit -n WendoverHOA.Infrastructure.Tests -o tests/WendoverHOA.Infrastructure.Tests
dotnet new xunit -n WendoverHOA.Web.Tests -o tests/WendoverHOA.Web.Tests

# Add projects to solution
dotnet sln add src/WendoverHOA.Domain/WendoverHOA.Domain.csproj
dotnet sln add src/WendoverHOA.Application/WendoverHOA.Application.csproj
dotnet sln add src/WendoverHOA.Infrastructure/WendoverHOA.Infrastructure.csproj
dotnet sln add src/WendoverHOA.Web/WendoverHOA.Web.csproj
dotnet sln add tests/WendoverHOA.Domain.Tests/WendoverHOA.Domain.Tests.csproj
dotnet sln add tests/WendoverHOA.Application.Tests/WendoverHOA.Application.Tests.csproj
dotnet sln add tests/WendoverHOA.Infrastructure.Tests/WendoverHOA.Infrastructure.Tests.csproj
dotnet sln add tests/WendoverHOA.Web.Tests/WendoverHOA.Web.Tests.csproj

# Add project references
dotnet add src/WendoverHOA.Application/WendoverHOA.Application.csproj reference src/WendoverHOA.Domain/WendoverHOA.Domain.csproj
dotnet add src/WendoverHOA.Infrastructure/WendoverHOA.Infrastructure.csproj reference src/WendoverHOA.Domain/WendoverHOA.Domain.csproj
dotnet add src/WendoverHOA.Infrastructure/WendoverHOA.Infrastructure.csproj reference src/WendoverHOA.Application/WendoverHOA.Application.csproj
dotnet add src/WendoverHOA.Web/WendoverHOA.Web.csproj reference src/WendoverHOA.Domain/WendoverHOA.Domain.csproj
dotnet add src/WendoverHOA.Web/WendoverHOA.Web.csproj reference src/WendoverHOA.Application/WendoverHOA.Application.csproj
dotnet add src/WendoverHOA.Web/WendoverHOA.Web.csproj reference src/WendoverHOA.Infrastructure/WendoverHOA.Infrastructure.csproj

# Add test project references
dotnet add tests/WendoverHOA.Domain.Tests/WendoverHOA.Domain.Tests.csproj reference src/WendoverHOA.Domain/WendoverHOA.Domain.csproj
dotnet add tests/WendoverHOA.Application.Tests/WendoverHOA.Application.Tests.csproj reference src/WendoverHOA.Application/WendoverHOA.Application.csproj
dotnet add tests/WendoverHOA.Infrastructure.Tests/WendoverHOA.Infrastructure.Tests.csproj reference src/WendoverHOA.Infrastructure/WendoverHOA.Infrastructure.csproj
dotnet add tests/WendoverHOA.Web.Tests/WendoverHOA.Web.Tests.csproj reference src/WendoverHOA.Web/WendoverHOA.Web.csproj
```

## Project Dependencies

### Central Package Management

Create a `Directory.Packages.props` file in the solution root:

```xml
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
    <PackageVersion Include="Microsoft.EntityFrameworkCore.Design" Version="9.0.5" />
    
    <!-- MediatR -->
    <PackageVersion Include="MediatR" Version="12.3.0" />
    
    <!-- FluentValidation -->
    <PackageVersion Include="FluentValidation" Version="11.9.0" />
    <PackageVersion Include="FluentValidation.AspNetCore" Version="11.4.0" />
    <PackageVersion Include="FluentValidation.DependencyInjectionExtensions" Version="11.9.0" />
    
    <!-- AutoMapper -->
    <PackageVersion Include="AutoMapper" Version="13.0.1" />
    <PackageVersion Include="AutoMapper.Extensions.Microsoft.DependencyInjection" Version="13.0.1" />
    
    <!-- Logging -->
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

### Project-Specific Dependencies

#### WendoverHOA.Domain.csproj

```xml
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <TargetFramework>net9.0</TargetFramework>
    <ImplicitUsings>enable</ImplicitUsings>
    <Nullable>enable</Nullable>
  </PropertyGroup>
  <!-- No external dependencies -->
</Project>
```

#### WendoverHOA.Application.csproj

```xml
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <TargetFramework>net9.0</TargetFramework>
    <ImplicitUsings>enable</ImplicitUsings>
    <Nullable>enable</Nullable>
  </PropertyGroup>
  <ItemGroup>
    <ProjectReference Include="..\WendoverHOA.Domain\WendoverHOA.Domain.csproj" />
  </ItemGroup>
  <ItemGroup>
    <PackageReference Include="AutoMapper" />
    <PackageReference Include="FluentValidation" />
    <PackageReference Include="MediatR" />
  </ItemGroup>
</Project>
```

#### WendoverHOA.Infrastructure.csproj

```xml
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <TargetFramework>net9.0</TargetFramework>
    <ImplicitUsings>enable</ImplicitUsings>
    <Nullable>enable</Nullable>
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

#### WendoverHOA.Web.csproj

```xml
<Project Sdk="Microsoft.NET.Sdk.Web">
  <PropertyGroup>
    <TargetFramework>net9.0</TargetFramework>
    <ImplicitUsings>enable</ImplicitUsings>
    <Nullable>enable</Nullable>
  </PropertyGroup>
  <ItemGroup>
    <ProjectReference Include="..\WendoverHOA.Domain\WendoverHOA.Domain.csproj" />
    <ProjectReference Include="..\WendoverHOA.Application\WendoverHOA.Application.csproj" />
    <ProjectReference Include="..\WendoverHOA.Infrastructure\WendoverHOA.Infrastructure.csproj" />
  </ItemGroup>
  <ItemGroup>
    <PackageReference Include="Microsoft.AspNetCore.Authentication.JwtBearer" />
    <PackageReference Include="Microsoft.EntityFrameworkCore.Design" />
    <PackageReference Include="Swashbuckle.AspNetCore" />
    <PackageReference Include="AutoMapper.Extensions.Microsoft.DependencyInjection" />
    <PackageReference Include="FluentValidation.AspNetCore" />
    <PackageReference Include="FluentValidation.DependencyInjectionExtensions" />
    <PackageReference Include="Serilog.AspNetCore" />
  </ItemGroup>
</Project>
```

## Basic Configuration Files

### appsettings.json

Create this file in the WendoverHOA.Web project:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost;Database=WendoverHOA;Trusted_Connection=True;MultipleActiveResultSets=true"
  },
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning"
    }
  },
  "AllowedHosts": "*",
  "Serilog": {
    "MinimumLevel": {
      "Default": "Information",
      "Override": {
        "Microsoft": "Warning",
        "Microsoft.Hosting.Lifetime": "Information",
        "Microsoft.EntityFrameworkCore": "Warning"
      }
    },
    "WriteTo": [
      { "Name": "Console" },
      {
        "Name": "File",
        "Args": {
          "path": "logs/wendoverhoa-.log",
          "rollingInterval": "Day"
        }
      }
    ],
    "Enrich": [ "FromLogContext", "WithMachineName", "WithThreadId" ]
  },
  "JwtSettings": {
    "Secret": "PLEASE_CHANGE_THIS_TO_A_RANDOM_STRING_IN_PRODUCTION",
    "Issuer": "WendoverHOA",
    "Audience": "WendoverHOAUsers",
    "ExpiryMinutes": 60,
    "RefreshExpiryDays": 7
  }
}
```

### Program.cs

Create this file in the WendoverHOA.Web project:

```csharp
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Serilog;
using WendoverHOA.Application;
using WendoverHOA.Infrastructure;
using WendoverHOA.Infrastructure.Persistence;
using WendoverHOA.Infrastructure.Identity;

var builder = WebApplication.CreateBuilder(args);

// Add Serilog
builder.Host.UseSerilog((context, services, configuration) => configuration
    .ReadFrom.Configuration(context.Configuration)
    .ReadFrom.Services(services)
    .Enrich.FromLogContext());

// Add services to the container
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(connectionString));

builder.Services.AddIdentity<ApplicationUser, ApplicationRole>()
    .AddEntityFrameworkStores<ApplicationDbContext>()
    .AddDefaultTokenProviders();

// Add application services
builder.Services.AddApplicationServices();
builder.Services.AddInfrastructureServices(builder.Configuration);

// Add controllers and views
builder.Services.AddControllersWithViews();

// Add Swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline
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

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseSerilogRequestLogging();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
```

### DependencyInjection.cs

Create these files in the Application and Infrastructure projects:

#### WendoverHOA.Application/DependencyInjection.cs

```csharp
using System.Reflection;
using FluentValidation;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using WendoverHOA.Application.Common.Behaviors;

namespace WendoverHOA.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection services)
    {
        services.AddAutoMapper(Assembly.GetExecutingAssembly());
        services.AddValidatorsFromAssembly(Assembly.GetExecutingAssembly());
        
        services.AddMediatR(cfg => {
            cfg.RegisterServicesFromAssembly(Assembly.GetExecutingAssembly());
            cfg.AddBehavior(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));
            cfg.AddBehavior(typeof(IPipelineBehavior<,>), typeof(LoggingBehavior<,>));
        });
        
        return services;
    }
}
```

#### WendoverHOA.Infrastructure/DependencyInjection.cs

```csharp
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using WendoverHOA.Application.Common.Interfaces;
using WendoverHOA.Infrastructure.Identity;
using WendoverHOA.Infrastructure.Persistence;
using WendoverHOA.Infrastructure.Services;

namespace WendoverHOA.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructureServices(
        this IServiceCollection services, 
        IConfiguration configuration)
    {
        services.AddDbContext<ApplicationDbContext>(options =>
            options.UseSqlServer(
                configuration.GetConnectionString("DefaultConnection"),
                b => b.MigrationsAssembly(typeof(ApplicationDbContext).Assembly.FullName)));

        services.AddScoped<IApplicationDbContext>(provider => 
            provider.GetRequiredService<ApplicationDbContext>());
            
        services.AddScoped<IUnitOfWork, UnitOfWork>();
        
        services.AddTransient<IDateTime, DateTimeService>();
        services.AddTransient<IIdentityService, IdentityService>();
        services.AddTransient<IEmailService, EmailService>();
        
        return services;
    }
}
```

## Initial Folder Structure

Create the following folder structure in each project:

### WendoverHOA.Domain

```
WendoverHOA.Domain/
├── Common/
│   ├── BaseEntity.cs
│   ├── ValueObject.cs
│   └── Enumeration.cs
├── Entities/
│   ├── Property.cs
│   ├── Resident.cs
│   ├── Announcement.cs
│   └── Document.cs
├── Enums/
│   ├── PropertyStatus.cs
│   └── DocumentType.cs
├── Events/
│   └── DomainEvent.cs
└── Exceptions/
    └── DomainException.cs
```

### WendoverHOA.Application

```
WendoverHOA.Application/
├── Common/
│   ├── Behaviors/
│   │   ├── ValidationBehavior.cs
│   │   └── LoggingBehavior.cs
│   ├── Exceptions/
│   │   ├── ApplicationException.cs
│   │   ├── ValidationException.cs
│   │   └── NotFoundException.cs
│   ├── Interfaces/
│   │   ├── IApplicationDbContext.cs
│   │   ├── ICurrentUserService.cs
│   │   ├── IDateTime.cs
│   │   ├── IEmailService.cs
│   │   ├── IIdentityService.cs
│   │   └── IUnitOfWork.cs
│   └── Mappings/
│       └── MappingProfile.cs
├── Features/
│   ├── Announcements/
│   ├── Authentication/
│   ├── Documents/
│   ├── Properties/
│   └── Residents/
└── DTOs/
    ├── AnnouncementDto.cs
    ├── DocumentDto.cs
    ├── PropertyDto.cs
    └── ResidentDto.cs
```

### WendoverHOA.Infrastructure

```
WendoverHOA.Infrastructure/
├── Identity/
│   ├── ApplicationUser.cs
│   ├── ApplicationRole.cs
│   └── IdentityService.cs
├── Persistence/
│   ├── ApplicationDbContext.cs
│   ├── UnitOfWork.cs
│   ├── Configurations/
│   │   ├── PropertyConfiguration.cs
│   │   ├── ResidentConfiguration.cs
│   │   ├── AnnouncementConfiguration.cs
│   │   └── DocumentConfiguration.cs
│   ├── Interceptors/
│   │   └── AuditableEntityInterceptor.cs
│   └── Repositories/
│       ├── PropertyRepository.cs
│       ├── ResidentRepository.cs
│       ├── AnnouncementRepository.cs
│       └── DocumentRepository.cs
└── Services/
    ├── DateTimeService.cs
    ├── EmailService.cs
    └── CurrentUserService.cs
```

### WendoverHOA.Web

```
WendoverHOA.Web/
├── Controllers/
│   ├── HomeController.cs
│   ├── AccountController.cs
│   ├── AnnouncementsController.cs
│   ├── DocumentsController.cs
│   ├── PropertiesController.cs
│   └── ResidentsController.cs
├── Views/
│   ├── Home/
│   ├── Account/
│   ├── Announcements/
│   ├── Documents/
│   ├── Properties/
│   ├── Residents/
│   └── Shared/
├── wwwroot/
│   ├── css/
│   ├── js/
│   ├── lib/
│   └── images/
└── Areas/
    └── Api/
        └── Controllers/
            ├── AnnouncementsController.cs
            ├── DocumentsController.cs
            ├── PropertiesController.cs
            └── ResidentsController.cs
```

## Next Steps

After initializing the project structure:

1. Create the base entity classes and interfaces
2. Set up the database context and initial migrations
3. Implement the authentication system
4. Begin implementing core features according to the development roadmap

This template provides the foundation for a clean, maintainable application following Clean Architecture principles and the project requirements.
