# Community Calendar Feature Architecture Guide

## Overview

This document provides architectural guidance for implementing the Community Calendar feature in the Wendover HOA application, mapping components to the Clean Architecture layers, CQRS/MediatR patterns, and identifying cross-cutting concerns.

## Domain Layer Components

### Entities
- **Event**: Core entity with title, description, start/end dates, location, etc.
- **EventCategory**: Categories for organizing events
- **EventRSVP**: Records user RSVPs to events
- **EventAttachment**: File attachments for events
- **CalendarSettings**: Global settings for the calendar feature
- **RecurrencePattern**: Defines how events recur (daily, weekly, monthly, etc.)

### Value Objects
- **EventStatus**: Enum (Active, Canceled, Completed)
- **RSVPStatus**: Enum (Attending, Not Attending, Maybe)
- **TimeZoneInfo**: Value object for handling time zones
- **EventLocation**: Value object with physical address and/or virtual meeting details

### Domain Events
- **EventCreatedEvent**: Raised when event is created
- **EventUpdatedEvent**: Raised when event is updated
- **EventCanceledEvent**: Raised when event is canceled
- **EventDeletedEvent**: Raised when event is deleted
- **RSVPSubmittedEvent**: Raised when RSVP is submitted
- **RSVPUpdatedEvent**: Raised when RSVP is updated

### Domain Services
- **RecurrenceService**: Handles recurring event logic
- **EventNotificationService**: Determines notification requirements
- **RSVPService**: Manages RSVP logic and capacity management
- **CalendarExportService**: Handles calendar export logic

### Domain Interfaces
- **IEventRepository**: Repository for Event entities
- **IEventCategoryRepository**: Repository for EventCategory entities
- **IEventRSVPRepository**: Repository for EventRSVP entities
- **IEventAttachmentRepository**: Repository for EventAttachment entities
- **ICalendarSettingsRepository**: Repository for CalendarSettings entity

## Application Layer Components

### Commands
- **CreateEventCommand**: Creates a new event
- **UpdateEventCommand**: Updates an existing event
- **CancelEventCommand**: Cancels an event
- **DeleteEventCommand**: Deletes an event
- **CreateRecurringEventCommand**: Creates a recurring event series
- **UpdateRecurringEventCommand**: Updates a recurring event series
- **SubmitRSVPCommand**: Submits an RSVP for an event
- **UpdateRSVPCommand**: Updates an existing RSVP
- **CancelRSVPCommand**: Cancels an RSVP
- **CreateEventCategoryCommand**: Creates a new category
- **UpdateEventCategoryCommand**: Updates a category
- **DeleteEventCategoryCommand**: Deletes a category
- **UpdateCalendarSettingsCommand**: Updates global settings
- **UploadEventAttachmentCommand**: Uploads a file attachment
- **DeleteEventAttachmentCommand**: Deletes a file attachment
- **ImportEventsCommand**: Imports events from ICS file

### Queries
- **GetEventByIdQuery**: Gets a specific event
- **GetEventsQuery**: Gets a filtered list of events
- **GetEventsByDateRangeQuery**: Gets events within a date range
- **GetRecurringEventInstancesQuery**: Gets instances of a recurring event
- **GetEventCategoriesQuery**: Gets all categories
- **GetEventCategoryByIdQuery**: Gets a specific category
- **GetEventAttachmentsQuery**: Gets attachments for an event
- **GetEventRSVPsQuery**: Gets RSVPs for an event
- **GetUserRSVPsQuery**: Gets a user's RSVPs
- **GetCalendarSettingsQuery**: Gets global settings
- **ExportCalendarQuery**: Exports calendar as ICS
- **ExportEventQuery**: Exports specific event as ICS

### DTOs
- **EventDto**: Data transfer object for Event
- **EventSummaryDto**: Summary DTO with limited fields
- **RecurringEventDto**: DTO for recurring event information
- **EventCategoryDto**: DTO for EventCategory
- **EventAttachmentDto**: DTO for EventAttachment
- **EventRSVPDto**: DTO for EventRSVP
- **CalendarSettingsDto**: DTO for CalendarSettings
- **CalendarExportDto**: DTO for calendar export data

