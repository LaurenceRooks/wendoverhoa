# Local Development Instructions

This guide provides detailed instructions for setting up and working with the Wendover HOA project locally. It covers repository cloning, branch strategy, building, and running the application in development mode.

## Repository Setup

### Cloning the Repository

1. Open a terminal or command prompt
2. Clone the repository using HTTPS or SSH:

   ```bash
   # Using HTTPS
   git clone https://github.com/wendoverhoa/wendoverhoa.git

   # Using SSH (requires SSH key setup with GitHub)
   git clone git@github.com:wendoverhoa/wendoverhoa.git
   ```

3. Navigate to the project directory:

   ```bash
   cd wendoverhoa
   ```

### Branch Strategy

The project follows a GitFlow-inspired branching strategy:

#### Main Branches

- **`main`**: Production code. This branch is protected and can only be updated via approved pull requests.
- **`develop`**: Integration branch for features. This is where feature branches are merged before going to production.

#### Supporting Branches

- **`feature/feature-name`**: Feature branches for new development work.
- **`bugfix/issue-description`**: Branches for fixing bugs.
- **`hotfix/issue-description`**: Emergency fixes for production issues.
- **`release/version-number`**: Branches for preparing releases.

#### Workflow

1. Create a new feature branch from `develop`:

   ```bash
   git checkout develop
   git pull
   git checkout -b feature/your-feature-name
   ```

2. Make your changes, commit, and push:

   ```bash
   git add .
   git commit -m "Descriptive commit message"
   git push -u origin feature/your-feature-name
   ```

3. Create a pull request to merge your feature into `develop`
4. After code review and approval, the feature will be merged
5. Periodically, `develop` will be merged into `main` for production releases

## Building the Application

### First-time Setup

1. Restore NuGet packages:

   ```bash
   dotnet restore
   ```

2. Install frontend dependencies:

   ```bash
   cd src/WendoverHOA.Web
   npm install
   cd ../..
   ```

### Building the Solution

#### Using Command Line

1. Build the entire solution:

   ```bash
   dotnet build
   ```

2. Build in Release mode:

   ```bash
   dotnet build --configuration Release
   ```

#### Using Visual Studio 2022

1. Open `WendoverHOA.sln`
2. Select the desired build configuration (Debug/Release)
3. Build the solution (F6 or Build → Build Solution)

#### Using Visual Studio Code

1. Open the project folder
2. Press Ctrl+Shift+B (or Cmd+Shift+B on macOS)
3. Select the build task

## Running the Application

### Setting Up the Database

Before running the application, ensure your database is set up correctly:

1. Apply migrations to create/update the database:

   ```bash
   dotnet ef database update --project src/WendoverHOA.Infrastructure --startup-project src/WendoverHOA.Web
   ```

2. Verify the database was created in SQL Server Management Studio or Azure Data Studio

### Running the Web Application

#### Using Command Line

1. Run the web application:

   ```bash
   cd src/WendoverHOA.Web
   dotnet run
   ```

2. For hot reload (automatically apply code changes):

   ```bash
   dotnet watch run
   ```

3. The application will be available at:
   - HTTPS: https://localhost:7001
   - HTTP: http://localhost:5001

#### Using Visual Studio 2022

1. Set `WendoverHOA.Web` as the startup project
2. Press F5 or click the "Play" button
3. Visual Studio will launch the application and open it in your default browser

#### Using Visual Studio Code

1. Press F5 to start debugging
2. The application will launch and open in your default browser

### Running with Specific Environment

To run with a specific environment configuration:

```bash
dotnet run --environment Development
```

## Testing

### Running Unit Tests

```bash
dotnet test
```

### Running Tests with Coverage

```bash
dotnet test /p:CollectCoverage=true /p:CoverletOutputFormat=lcov /p:CoverletOutput=./lcov.info
```

## Working with Frontend Assets

### CSS and JavaScript

The project uses Bootstrap 5.3.6 with the Bootswatch Cosmo theme. Frontend assets are managed using npm and bundled with ASP.NET Core's built-in bundling.

To modify frontend assets:

1. Edit files in the appropriate directories:
   - CSS: `src/WendoverHOA.Web/wwwroot/css/`
   - JavaScript: `src/WendoverHOA.Web/wwwroot/js/`

2. The changes will be automatically picked up when running with hot reload

## Debugging

### Debugging with Visual Studio 2022

1. Set breakpoints by clicking in the margin
2. Run the application in debug mode (F5)
3. Use the Debug tools to inspect variables, step through code, etc.

### Debugging with Visual Studio Code

1. Set breakpoints by clicking in the margin
2. Start debugging (F5)
3. Use the Debug panel to inspect variables, step through code, etc.

### Debugging API Endpoints

For testing API endpoints, you can use:

1. **Swagger UI**: Available at `/swagger` when running in Development mode
2. **Postman**: Import the collection from `docs/api/WendoverHOA.postman_collection.json`
3. **VS Code REST Client**: Create `.http` files in the `tests/http` directory

## Common Development Tasks

### Adding a New Entity

1. Create the entity class in `src/WendoverHOA.Domain/Entities`
2. Add DbSet to `ApplicationDbContext` in `src/WendoverHOA.Infrastructure/Persistence`
3. Create a configuration class in `src/WendoverHOA.Infrastructure/Persistence/Configurations`
4. Add a migration: `dotnet ef migrations add Add{EntityName} --project src/WendoverHOA.Infrastructure --startup-project src/WendoverHOA.Web`
5. Update the database: `dotnet ef database update --project src/WendoverHOA.Infrastructure --startup-project src/WendoverHOA.Web`

### Adding a New API Endpoint

1. Create DTOs in `src/WendoverHOA.Application/DTOs`
2. Create Command/Query in `src/WendoverHOA.Application/Features/{FeatureName}/Commands` or `Queries`
3. Create Handler in the same directory
4. Create Controller action in `src/WendoverHOA.Web/Controllers/Api`
5. Test the endpoint using Swagger or Postman

## Troubleshooting

### Common Issues and Solutions

#### Port Already in Use

If you see an error like "Failed to bind to address https://localhost:7001: address already in use":

```bash
# Find the process using the port
netstat -ano | findstr :7001  # Windows
lsof -i :7001                 # macOS/Linux

# Kill the process
taskkill /PID [PID] /F        # Windows
kill -9 [PID]                 # macOS/Linux
```

#### Database Connection Issues

1. Verify connection string in `appsettings.Development.json`
2. Ensure SQL Server is running
3. Check that the database exists
4. Verify user permissions

#### Package Restore Failures

If NuGet package restore fails:

```bash
dotnet nuget locals all --clear
dotnet restore
```

## Performance Optimization

### Development Performance Tips

1. **Use a local SQL Server** instead of a remote one for better performance
2. **Disable browser extensions** that might interfere with debugging
3. **Increase Visual Studio memory allocation** for large projects
4. **Use Release configuration** for performance testing
5. **Disable unnecessary diagnostics tools** when not needed
