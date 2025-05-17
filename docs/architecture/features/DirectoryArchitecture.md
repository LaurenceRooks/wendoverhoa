# Directory Feature Architecture Guide

## Overview

This document provides architectural guidance for implementing the Directory feature in the Wendover HOA application, mapping components to the Clean Architecture layers, CQRS/MediatR patterns, and identifying cross-cutting concerns. The Directory feature provides a comprehensive system for managing both properties and residents with a one-to-many relationship between properties and residents.

## Domain Layer Components

### Entities
- **Property**: Core entity with address, details, and property characteristics
- **Resident**: Person entity with personal information and privacy settings
- **PropertyResident**: Join entity representing the relationship between properties and residents
- **PropertyHistory**: Records changes to property information
- **ArchitecturalModification**: Records modifications made to properties
- **ResidentPrivacySetting**: Controls visibility of resident information

### Value Objects
- **Address**: Value object containing address components
- **ContactInfo**: Value object for phone, email, and emergency contact
- **ResidentType**: Enum (Owner, Tenant, Family Member)
- **PropertyType**: Enum (Single Family, Townhouse, etc.)
- **ModificationType**: Enum for types of architectural modifications
- **ModificationStatus**: Enum (Planned, In Progress, Completed, Rejected)
- **PrivacyLevel**: Enum (All Residents, Board Only, Not Visible)
- **ChangeType**: Enum for types of property changes

### Domain Events
- **PropertyCreatedEvent**: Raised when property is created
- **PropertyUpdatedEvent**: Raised when property is updated
- **PropertyDeletedEvent**: Raised when property is deleted
- **ResidentCreatedEvent**: Raised when resident is created
- **ResidentUpdatedEvent**: Raised when resident is updated
- **ResidentDeletedEvent**: Raised when resident is deleted
- **PropertyResidentAddedEvent**: Raised when resident is added to property
- **PropertyResidentRemovedEvent**: Raised when resident is removed from property
- **OwnershipChangedEvent**: Raised when property ownership changes
- **ModificationRecordedEvent**: Raised when architectural modification is recorded

### Domain Services
- **PropertyValidationService**: Validates property data
- **ResidentValidationService**: Validates resident data
- **PrivacyService**: Handles privacy rules and filtering
- **OwnershipService**: Manages property ownership logic
- **ResidencyService**: Manages residency relationships

### Domain Interfaces
- **IPropertyRepository**: Repository for Property entities
- **IResidentRepository**: Repository for Resident entities
- **IPropertyResidentRepository**: Repository for PropertyResident relationships
- **IPropertyHistoryRepository**: Repository for PropertyHistory records
- **IArchitecturalModificationRepository**: Repository for ArchitecturalModification records
- **IResidentPrivacySettingRepository**: Repository for ResidentPrivacySetting entities

## Application Layer Components

### Commands
- **CreatePropertyCommand**: Creates a new property
- **UpdatePropertyCommand**: Updates an existing property
- **DeletePropertyCommand**: Deletes a property
- **CreateResidentCommand**: Creates a new resident
- **UpdateResidentCommand**: Updates an existing resident
- **DeleteResidentCommand**: Deletes a resident
- **AddResidentToPropertyCommand**: Associates a resident with a property
- **RemoveResidentFromPropertyCommand**: Removes a resident from a property
- **UpdateOwnershipCommand**: Updates property ownership information
- **RecordArchitecturalModificationCommand**: Records a new modification
- **UpdateArchitecturalModificationCommand**: Updates a modification record
- **DeleteArchitecturalModificationCommand**: Deletes a modification record
- **UpdateResidentPrivacySettingsCommand**: Updates privacy settings
- **UpdateDirectorySettingsCommand**: Updates global directory settings

### Queries
- **GetPropertyByIdQuery**: Gets a specific property
- **GetPropertiesQuery**: Gets a filtered list of properties
- **GetResidentByIdQuery**: Gets a specific resident
- **GetResidentsQuery**: Gets a filtered list of residents
- **GetPropertyResidentsQuery**: Gets residents for a property
- **GetResidentPropertiesQuery**: Gets properties for a resident
- **GetPropertyHistoryQuery**: Gets history for a property
- **GetArchitecturalModificationsQuery**: Gets modifications for a property
- **GetResidentPrivacySettingsQuery**: Gets privacy settings for a resident
- **SearchDirectoryQuery**: Searches across properties and residents
- **GeneratePropertyReportQuery**: Generates property reports
- **GenerateResidentReportQuery**: Generates resident reports
- **GetDirectorySettingsQuery**: Gets global directory settings

### DTOs
- **PropertyDto**: Data transfer object for Property
- **PropertySummaryDto**: Summary DTO with limited property fields
- **ResidentDto**: DTO for Resident with privacy filtering
- **ResidentSummaryDto**: Summary DTO with limited resident fields
- **PropertyResidentDto**: DTO for property-resident relationship
- **PropertyHistoryDto**: DTO for property history records
- **ArchitecturalModificationDto**: DTO for modifications
- **ResidentPrivacySettingDto**: DTO for privacy settings
- **DirectorySearchResultDto**: DTO for search results
- **DirectoryReportDto**: DTO for directory reports
- **DirectorySettingsDto**: DTO for directory settings

### Validators
- **CreatePropertyCommandValidator**: Validates property creation
- **UpdatePropertyCommandValidator**: Validates property updates
- **CreateResidentCommandValidator**: Validates resident creation
- **UpdateResidentCommandValidator**: Validates resident updates
- **AddResidentToPropertyCommandValidator**: Validates resident-property association
- **RecordArchitecturalModificationCommandValidator**: Validates modification records
- **UpdateResidentPrivacySettingsCommandValidator**: Validates privacy settings
- **SearchDirectoryQueryValidator**: Validates search parameters

