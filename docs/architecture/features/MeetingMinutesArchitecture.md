# Meeting Minutes Feature Architecture Guide

## Overview

This document provides architectural guidance for implementing the Meeting Minutes feature in the Wendover HOA application, mapping components to the Clean Architecture layers, CQRS/MediatR patterns, and identifying cross-cutting concerns. The Meeting Minutes feature provides a structured system for creating, publishing, and archiving meeting minutes for board meetings, committee meetings, and annual homeowner meetings.

## Domain Layer Components

### Entities
- **MeetingMinutes**: Core entity with meeting details and content
- **Motion**: Entity representing a motion made during a meeting
- **ActionItem**: Entity representing a task assigned during a meeting
- **Attendance**: Entity recording attendance at a meeting
- **MinutesAttachment**: Entity representing a file attached to minutes

### Value Objects
- **MeetingType**: Enum (Board, Committee, Annual, Special)
- **MinutesStatus**: Enum (Draft, Pending Approval, Published)
- **MotionResult**: Enum (Passed, Failed, Tabled, Withdrawn)
- **ActionItemPriority**: Enum (Low, Medium, High)
- **ActionItemStatus**: Enum (New, In Progress, Completed, Canceled)
- **AttendanceType**: Enum (Present, Absent, Excused, Late)
- **VoteRecord**: Value object containing vote counts and results

### Domain Events
- **MinutesCreatedEvent**: Raised when minutes are created
- **MinutesUpdatedEvent**: Raised when minutes are updated
- **MinutesPublishedEvent**: Raised when minutes are published
- **MinutesDeletedEvent**: Raised when minutes are deleted
- **MotionRecordedEvent**: Raised when a motion is recorded
- **ActionItemCreatedEvent**: Raised when an action item is created
- **ActionItemStatusChangedEvent**: Raised when action item status changes
- **AttendanceRecordedEvent**: Raised when attendance is recorded

### Domain Services
- **MinutesValidationService**: Validates minutes content and structure
- **MotionValidationService**: Validates motion data
- **ActionItemValidationService**: Validates action item data
- **PublicationService**: Handles publication workflow and rules

### Domain Interfaces
- **IMeetingMinutesRepository**: Repository for MeetingMinutes entities
- **IMotionRepository**: Repository for Motion entities
- **IActionItemRepository**: Repository for ActionItem entities
- **IAttendanceRepository**: Repository for Attendance entities
- **IMinutesAttachmentRepository**: Repository for MinutesAttachment entities

## Application Layer Components

### Commands
- **CreateMinutesCommand**: Creates new meeting minutes
- **UpdateMinutesCommand**: Updates existing minutes
- **PublishMinutesCommand**: Publishes minutes
- **DeleteMinutesCommand**: Deletes minutes
- **RecordMotionCommand**: Records a motion
- **UpdateMotionCommand**: Updates a motion
- **DeleteMotionCommand**: Deletes a motion
- **CreateActionItemCommand**: Creates an action item
- **UpdateActionItemCommand**: Updates an action item
- **UpdateActionItemStatusCommand**: Updates action item status
- **DeleteActionItemCommand**: Deletes an action item
- **RecordAttendanceCommand**: Records attendance
- **UpdateAttendanceCommand**: Updates attendance
- **AddAttachmentCommand**: Adds an attachment to minutes
- **DeleteAttachmentCommand**: Deletes an attachment

### Queries
- **GetMinutesByIdQuery**: Gets specific minutes
- **GetMinutesQuery**: Gets a filtered list of minutes
- **GetMotionsByMinutesIdQuery**: Gets motions for specific minutes
- **GetActionItemsByMinutesIdQuery**: Gets action items for specific minutes
- **GetAttendanceByMinutesIdQuery**: Gets attendance for specific minutes
- **GetAttachmentsByMinutesIdQuery**: Gets attachments for specific minutes
- **GetActionItemsByStatusQuery**: Gets action items by status
- **GetActionItemsByAssigneeQuery**: Gets action items by assignee
- **SearchMinutesQuery**: Searches across minutes content
- **GenerateActionItemReportQuery**: Generates action item report
- **GenerateMotionReportQuery**: Generates motion report
- **GenerateAttendanceReportQuery**: Generates attendance report

### DTOs
- **MeetingMinutesDto**: Data transfer object for MeetingMinutes
- **MeetingMinutesSummaryDto**: Summary DTO with limited fields
- **MotionDto**: DTO for Motion
- **ActionItemDto**: DTO for ActionItem
- **AttendanceDto**: DTO for Attendance
- **MinutesAttachmentDto**: DTO for MinutesAttachment
- **ActionItemReportDto**: DTO for action item reports
- **MotionReportDto**: DTO for motion reports
- **AttendanceReportDto**: DTO for attendance reports
- **MinutesSearchResultDto**: DTO for search results

### Validators
- **CreateMinutesCommandValidator**: Validates minutes creation
- **UpdateMinutesCommandValidator**: Validates minutes updates
- **PublishMinutesCommandValidator**: Validates minutes publication
- **RecordMotionCommandValidator**: Validates motion recording
- **CreateActionItemCommandValidator**: Validates action item creation
- **UpdateActionItemStatusCommandValidator**: Validates status updates
- **RecordAttendanceCommandValidator**: Validates attendance recording

