# Wendover Homeowners Association Web Application

## Project Overview

This document provides a high-level overview of the Wendover Homeowners Association web application project, which aims to create a secure, modern web application for residents and board members of the Wendover HOA in Bedford, Texas.

## Technology Stack

- **Frontend**: 
  - ASP.NET Core MVC 9
  - Blazor for interactive components
  - Bootstrap 5.3.6
  - Bootstrap Icons
  - Bootswatch Theme (Cosmo)
  - Mobile-first, responsive design

- **Backend**:
  - .NET 9 Core (9.0.5)
  - ASP.NET Core 9
  - Entity Framework Core 9
  - RESTful API layer

- **Database**:
  - Microsoft SQL Server 2022

- **Authentication**:
  - .NET Core Identity
  - External providers (Microsoft, Google, Apple)
  - Multi-factor authentication
  - Advanced password policies

- **Hosting**:
  - IONOS Web Hosting Business Windows plan
  - Production domain: wendoverhoa.org
  - Development domain: dev.wendoverhoa.org

## Architecture

- Clean Architecture with CQRS and MediatR patterns
- Role-based access control (viewer, editor, admin)
- Three environments: localhost, development, production
- Security-focused design with GitHub security tools and best practices
- Comprehensive API management with OAuth 2.0 and JWT authentication
- Centralized notification system for critical and important announcements

## CI/CD Pipeline

- GitHub Actions for automated build and deployment
- Automated unit and integration testing on each commit
- Code quality checks using GitHub's free tools:
  - CodeQL for code scanning
  - Dependency scanning for vulnerable packages
  - Secret scanning to prevent credential leaks
- Linting and code style enforcement
- Automated deployment to development environment on successful builds
- Manual approval process for production deployments
- Monitoring and alerting for deployment issues
- Rollback capabilities for failed deployments
- Documentation generation integrated into build process

## Development Approach

The application will be developed in phases, with each phase focusing on specific features. Each feature will have its own requirements document, and each phase will have a checklist of tasks to be completed.

## Documentation Structure

- `/docs/requirements/features/`: Contains feature-specific use case requirements
- `/docs/phases/`: Contains phase-specific task checklists
- `/docs/architecture/`: Contains architectural documentation
- `/docs/setup/`: Contains setup and installation guides

## Project Phases

1. **Phase 1: Core Infrastructure**
   - Authentication and authorization with multi-factor authentication
   - User management with role-based access control
   - Site layout and navigation with responsive design
   - API infrastructure with CQRS and MediatR patterns
   - Security implementation (HTTPS, CSRF, CSP, rate limiting)
   - CI/CD pipeline configuration
   - Database design and implementation

2. **Phase 2: Community Features**
   - Announcements with importance levels and notification banner
   - Community calendar with event management
   - Document repository with categorization and versioning
   - Notification system for critical and important announcements
   - Search functionality across community features
   - Mobile-responsive interfaces for community features

3. **Phase 3: Administrative Features**
   - Directory (unified property and resident management)
   - Board management with committee structure
   - Meeting minutes with action item tracking
   - User feedback system with response management
   - Vendor suggestions with voting and status tracking
   - Administrative controls and system configuration
   - API management for administrators
   - Audit logging and system monitoring

4. **Phase 4: Financial Features**
   - Dues tracking with payment history
   - Payment processing with multiple payment methods
   - Financial reporting with visualizations
   - Expense tracking with approval workflows
   - Budget management and forecasting
   - Reserve fund tracking
   - Financial data export and reporting

Each phase will be developed iteratively with Cascade AI Claude 3.7 Sonnet, ensuring adherence to best practices and project requirements.

## Key Design Decisions

1. **Simplified Data Management**
   - Streamlined record management without archiving functionality
   - Direct deletion with appropriate confirmation and authorization checks
   - Focus on core functionality and user experience

2. **Enhanced User Experience**
   - Collapsible notification banner for critical and important announcements
   - Color-coded alerts (red for critical, yellow for important)
   - Session-based state memory for user preferences

3. **API-First Approach**
   - RESTful API endpoints for all features
   - Comprehensive API management for administrators
   - OAuth 2.0 and JWT authentication for secure API access
   - API versioning strategy for backward compatibility
   - Swagger/OpenAPI documentation generation

4. **Security and Compliance**
   - Role-based access control across all features
   - Comprehensive audit logging of all system actions
   - Two-factor authentication for sensitive operations
   - Secure handling of all user data and configurations

5. **Testing and Quality Assurance**
   - Comprehensive unit tests for all core functionality
   - Integration tests for feature interactions
   - UI tests for critical user flows
   - Automated code quality checks and security scanning