### Mapping Profiles
- **DirectoryMappingProfile**: Maps between entities and DTOs

## Infrastructure Layer Components

### Repositories
- **PropertyRepository**: Implements IPropertyRepository
- **ResidentRepository**: Implements IResidentRepository
- **PropertyResidentRepository**: Implements IPropertyResidentRepository
- **PropertyHistoryRepository**: Implements IPropertyHistoryRepository
- **ArchitecturalModificationRepository**: Implements IArchitecturalModificationRepository
- **ResidentPrivacySettingRepository**: Implements IResidentPrivacySettingRepository

### Persistence Configurations
- **PropertyConfiguration**: EF Core configuration for Property
- **ResidentConfiguration**: EF Core configuration for Resident
- **PropertyResidentConfiguration**: EF Core configuration for PropertyResident
- **PropertyHistoryConfiguration**: EF Core configuration for PropertyHistory
- **ArchitecturalModificationConfiguration**: EF Core configuration for ArchitecturalModification
- **ResidentPrivacySettingConfiguration**: EF Core configuration for ResidentPrivacySetting

### External Services
- **FileStorageService**: Handles profile photos and document storage
- **EmailNotificationService**: Sends notifications about directory changes
- **GeocodingService**: Converts addresses to geographic coordinates
- **MappingService**: Provides mapping visualization
- **ReportGenerationService**: Generates directory reports
- **UserManagementService**: Integration with user accounts

## Presentation Layer Components

### API Controllers
- **PropertiesController**: API endpoints for properties
- **ResidentsController**: API endpoints for residents
- **PropertyResidentsController**: API endpoints for property-resident relationships
- **ArchitecturalModificationsController**: API endpoints for modifications
- **DirectorySearchController**: API endpoints for directory search
- **DirectoryReportController**: API endpoints for reports
- **DirectorySettingsController**: API endpoints for settings

### Blazor Components
- **DirectoryBrowser**: Main directory browsing component
- **PropertyListView**: Displays property listings
- **ResidentListView**: Displays resident listings
- **PropertyDetail**: Displays property details
- **ResidentProfile**: Displays resident profile
- **PropertyForm**: Form for creating/editing properties
- **ResidentForm**: Form for creating/editing residents
- **PrivacySettingsManager**: Interface for managing privacy settings
- **ArchitecturalModificationManager**: Interface for managing modifications
- **DirectorySearchBox**: Search interface
- **DirectoryReportGenerator**: Interface for generating reports
- **DirectorySettingsManager**: Interface for managing settings
- **DirectoryMapView**: Map visualization of properties

### View Models
- **DirectoryViewModel**: View model for directory browser
- **PropertyViewModel**: View model for property details
- **ResidentViewModel**: View model for resident profile
- **PropertyFormViewModel**: View model for property form
- **ResidentFormViewModel**: View model for resident form
- **PrivacySettingsViewModel**: View model for privacy settings
- **DirectorySearchViewModel**: View model for directory search
- **DirectoryReportViewModel**: View model for report generation
- **DirectorySettingsViewModel**: View model for settings management

## Cross-Cutting Concerns

### Logging
- Log property creation, updates, and deletions
- Log resident creation, updates, and deletions
- Log ownership and residency changes
- Log architectural modifications
- Log privacy setting changes
- Log report generation
- Log directory searches

### Caching
- Cache property listings (short duration)
- Cache resident listings with privacy filtering (short duration)
- Cache directory settings (longer duration)
- Cache search results (very short duration)
- Cache reports (medium duration)

### Exception Handling
- Handle property not found
- Handle resident not found
- Handle unauthorized access to private information
- Handle validation failures
- Handle file upload failures
- Handle report generation failures

## Security Considerations

### Role-Based Access Control
- View directory: Authenticated users
- View detailed resident information: Based on privacy settings
- Manage own directory information: Authenticated users
- Add/edit properties and residents: Board Members, Administrators
- Delete properties and residents: Administrators
- Manage directory settings: Administrators

### Privacy Controls
- Field-level privacy settings for resident information
- Privacy-aware data retrieval in all queries
- Audit logging for all privacy setting changes
- Clear indication of information visibility to users

### Data Protection
- Encrypt sensitive personal information
- Secure handling of profile photos
- Validate and sanitize all input
- Implement rate limiting for directory searches
- Prevent unauthorized bulk exports

## Implementation Checklist

- [ ] **Domain Layer**
  - [ ] Create entities (Property, Resident, PropertyResident, etc.)
  - [ ] Define value objects (Address, ContactInfo, etc.)
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
  - [ ] Implement report generation service
  - [ ] Implement mapping service

- [ ] **Presentation Layer**
  - [ ] Create API controllers
  - [ ] Implement directory browser components
  - [ ] Implement property and resident detail components
  - [ ] Implement property and resident management forms
  - [ ] Implement privacy settings management
  - [ ] Implement report generation interface
  - [ ] Create view models

- [ ] **Cross-Cutting Concerns**
  - [ ] Configure logging
  - [ ] Implement caching
  - [ ] Set up exception handling

- [ ] **Security**
  - [ ] Configure role-based access control
  - [ ] Implement privacy filtering
  - [ ] Configure data protection
  - [ ] Implement audit logging
  - [ ] Configure API security

- [ ] **Testing**
  - [ ] Unit tests for domain entities and services
  - [ ] Unit tests for command and query handlers
  - [ ] Integration tests for repositories
  - [ ] Integration tests for API endpoints
  - [ ] Tests for privacy filtering logic
