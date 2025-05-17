# Authentication Feature Architecture Guide

## Overview

This document provides architectural guidance for implementing the Authentication feature in the Wendover HOA application, mapping components to the Clean Architecture layers, CQRS/MediatR patterns, and identifying cross-cutting concerns. The Authentication feature provides user authentication, registration, role-based access control, and account management functionality.

## Domain Layer Components

### Entities
- **ApplicationUser**: Core user entity extending IdentityUser
- **RefreshToken**: Entity for JWT refresh tokens
- **LoginAttempt**: Entity recording login attempts
- **UserActivity**: Entity tracking user activity for audit
- **PasswordResetToken**: Entity for password reset tokens
- **EmailVerificationToken**: Entity for email verification

### Value Objects
- **Password**: Encapsulates password validation logic
- **EmailAddress**: Encapsulates email validation logic
- **UserStatus**: Enum (Active, Pending, Locked, Inactive)
- **ActivityType**: Enum for types of user activity
- **MfaType**: Enum (None, Email, Authenticator, SMS)

### Domain Events
- **UserRegisteredEvent**: Raised when a new user registers
- **UserLoggedInEvent**: Raised when a user logs in
- **UserLockedOutEvent**: Raised when a user is locked out
- **PasswordChangedEvent**: Raised when a user changes password
- **UserRoleChangedEvent**: Raised when a user's role changes
- **MfaEnabledEvent**: Raised when MFA is enabled
- **PasswordResetRequestedEvent**: Raised when password reset is requested
- **EmailVerificationRequestedEvent**: Raised when email verification is requested

### Domain Services
- **PasswordPolicyService**: Enforces password policies
- **UserVerificationService**: Handles user verification logic
- **MfaService**: Handles multi-factor authentication
- **LoginAttemptService**: Manages login attempt tracking

### Domain Interfaces
- **IUserRepository**: Repository for ApplicationUser entities
- **IRefreshTokenRepository**: Repository for RefreshToken entities
- **ILoginAttemptRepository**: Repository for LoginAttempt entities
- **IUserActivityRepository**: Repository for UserActivity entities
- **ITokenService**: Service for JWT token management
- **IIdentityService**: Service for identity operations

## Application Layer Components

### Commands
- **RegisterUserCommand**: Registers a new user
- **LoginUserCommand**: Authenticates a user
- **ExternalLoginCommand**: Handles external authentication
- **LogoutUserCommand**: Logs out a user
- **ChangePasswordCommand**: Changes user password
- **ResetPasswordCommand**: Resets user password
- **ForgotPasswordCommand**: Initiates password reset
- **VerifyEmailCommand**: Verifies user email
- **EnableMfaCommand**: Enables MFA for a user
- **DisableMfaCommand**: Disables MFA for a user
- **UpdateUserProfileCommand**: Updates user profile
- **AssignUserRoleCommand**: Assigns role to user
- **RemoveUserRoleCommand**: Removes role from user
- **ApproveRegistrationCommand**: Approves pending registration
- **LockUserAccountCommand**: Locks a user account
- **UnlockUserAccountCommand**: Unlocks a user account
- **DeleteUserCommand**: Deletes a user account
- **RefreshTokenCommand**: Refreshes access token

### Queries
- **GetUserByIdQuery**: Gets user by ID
- **GetUserByEmailQuery**: Gets user by email
- **GetUserListQuery**: Gets list of users
- **GetUserActivityQuery**: Gets user activity log
- **GetUserRolesQuery**: Gets roles for a user
- **GetCurrentUserQuery**: Gets current user info
- **GetLoginAttemptsQuery**: Gets login attempts
- **ValidateTokenQuery**: Validates a token
- **GetMfaSetupInfoQuery**: Gets MFA setup information

### DTOs
- **UserDto**: Data transfer object for User
- **UserSummaryDto**: Summary DTO with limited fields
- **UserActivityDto**: DTO for user activity
- **LoginAttemptDto**: DTO for login attempts
- **AuthResultDto**: DTO for authentication result
- **MfaSetupDto**: DTO for MFA setup
- **TokenDto**: DTO for token information
- **PasswordResetDto**: DTO for password reset
- **UserRoleDto**: DTO for user role information

### Validators
- **RegisterUserCommandValidator**: Validates user registration
- **LoginUserCommandValidator**: Validates login
- **ChangePasswordCommandValidator**: Validates password change
- **ResetPasswordCommandValidator**: Validates password reset
- **UpdateUserProfileCommandValidator**: Validates profile updates
- **AssignUserRoleCommandValidator**: Validates role assignment

### Mapping Profiles
- **AuthenticationMappingProfile**: Maps between entities and DTOs

## Infrastructure Layer Components

### Repositories
- **UserRepository**: Implements IUserRepository
- **RefreshTokenRepository**: Implements IRefreshTokenRepository
- **LoginAttemptRepository**: Implements ILoginAttemptRepository
- **UserActivityRepository**: Implements IUserActivityRepository

