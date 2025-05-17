# [Feature Name] Architecture Guide

## Overview

This document provides detailed architectural guidance for implementing the [Feature Name] feature in the Wendover HOA application. It maps the feature requirements to the Clean Architecture layers, CQRS/MediatR patterns, cross-cutting concerns, and security considerations to ensure consistent implementation.

## Domain Layer Components

### Entities

| Entity Name | Description | Properties | Validation Rules |
|-------------|-------------|------------|------------------|
| [Entity1] | [Description] | - Id: int<br>- [Property1]: [Type]<br>- [Property2]: [Type] | - [Rule1]<br>- [Rule2] |
| [Entity2] | [Description] | - Id: int<br>- [Property1]: [Type]<br>- [Property2]: [Type] | - [Rule1]<br>- [Rule2] |

### Value Objects

| Value Object Name | Description | Properties | Validation Rules |
|-------------------|-------------|------------|------------------|
| [ValueObject1] | [Description] | - [Property1]: [Type]<br>- [Property2]: [Type] | - [Rule1]<br>- [Rule2] |

### Domain Events

| Event Name | Description | Properties | When Raised |
|------------|-------------|------------|-------------|
| [Event1] | [Description] | - [Property1]: [Type]<br>- [Property2]: [Type] | [When Raised] |

### Domain Services

| Service Name | Description | Methods | Dependencies |
|--------------|-------------|---------|--------------|
| [Service1] | [Description] | - [Method1]: [Signature]<br>- [Method2]: [Signature] | - [Dependency1]<br>- [Dependency2] |

### Domain Interfaces

| Interface Name | Description | Methods | Implemented By |
|----------------|-------------|---------|---------------|
| I[Repository1] | Repository interface for [Entity1] | - GetByIdAsync(int id): Task<[Entity1]><br>- GetAllAsync(): Task<IEnumerable<[Entity1]>><br>- Add([Entity1] entity): void<br>- Update([Entity1] entity): void<br>- Delete([Entity1] entity): void | [Repository1] in Infrastructure Layer |

## Application Layer Components

### Commands

| Command Name | Description | Properties | Handler Dependencies | Authorization |
|--------------|-------------|------------|---------------------|---------------|
| Create[Entity1]Command | Creates a new [Entity1] | - [Property1]: [Type]<br>- [Property2]: [Type] | - I[Repository1]<br>- ICurrentUserService<br>- IUnitOfWork | [Role1], [Role2] |
| Update[Entity1]Command | Updates an existing [Entity1] | - Id: int<br>- [Property1]: [Type]<br>- [Property2]: [Type] | - I[Repository1]<br>- ICurrentUserService<br>- IUnitOfWork | [Role1], [Role2] |
| Delete[Entity1]Command | Deletes an [Entity1] | - Id: int | - I[Repository1]<br>- ICurrentUserService<br>- IUnitOfWork | [Role1], [Role2] |

### Queries

| Query Name | Description | Properties | Handler Dependencies | Authorization | Caching |
|------------|-------------|------------|---------------------|---------------|---------|
| Get[Entity1]ByIdQuery | Gets an [Entity1] by ID | - Id: int | - I[Repository1]<br>- IMapper | [Role1], [Role2], [Role3] | Yes, 5 minutes |
| Get[Entity1]ListQuery | Gets a list of [Entity1] | - [FilterProperty]: [Type]<br>- PageNumber: int<br>- PageSize: int | - I[Repository1]<br>- IMapper | [Role1], [Role2], [Role3] | Yes, 5 minutes |

### DTOs

| DTO Name | Description | Properties | Used By |
|----------|-------------|------------|---------|
| [Entity1]Dto | Data transfer object for [Entity1] | - Id: int<br>- [Property1]: [Type]<br>- [Property2]: [Type]<br>- CreatedBy: string<br>- CreatedAt: DateTime<br>- LastModifiedBy: string<br>- LastModifiedAt: DateTime | Get[Entity1]ByIdQuery, Get[Entity1]ListQuery |
| [Entity1]SummaryDto | Summary DTO for [Entity1] | - Id: int<br>- [Property1]: [Type] | Get[Entity1]ListQuery |

### Validators

| Validator Name | Description | Validation Rules |
|----------------|-------------|------------------|
| Create[Entity1]CommandValidator | Validates Create[Entity1]Command | - [Property1]: NotEmpty, MaxLength(100)<br>- [Property2]: NotEmpty |
| Update[Entity1]CommandValidator | Validates Update[Entity1]Command | - Id: NotEmpty<br>- [Property1]: NotEmpty, MaxLength(100)<br>- [Property2]: NotEmpty |

