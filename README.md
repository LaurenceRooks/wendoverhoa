# Wendover Homeowners Association Web Application

A modern web application for the Wendover Homeowners Association in Bedford, Texas, built with ASP.NET Core 9 following Clean Architecture principles.

## Project Overview

This application provides a comprehensive platform for managing all aspects of the Wendover Homeowners Association, including:

- User authentication and role-based access control
- Community announcements and calendar
- Document repository
- Property and resident directory
- Board management and meeting minutes
- Financial features including dues tracking and payment processing

## Technology Stack

- **Frontend**:
  - ASP.NET Core MVC 9
  - Blazor for interactive components
  - Bootstrap 5.3.6 with Bootswatch Cosmo theme
  - Mobile-first, responsive design

- **Backend**:
  - .NET 9 Core (9.0.5)
  - Entity Framework Core 9
  - SQL Server 2022
  - RESTful API with CQRS and MediatR patterns

- **Authentication**:
  - ASP.NET Core Identity
  - External providers (Microsoft, Google, Apple)
  - Multi-factor authentication

## Architecture

This project follows Clean Architecture principles with the following layers:

- **Domain**: Core business entities, interfaces, and logic
- **Application**: Use cases, DTOs, and application services
- **Infrastructure**: External concerns implementation (database, identity, etc.)
- **Web**: User interface and API controllers

## Getting Started

### Prerequisites

- .NET 9 SDK
- SQL Server 2022 (or compatible version)
- Visual Studio 2025 or compatible IDE

### Installation

1. Clone the repository
   ```
   git clone https://github.com/LaurenceRooks/wendoverhoa.git
   ```

2. Navigate to the project directory
   ```
   cd wendoverhoa
   ```

3. Restore dependencies
   ```
   dotnet restore
   ```

4. Update the database
   ```
   dotnet ef database update --project src/WendoverHOA.Infrastructure --startup-project src/WendoverHOA.Web
   ```

5. Run the application
   ```
   dotnet run --project src/WendoverHOA.Web
   ```

## Development Approach

The application is being developed in phases:

1. **Phase 1**: Core Infrastructure
2. **Phase 2**: Community Features
3. **Phase 3**: Administrative Features
4. **Phase 4**: Financial Features

## Security

This project uses GitHub's security tools:
- CodeQL for code scanning
- Dependency scanning
- Secret scanning
- Comprehensive role-based access control

## License

This project is proprietary and confidential. Unauthorized copying, distribution, or use is strictly prohibited.

## Contact

For questions or support, please contact the Wendover HOA board.