### Persistence Configurations
- **ApplicationUserConfiguration**: EF Core configuration for ApplicationUser
- **RefreshTokenConfiguration**: EF Core configuration for RefreshToken
- **LoginAttemptConfiguration**: EF Core configuration for LoginAttempt
- **UserActivityConfiguration**: EF Core configuration for UserActivity
- **PasswordResetTokenConfiguration**: EF Core configuration for PasswordResetToken
- **EmailVerificationTokenConfiguration**: EF Core configuration for EmailVerificationToken

### External Services
- **IdentityService**: Implements IIdentityService using ASP.NET Core Identity
- **TokenService**: Implements ITokenService for JWT token management
- **EmailService**: Sends emails for authentication flows
- **ExternalAuthenticationService**: Handles external authentication providers
- **MfaTokenProvider**: Provides MFA tokens
- **PasswordHasher**: Handles password hashing

## Presentation Layer Components

### API Controllers
- **AuthController**: API endpoints for authentication
- **AccountController**: API endpoints for account management
- **UserController**: API endpoints for user management
- **RoleController**: API endpoints for role management
- **MfaController**: API endpoints for MFA

### Blazor Components
- **LoginForm**: Component for user login
- **RegistrationForm**: Component for user registration
- **PasswordResetForm**: Component for password reset
- **MfaSetup**: Component for MFA setup
- **UserProfile**: Component for user profile management
- **UserManagement**: Component for user management
- **RoleManagement**: Component for role management
- **ActivityLog**: Component for viewing activity logs
- **LoginAttemptLog**: Component for viewing login attempts

### View Models
- **LoginViewModel**: View model for login
- **RegistrationViewModel**: View model for registration
- **PasswordResetViewModel**: View model for password reset
- **MfaSetupViewModel**: View model for MFA setup
- **UserProfileViewModel**: View model for user profile
- **UserManagementViewModel**: View model for user management
- **RoleManagementViewModel**: View model for role management

## Cross-Cutting Concerns

### Logging
- Log user registration and verification
- Log login attempts (successful and failed)
- Log password changes and resets
- Log role changes
- Log MFA events
- Log account lockouts and unlocks
- Log token refresh events

### Caching
- Cache user roles (short duration)
- Cache navigation permissions (short duration)
- Cache current user information (very short duration)
- Cache external authentication providers (longer duration)

### Exception Handling
- Handle invalid credentials
- Handle account lockouts
- Handle token validation failures
- Handle MFA verification failures
- Handle external authentication failures
- Handle password policy violations

## Security Considerations

### Authentication Mechanisms
- JWT-based authentication with refresh tokens
- External authentication providers (Microsoft, Google, Apple)
- Multi-factor authentication options
- Secure password policies
- Account lockout for failed attempts

### Password Security
- Minimum length: 12 characters
- Require uppercase, lowercase, digit, and special character
- Prevent common passwords
- Secure password hashing with ASP.NET Core Identity
- Password expiration policies

### Token Security
- Short-lived access tokens (15 minutes)
- Refresh token rotation
- Token revocation capabilities
- Secure token storage
- CSRF protection

### Access Control
- Role-based access control (Guest, Resident, Committee Member, Board Member, Administrator)
- Permission-based authorization
- Resource-based authorization
- IP-based restrictions for sensitive operations
- Audit logging for all security events

## Implementation Checklist

- [ ] **Domain Layer**
  - [ ] Create entities (ApplicationUser, RefreshToken, etc.)
  - [ ] Define value objects (Password, EmailAddress, etc.)
  - [ ] Implement domain services
  - [ ] Define domain events
  - [ ] Define repository interfaces

- [ ] **Application Layer**
  - [ ] Create commands and command handlers
  - [ ] Create queries and query handlers
  - [ ] Create DTOs
  - [ ] Implement validators
  - [ ] Create mapping profiles

- [ ] **Infrastructure Layer**
  - [ ] Configure ASP.NET Core Identity
  - [ ] Implement repositories
  - [ ] Configure entity persistence
  - [ ] Implement TokenService
  - [ ] Implement EmailService
  - [ ] Implement ExternalAuthenticationService

- [ ] **Presentation Layer**
  - [ ] Create API controllers
  - [ ] Implement login component
  - [ ] Implement registration component
  - [ ] Implement password reset component
  - [ ] Implement MFA setup component
  - [ ] Implement user management components
  - [ ] Create view models

- [ ] **Cross-Cutting Concerns**
  - [ ] Configure logging
  - [ ] Implement caching
  - [ ] Set up exception handling

- [ ] **Security**
  - [ ] Configure JWT authentication
  - [ ] Configure external authentication providers
  - [ ] Implement password policies
  - [ ] Configure account lockout
  - [ ] Implement MFA
  - [ ] Configure CSRF protection

- [ ] **Testing**
  - [ ] Unit tests for domain entities and services
  - [ ] Unit tests for command and query handlers
  - [ ] Integration tests for repositories
  - [ ] Integration tests for API endpoints
  - [ ] Security tests for authentication flows