### Mapping Profiles

```csharp
public class [Feature]MappingProfile : Profile
{
    public [Feature]MappingProfile()
    {
        CreateMap<[Entity1], [Entity1]Dto>()
            .ForMember(d => d.CreatedBy, opt => opt.MapFrom<UserNameResolver, int>(s => s.CreatedById))
            .ForMember(d => d.LastModifiedBy, opt => opt.MapFrom<UserNameResolver, int?>(s => s.LastModifiedById));
            
        CreateMap<[Entity1], [Entity1]SummaryDto>();
    }
}
```

## Infrastructure Layer Components

### Repositories

| Repository Name | Description | Methods | Dependencies |
|-----------------|-------------|---------|--------------|
| [Repository1] | Repository for [Entity1] | - GetByIdAsync(int id): Task<[Entity1]><br>- GetAllAsync(): Task<IEnumerable<[Entity1]>><br>- Add([Entity1] entity): void<br>- Update([Entity1] entity): void<br>- Delete([Entity1] entity): void | - ApplicationDbContext |

### Persistence Configurations

```csharp
public class [Entity1]Configuration : IEntityTypeConfiguration<[Entity1]>
{
    public void Configure(EntityTypeBuilder<[Entity1]> builder)
    {
        builder.HasKey(e => e.Id);
        
        builder.Property(e => e.[Property1])
            .IsRequired()
            .HasMaxLength(100);
            
        builder.Property(e => e.[Property2])
            .IsRequired();
            
        builder.Property(e => e.CreatedById)
            .IsRequired();
            
        builder.Property(e => e.CreatedAt)
            .IsRequired();
            
        // Add indexes
        builder.HasIndex(e => e.[Property1]);
        
        // Add relationships
        builder.HasOne(e => e.CreatedBy)
            .WithMany()
            .HasForeignKey(e => e.CreatedById)
            .OnDelete(DeleteBehavior.Restrict);
            
        builder.HasOne(e => e.LastModifiedBy)
            .WithMany()
            .HasForeignKey(e => e.LastModifiedById)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
```

### External Services

| Service Name | Description | Methods | Dependencies |
|--------------|-------------|---------|--------------|
| [ExternalService1] | [Description] | - [Method1]: [Signature]<br>- [Method2]: [Signature] | - [Dependency1]<br>- [Dependency2] |

## Presentation Layer Components

### API Controllers

| Controller Name | Description | Endpoints | Dependencies |
|-----------------|-------------|----------|--------------|
| [Entity1]Controller | API controller for [Entity1] | - GET /api/[entity1]s: Get[Entity1]ListQuery<br>- GET /api/[entity1]s/{id}: Get[Entity1]ByIdQuery<br>- POST /api/[entity1]s: Create[Entity1]Command<br>- PUT /api/[entity1]s/{id}: Update[Entity1]Command<br>- DELETE /api/[entity1]s/{id}: Delete[Entity1]Command | - IMediator |

### View Models

| View Model Name | Description | Properties | Used By |
|-----------------|-------------|------------|---------|
| [Entity1]ViewModel | View model for [Entity1] | - Id: int<br>- [Property1]: [Type]<br>- [Property2]: [Type]<br>- CreatedBy: string<br>- CreatedAt: DateTime<br>- LastModifiedBy: string<br>- LastModifiedAt: DateTime | [Entity1]Controller |
| Create[Entity1]ViewModel | View model for creating [Entity1] | - [Property1]: [Type]<br>- [Property2]: [Type] | [Entity1]Controller |
| Update[Entity1]ViewModel | View model for updating [Entity1] | - Id: int<br>- [Property1]: [Type]<br>- [Property2]: [Type] | [Entity1]Controller |

### Blazor Components

| Component Name | Description | Parameters | Events | Dependencies |
|----------------|-------------|------------|--------|--------------|
| [Entity1]List | Displays a list of [Entity1] | - PageSize: int<br>- [FilterProperty]: [Type] | - OnSelected: EventCallback<[Entity1]Dto> | - IMediator |
| [Entity1]Detail | Displays details of an [Entity1] | - Id: int | - OnSaved: EventCallback<[Entity1]Dto><br>- OnDeleted: EventCallback<int> | - IMediator |
| [Entity1]Form | Form for creating/editing [Entity1] | - [Entity1]Dto: [Entity1]Dto | - OnSubmit: EventCallback<[Entity1]Dto> | - IMediator |

