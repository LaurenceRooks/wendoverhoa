# Developer Environment Setup Guide

This guide provides detailed instructions for setting up a development environment for the Wendover HOA web application. Following these steps will ensure that all developers have a consistent environment that matches the production configuration.

## Required Tools and Versions

### Core Development Tools

| Tool | Version | Purpose | Download Link |
|------|---------|---------|--------------|
| .NET SDK | 9.0.5 or later | Core framework | [Download .NET 9](https://dotnet.microsoft.com/download/dotnet/9.0) |
| SQL Server | 2022 | Database | [Download SQL Server 2022](https://www.microsoft.com/en-us/sql-server/sql-server-downloads) |
| SQL Server Management Studio | 19.0 or later | Database management | [Download SSMS](https://docs.microsoft.com/en-us/sql/ssms/download-sql-server-management-studio-ssms) |
| Git | Latest | Version control | [Download Git](https://git-scm.com/downloads) |
| Node.js | 20.x LTS | Frontend tooling | [Download Node.js](https://nodejs.org/) |
| npm | 10.x or later | Package management | Included with Node.js |

### IDE Options

#### Option 1: Visual Studio 2022 (Recommended for Windows)

| Component | Version | Notes |
|-----------|---------|-------|
| Visual Studio | 2022 (17.8 or later) | Community Edition is sufficient |
| Workloads | ASP.NET and web development, .NET desktop development, Data storage and processing | Required for full functionality |

[Download Visual Studio 2022](https://visualstudio.microsoft.com/vs/)

#### Option 2: Visual Studio Code (Cross-platform)

| Component | Version | Notes |
|-----------|---------|-------|
| Visual Studio Code | Latest | Lightweight, cross-platform IDE |
| C# Dev Kit | Latest | Essential for .NET development |

[Download Visual Studio Code](https://code.visualstudio.com/)

### Recommended Extensions

#### For Visual Studio 2022

- **Web Essentials**: Enhanced web development features
- **ReSharper** (optional): Code quality and refactoring tools
- **EF Core Power Tools**: Entity Framework Core visualization and reverse engineering
- **GitHub Extension for Visual Studio**: Enhanced GitHub integration

#### For Visual Studio Code

- **C# Dev Kit**: Comprehensive C# development experience
- **C# Extensions**: Additional C# tooling
- **SQL Server (mssql)**: SQL Server integration
- **Blazor Snippets**: Productivity snippets for Blazor
- **GitLens**: Enhanced Git capabilities
- **Prettier - Code formatter**: Code formatting
- **ESLint**: JavaScript linting
- **IntelliCode**: AI-assisted development
- **REST Client**: Test API endpoints within VS Code

## IDE Configuration

### Visual Studio 2022 Configuration

1. **Editor Settings**
   - Enable "Format document on save" (Tools → Options → Text Editor → C# → Code Style)
   - Set tab size to 4 spaces (Tools → Options → Text Editor → C# → Tabs)
   - Enable "Show white space" (Tools → Options → Text Editor → Advanced)

2. **Git Configuration**
   - Configure Git user name and email (Team Explorer → Settings → Git → Global Settings)
   - Set up Git credential manager (automatically configured)

3. **Build Configuration**
   - Enable parallel build (Tools → Options → Projects and Solutions → Build and Run)
   - Set "On Run, when projects are out of date" to "Always build"

### Visual Studio Code Configuration

1. **Create workspace settings file** (`.vscode/settings.json`):

```json
{
  "editor.formatOnSave": true,
  "editor.tabSize": 4,
  "editor.renderWhitespace": "all",
  "editor.codeActionsOnSave": {
    "source.fixAll.eslint": true
  },
  "csharp.format.enable": true,
  "dotnet.defaultSolution": "WendoverHOA.sln",
  "files.exclude": {
    "**/bin": true,
    "**/obj": true
  },
  "[csharp]": {
    "editor.defaultFormatter": "ms-dotnettools.csharp"
  },
  "[html]": {
    "editor.defaultFormatter": "esbenp.prettier-vscode"
  },
  "[javascript]": {
    "editor.defaultFormatter": "esbenp.prettier-vscode"
  },
  "[css]": {
    "editor.defaultFormatter": "esbenp.prettier-vscode"
  }
}
```

2. **Create launch configuration** (`.vscode/launch.json`):

```json
{
  "version": "0.2.0",
  "configurations": [
    {
      "name": "Launch WendoverHOA (web)",
      "type": "coreclr",
      "request": "launch",
      "preLaunchTask": "build",
      "program": "${workspaceFolder}/src/WendoverHOA.Web/bin/Debug/net9.0/WendoverHOA.Web.dll",
      "args": [],
      "cwd": "${workspaceFolder}/src/WendoverHOA.Web",
      "stopAtEntry": false,
      "serverReadyAction": {
        "action": "openExternally",
        "pattern": "\\bNow listening on:\\s+(https?://\\S+)"
      },
      "env": {
        "ASPNETCORE_ENVIRONMENT": "Development"
      }
    }
  ]
}
```

3. **Create tasks configuration** (`.vscode/tasks.json`):

```json
{
  "version": "2.0.0",
  "tasks": [
    {
      "label": "build",
      "command": "dotnet",
      "type": "process",
      "args": [
        "build",
        "${workspaceFolder}/WendoverHOA.sln",
        "/property:GenerateFullPaths=true",
        "/consoleloggerparameters:NoSummary"
      ],
      "problemMatcher": "$msCompile"
    }
  ]
}
```

## Environment Variables

Set up the following environment variables for local development:

### Windows (PowerShell)

```powershell
$Env:ASPNETCORE_ENVIRONMENT = "Development"
$Env:ConnectionStrings__DefaultConnection = "Server=(localdb)\\mssqllocaldb;Database=WendoverHOA;Trusted_Connection=True;MultipleActiveResultSets=true"
```

### macOS/Linux (Bash)

```bash
export ASPNETCORE_ENVIRONMENT="Development"
export ConnectionStrings__DefaultConnection="Server=localhost;Database=WendoverHOA;User Id=sa;Password=YourPassword;TrustServerCertificate=True"
```

## Code Style and Standards

The project follows these coding standards:

1. **C# Code Style**
   - Follow Microsoft's C# coding conventions
   - Use camelCase for private fields and parameters
   - Use PascalCase for public properties, methods, and classes
   - Use meaningful, descriptive names
   - Include XML documentation for public APIs

2. **Clean Architecture**
   - Respect layer boundaries (Domain, Application, Infrastructure, Presentation)
   - Follow CQRS pattern with MediatR
   - Keep domain logic free of infrastructure concerns

3. **Testing**
   - Write unit tests for all business logic
   - Follow Arrange-Act-Assert pattern
   - Use meaningful test names that describe the scenario being tested

## Troubleshooting Common Issues

### Entity Framework Migrations

If you encounter issues with Entity Framework migrations:

```bash
# Reset migrations (use with caution)
dotnet ef database drop --project src/WendoverHOA.Infrastructure --startup-project src/WendoverHOA.Web
dotnet ef migrations remove --project src/WendoverHOA.Infrastructure --startup-project src/WendoverHOA.Web

# Create a new migration
dotnet ef migrations add InitialCreate --project src/WendoverHOA.Infrastructure --startup-project src/WendoverHOA.Web

# Apply migrations
dotnet ef database update --project src/WendoverHOA.Infrastructure --startup-project src/WendoverHOA.Web
```

### .NET SDK Version Issues

If you encounter SDK version issues:

```bash
# Check installed SDKs
dotnet --list-sdks

# Check runtime version
dotnet --info
```

### SQL Server Connection Issues

1. Verify SQL Server is running:
   - Windows: Check Services app
   - macOS/Linux: Check Docker container status

2. Test connection with SQL Server Management Studio or Azure Data Studio

3. Verify connection string in `appsettings.Development.json`