### Mapping Profiles
- **MeetingMinutesMappingProfile**: Maps between entities and DTOs

## Infrastructure Layer Components

### Repositories
- **MeetingMinutesRepository**: Implements IMeetingMinutesRepository
- **MotionRepository**: Implements IMotionRepository
- **ActionItemRepository**: Implements IActionItemRepository
- **AttendanceRepository**: Implements IAttendanceRepository
- **MinutesAttachmentRepository**: Implements IMinutesAttachmentRepository

### Persistence Configurations
- **MeetingMinutesConfiguration**: EF Core configuration for MeetingMinutes
- **MotionConfiguration**: EF Core configuration for Motion
- **ActionItemConfiguration**: EF Core configuration for ActionItem
- **AttendanceConfiguration**: EF Core configuration for Attendance
- **MinutesAttachmentConfiguration**: EF Core configuration for MinutesAttachment

### External Services
- **FileStorageService**: Handles attachment storage
- **EmailNotificationService**: Sends notifications about minutes
- **DocumentGenerationService**: Generates formatted documents
- **FullTextSearchService**: Provides advanced search capabilities
- **ReportGenerationService**: Generates reports from minutes data

## Presentation Layer Components

### API Controllers
- **MeetingMinutesController**: API endpoints for minutes
- **MotionsController**: API endpoints for motions
- **ActionItemsController**: API endpoints for action items
- **AttendanceController**: API endpoints for attendance
- **MinutesAttachmentsController**: API endpoints for attachments
- **MinutesReportsController**: API endpoints for reports

### Blazor Components
- **MinutesList**: Displays list of meeting minutes
- **MinutesDetail**: Displays minutes details
- **MinutesEditor**: Interface for creating/editing minutes
- **MotionRecorder**: Component for recording motions
- **ActionItemManager**: Interface for managing action items
- **AttendanceRecorder**: Component for recording attendance
- **AttachmentUploader**: Component for uploading attachments
- **ActionItemTracker**: Component for tracking action items
- **MinutesSearch**: Search interface for minutes
- **ReportGenerator**: Interface for generating reports

### View Models
- **MinutesListViewModel**: View model for minutes list
- **MinutesDetailViewModel**: View model for minutes details
- **MinutesEditorViewModel**: View model for minutes editor
- **MotionRecorderViewModel**: View model for motion recorder
- **ActionItemManagerViewModel**: View model for action item manager
- **AttendanceRecorderViewModel**: View model for attendance recorder
- **ActionItemTrackerViewModel**: View model for action item tracker
- **ReportGeneratorViewModel**: View model for report generator

## Cross-Cutting Concerns

### Logging
- Log minutes creation, updates, and publication
- Log motion recording and updates
- Log action item creation and status changes
- Log attendance recording
- Log report generation
- Log minutes searches

### Caching
- Cache minutes listings (short duration)
- Cache published minutes content (medium duration)
- Cache action item reports (short duration)
- Cache motion reports (short duration)
- Cache attendance reports (short duration)

### Exception Handling
- Handle minutes not found
- Handle unauthorized access
- Handle validation failures
- Handle file storage failures
- Handle report generation failures

## Security Considerations

### Role-Based Access Control
- View published minutes: All authenticated users
- Create/edit committee minutes: Committee Members
- Create/edit all minutes: Board Members, Administrators
- Publish minutes: Board Members, Administrators
- Delete minutes: Administrators
- Manage action items: Committee Members, Board Members, Administrators

### Data Protection
- Validate and sanitize minutes content
- Secure handling of attachments
- Implement version history for audit trail
- Protect draft minutes from unauthorized access

### API Security
- Implement rate limiting on API endpoints
- Use CSRF protection for form submissions
- Apply appropriate caching headers
- Validate file uploads

## Implementation Checklist

- [ ] **Domain Layer**
  - [ ] Create entities (MeetingMinutes, Motion, ActionItem, etc.)
  - [ ] Define value objects (MeetingType, MinutesStatus, etc.)
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
  - [ ] Implement file storage service
  - [ ] Implement notification service
  - [ ] Implement document generation service
  - [ ] Implement report generation service

- [ ] **Presentation Layer**
  - [ ] Create API controllers
  - [ ] Implement minutes list component
  - [ ] Implement minutes detail component
  - [ ] Implement minutes editor component
  - [ ] Implement motion recorder component
  - [ ] Implement action item manager component
  - [ ] Implement attendance recorder component
  - [ ] Implement report generator component
  - [ ] Create view models

- [ ] **Cross-Cutting Concerns**
  - [ ] Configure logging
  - [ ] Implement caching
  - [ ] Set up exception handling

- [ ] **Security**
  - [ ] Configure role-based access control
  - [ ] Implement content validation
  - [ ] Configure version history
  - [ ] Implement audit logging
  - [ ] Configure API security

- [ ] **Testing**
  - [ ] Unit tests for domain entities and services
  - [ ] Unit tests for command and query handlers
  - [ ] Integration tests for repositories
  - [ ] Integration tests for API endpoints
  - [ ] Tests for report generation