## Cross-Cutting Concerns

### Logging

| Operation | Log Level | Information to Log |
|-----------|----------|-------------------|
| Create[Entity1] | Information | - User ID<br>- [Entity1] ID<br>- [Key Properties] |
| Update[Entity1] | Information | - User ID<br>- [Entity1] ID<br>- Changed properties |
| Delete[Entity1] | Information | - User ID<br>- [Entity1] ID |
| Get[Entity1]ById | Debug | - User ID<br>- [Entity1] ID |
| Get[Entity1]List | Debug | - User ID<br>- Filter parameters |

### Caching

| Query | Cache Duration | Cache Key Pattern | Invalidation Triggers |
|-------|---------------|-------------------|----------------------|
| Get[Entity1]ByIdQuery | 5 minutes | [entity1]:{id} | Create[Entity1]Command, Update[Entity1]Command, Delete[Entity1]Command |
| Get[Entity1]ListQuery | 5 minutes | [entity1]s:list:{filterParams} | Create[Entity1]Command, Update[Entity1]Command, Delete[Entity1]Command |

### Exception Handling

| Exception | HTTP Status Code | Response Message | Logging Level |
|-----------|-----------------|------------------|--------------|
| [Entity1]NotFoundException | 404 Not Found | [Entity1] with ID {id} was not found | Warning |
| ValidationException | 400 Bad Request | Validation errors | Warning |
| UnauthorizedAccessException | 401 Unauthorized | Unauthorized | Warning |
| ForbiddenAccessException | 403 Forbidden | You do not have permission to access this resource | Warning |

## Security Considerations

### Role-Based Access Control

| Operation | Required Roles | Notes |
|-----------|---------------|-------|
| View [Entity1] List | Guest, Resident, Committee Member, Board Member, Administrator | All users can view the list |
| View [Entity1] Details | Resident, Committee Member, Board Member, Administrator | Authenticated users can view details |
| Create [Entity1] | Board Member, Administrator | Only board members and administrators can create |
| Update [Entity1] | Board Member, Administrator | Only board members and administrators can update |
| Delete [Entity1] | Administrator | Only administrators can delete |

### Data Protection

| Sensitive Data | Protection Method | Notes |
|----------------|------------------|-------|
| [SensitiveProperty1] | Encryption | Encrypted in database using Entity Framework value converters |
| [SensitiveProperty2] | Masking | Partially masked in UI and logs |

### API Security

| Endpoint | Rate Limiting | Additional Security |
|----------|--------------|---------------------|
| GET /api/[entity1]s | 100 requests per minute | Cache-Control headers |
| POST /api/[entity1]s | 20 requests per minute | CSRF protection |
| PUT /api/[entity1]s/{id} | 20 requests per minute | CSRF protection |
| DELETE /api/[entity1]s/{id} | 10 requests per minute | CSRF protection |

## Implementation Checklist

- [ ] **Domain Layer**
  - [ ] Create [Entity1] entity
  - [ ] Create [ValueObject1] value object
  - [ ] Define domain events
  - [ ] Define repository interfaces

- [ ] **Application Layer**
  - [ ] Create commands and command handlers
  - [ ] Create queries and query handlers
  - [ ] Create DTOs
  - [ ] Create validators
  - [ ] Create mapping profiles

- [ ] **Infrastructure Layer**
  - [ ] Implement repositories
  - [ ] Configure entity persistence
  - [ ] Implement external services

- [ ] **Presentation Layer**
  - [ ] Create API controllers
  - [ ] Create Blazor components
  - [ ] Implement view models

- [ ] **Cross-Cutting Concerns**
  - [ ] Configure logging
  - [ ] Configure caching
  - [ ] Implement exception handling

- [ ] **Security**
  - [ ] Configure role-based access control
  - [ ] Implement data protection
  - [ ] Configure API security

- [ ] **Testing**
  - [ ] Unit tests for domain entities and services
  - [ ] Unit tests for command and query handlers
  - [ ] Integration tests for repositories
  - [ ] Integration tests for API endpoints

## Conclusion

This architecture guide provides a comprehensive blueprint for implementing the [Feature Name] feature in accordance with the Wendover HOA application's Clean Architecture, CQRS/MediatR patterns, cross-cutting concerns, and security requirements. Following this guide will ensure consistency and adherence to the project's architectural principles.
