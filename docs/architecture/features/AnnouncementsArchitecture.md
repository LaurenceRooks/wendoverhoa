# Announcements Feature Architecture Guide

## Overview

This document provides architectural guidance for implementing the Announcements feature in the Wendover HOA application, mapping components to the Clean Architecture layers, CQRS/MediatR patterns, and identifying cross-cutting concerns.

## Domain Layer Components

### Entities
- **Announcement**: Core entity with title, content, category, importance level, publication dates, etc.
- **AnnouncementCategory**: Categories for organizing announcements
- **AnnouncementAcknowledgment**: Records which users have acknowledged announcements
- **AnnouncementAttachment**: File attachments for announcements
- **AnnouncementSettings**: Global settings for the announcements feature

### Value Objects
- **ImportanceLevel**: Enum (Normal, Important, Critical)
- **AcknowledgmentMethod**: Enum (Web, Email, Mobile)

### Domain Events
- **AnnouncementCreatedEvent**: Raised when announcement is created
- **AnnouncementPublishedEvent**: Raised when announcement is published
- **AnnouncementUpdatedEvent**: Raised when announcement is updated
- **AnnouncementDeletedEvent**: Raised when announcement is deleted
- **AnnouncementAcknowledgedEvent**: Raised when announcement is acknowledged

### Domain Services
- **AnnouncementPublishingService**: Handles publishing logic
- **AnnouncementNotificationService**: Determines notification requirements
- **AnnouncementAcknowledgmentService**: Manages acknowledgment logic

### Domain Interfaces
- **IAnnouncementRepository**: Repository for Announcement entities
- **IAnnouncementCategoryRepository**: Repository for AnnouncementCategory entities
- **IAnnouncementAcknowledgmentRepository**: Repository for AnnouncementAcknowledgment entities
- **IAnnouncementAttachmentRepository**: Repository for AnnouncementAttachment entities
- **IAnnouncementSettingsRepository**: Repository for AnnouncementSettings entity

## Application Layer Components

### Commands
- **CreateAnnouncementCommand**: Creates a new announcement
- **UpdateAnnouncementCommand**: Updates an existing announcement
- **DeleteAnnouncementCommand**: Deletes an announcement
- **PublishAnnouncementCommand**: Publishes an announcement
- **UnpublishAnnouncementCommand**: Unpublishes an announcement
- **AcknowledgeAnnouncementCommand**: Records user acknowledgment
- **CreateAnnouncementCategoryCommand**: Creates a new category
- **UpdateAnnouncementCategoryCommand**: Updates a category
- **DeleteAnnouncementCategoryCommand**: Deletes a category
- **UpdateAnnouncementSettingsCommand**: Updates global settings
- **UploadAttachmentCommand**: Uploads a file attachment
- **DeleteAttachmentCommand**: Deletes a file attachment
- **SendAnnouncementNotificationsCommand**: Sends notifications

### Queries
- **GetAnnouncementByIdQuery**: Gets a specific announcement
- **GetAnnouncementsQuery**: Gets a filtered list of announcements
- **GetAnnouncementCategoriesQuery**: Gets all categories
- **GetAnnouncementCategoryByIdQuery**: Gets a specific category
- **GetAnnouncementAttachmentsQuery**: Gets attachments for an announcement
- **GetAnnouncementAcknowledgmentsQuery**: Gets acknowledgments for an announcement
- **GetPendingAcknowledgmentsQuery**: Gets announcements requiring acknowledgment
- **GetAnnouncementSettingsQuery**: Gets global settings
- **GetImportantAnnouncementsQuery**: Gets critical/important announcements for banner

### DTOs
- **AnnouncementDto**: Data transfer object for Announcement
- **AnnouncementSummaryDto**: Summary DTO with limited fields
- **AnnouncementCategoryDto**: DTO for AnnouncementCategory
- **AnnouncementAttachmentDto**: DTO for AnnouncementAttachment
- **AnnouncementAcknowledgmentDto**: DTO for AnnouncementAcknowledgment
- **AnnouncementSettingsDto**: DTO for AnnouncementSettings

### Validators
- **CreateAnnouncementCommandValidator**: Validates creation command
- **UpdateAnnouncementCommandValidator**: Validates update command
- **CreateAnnouncementCategoryCommandValidator**: Validates category creation
- **UpdateAnnouncementSettingsCommandValidator**: Validates settings updates