### Validators
- **CreateEventCommandValidator**: Validates event creation
- **UpdateEventCommandValidator**: Validates event updates
- **SubmitRSVPCommandValidator**: Validates RSVP submission
- **CreateEventCategoryCommandValidator**: Validates category creation
- **UpdateCalendarSettingsCommandValidator**: Validates settings updates
- **ImportEventsCommandValidator**: Validates ICS import

### Mapping Profiles
- **CalendarMappingProfile**: Maps between entities and DTOs

## Infrastructure Layer Components

### Repositories
- **EventRepository**: Implements IEventRepository
- **EventCategoryRepository**: Implements IEventCategoryRepository
- **EventRSVPRepository**: Implements IEventRSVPRepository
- **EventAttachmentRepository**: Implements IEventAttachmentRepository
- **CalendarSettingsRepository**: Implements ICalendarSettingsRepository

### Persistence Configurations
- **EventConfiguration**: EF Core configuration for Event
- **EventCategoryConfiguration**: EF Core configuration for EventCategory
- **EventRSVPConfiguration**: EF Core configuration for EventRSVP
- **EventAttachmentConfiguration**: EF Core configuration for EventAttachment
- **CalendarSettingsConfiguration**: EF Core configuration for CalendarSettings

### External Services
- **EmailNotificationService**: Sends email notifications
- **ICalendarService**: Handles ICS format import/export
- **GoogleCalendarService**: Integration with Google Calendar
- **OutlookCalendarService**: Integration with Microsoft Outlook
- **FileStorageService**: Handles file attachment storage
- **MappingService**: Integration with mapping services for locations

## Presentation Layer Components

### API Controllers
- **EventsController**: API endpoints for events
- **EventCategoriesController**: API endpoints for categories
- **CalendarSettingsController**: API endpoints for settings
- **EventRSVPsController**: API endpoints for RSVPs
- **CalendarExportController**: API endpoints for calendar export

### Blazor Components
- **CalendarView**: Main calendar component with different view modes
- **MonthView**: Month calendar view
- **WeekView**: Week calendar view
- **DayView**: Day calendar view
- **ListView**: List/agenda view
- **EventDetail**: Displays event details
- **EventForm**: Form for creating/editing events
- **RecurrenceEditor**: UI for configuring recurring events
- **EventCategoryManager**: UI for managing categories
- **CalendarSettingsManager**: UI for managing settings
- **RSVPForm**: Form for submitting RSVPs
- **AttachmentUploader**: Component for uploading attachments
- **CalendarExporter**: UI for exporting calendar

### View Models
- **CalendarViewModel**: View model for calendar display
- **EventViewModel**: View model for event details
- **EventFormViewModel**: View model for event form
- **EventCategoryViewModel**: View model for category management
- **CalendarSettingsViewModel**: View model for settings management
- **RSVPViewModel**: View model for RSVP form

## Cross-Cutting Concerns

### Logging
- Log event creation, updates, cancellation, deletion
- Log RSVP submissions and updates
- Log calendar exports and imports
- Log settings changes

### Caching
- Cache event queries by date range (short duration)
- Cache category list (longer duration)
- Cache settings (longer duration)
- Cache recurring event patterns (medium duration)

### Exception Handling
- Handle event not found
- Handle category not found
- Handle RSVP capacity exceeded
- Handle attachment upload failures
- Handle ICS import/export failures

## Security Considerations

### Role-Based Access Control
- View public events: All users
- View all events: Authenticated users
- Create/edit events: Committee Members, Board Members, Administrators
- Cancel/delete events: Board Members, Administrators
- Manage categories: Board Members, Administrators
- Manage settings: Administrators
- Submit RSVPs: Authenticated users

### Data Protection
- Validate and sanitize event descriptions
- Validate file attachments for security
- Implement authorization checks on all operations
- Protect personal information in RSVPs

### API Security
- Implement rate limiting on API endpoints
- Use CSRF protection for form submissions
- Apply appropriate caching headers

## Implementation Checklist

- [ ] **Domain Layer**
  - [ ] Create entities (Event, EventCategory, EventRSVP, etc.)
  - [ ] Define value objects (EventStatus, RSVPStatus, etc.)
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
  - [ ] Implement calendar import/export services
  - [ ] Implement file storage service

- [ ] **Presentation Layer**
  - [ ] Create API controllers
  - [ ] Implement calendar view components
  - [ ] Implement event management components
  - [ ] Implement RSVP components
  - [ ] Create view models

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
  - [ ] Tests for recurring event logic