### Mapping Profiles
- **AnnouncementMappingProfile**: Maps between entities and DTOs

## Infrastructure Layer Components

### Repositories
- **AnnouncementRepository**: Implements IAnnouncementRepository
- **AnnouncementCategoryRepository**: Implements IAnnouncementCategoryRepository
- **AnnouncementAcknowledgmentRepository**: Implements IAnnouncementAcknowledgmentRepository
- **AnnouncementAttachmentRepository**: Implements IAnnouncementAttachmentRepository
- **AnnouncementSettingsRepository**: Implements IAnnouncementSettingsRepository

### Persistence Configurations
- **AnnouncementConfiguration**: EF Core configuration for Announcement
- **AnnouncementCategoryConfiguration**: EF Core configuration for AnnouncementCategory
- **AnnouncementAcknowledgmentConfiguration**: EF Core configuration for AnnouncementAcknowledgment
- **AnnouncementAttachmentConfiguration**: EF Core configuration for AnnouncementAttachment
- **AnnouncementSettingsConfiguration**: EF Core configuration for AnnouncementSettings

### External Services
- **EmailNotificationService**: Sends email notifications
- **SmsNotificationService**: Sends SMS notifications for critical announcements
- **FileStorageService**: Handles file attachment storage

## Presentation Layer Components

### API Controllers
- **AnnouncementsController**: API endpoints for announcements
- **AnnouncementCategoriesController**: API endpoints for categories
- **AnnouncementSettingsController**: API endpoints for settings
- **AnnouncementAcknowledgmentsController**: API endpoints for acknowledgments

### Blazor Components
- **AnnouncementList**: Displays list of announcements with filtering
- **AnnouncementDetail**: Displays announcement details
- **AnnouncementForm**: Form for creating/editing announcements
- **AnnouncementCategoryManager**: UI for managing categories
- **AnnouncementSettingsManager**: UI for managing settings
- **ImportantAnnouncementsBanner**: Banner for critical/important announcements
- **AnnouncementAcknowledgmentPrompt**: UI for acknowledging announcements
- **AttachmentUploader**: Component for uploading attachments

### View Models
- **AnnouncementViewModel**: View model for announcement details
- **AnnouncementListViewModel**: View model for announcement list
- **AnnouncementFormViewModel**: View model for announcement form
- **AnnouncementCategoryViewModel**: View model for category management
- **AnnouncementSettingsViewModel**: View model for settings management

## Cross-Cutting Concerns

### Logging
- Log announcement creation, updates, deletion
- Log acknowledgments
- Log notification sending
- Log settings changes

### Caching
- Cache announcement list queries (short duration)
- Cache category list (longer duration)
- Cache settings (longer duration)
- Cache important announcements for banner (short duration)

### Exception Handling
- Handle announcement not found
- Handle category not found
- Handle attachment upload failures
- Handle notification sending failures

## Security Considerations

### Role-Based Access Control
- View public announcements: All users
- View all announcements: Authenticated users
- Create/edit announcements: Committee Members, Board Members, Administrators
- Delete announcements: Board Members, Administrators
- Manage categories: Board Members, Administrators
- Manage settings: Administrators
- Acknowledge announcements: Authenticated users

### Data Protection
- Validate and sanitize HTML content
- Validate file attachments for security
- Implement authorization checks on all operations

### API Security
- Implement rate limiting on API endpoints
- Use CSRF protection for form submissions
- Apply appropriate caching headers

## Implementation Checklist

- [ ] **Domain Layer**
  - [ ] Create entities (Announcement, AnnouncementCategory, etc.)
  - [ ] Define value objects (ImportanceLevel, AcknowledgmentMethod)
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
  - [ ] Implement repositories
  - [ ] Configure entity persistence
  - [ ] Implement notification services
  - [ ] Implement file storage service

- [ ] **Presentation Layer**
  - [ ] Create API controllers
  - [ ] Implement Blazor components
  - [ ] Create view models
  - [ ] Implement important announcements banner

- [ ] **Cross-Cutting Concerns**
  - [ ] Configure logging
  - [ ] Implement caching
  - [ ] Set up exception handling

- [ ] **Security**
  - [ ] Configure role-based access control
  - [ ] Implement content sanitization
  - [ ] Configure API security

- [ ] **Testing**
  - [ ] Unit tests for domain entities and services
  - [ ] Unit tests for command and query handlers
  - [ ] Integration tests for repositories
  - [ ] Integration tests for API endpoints
