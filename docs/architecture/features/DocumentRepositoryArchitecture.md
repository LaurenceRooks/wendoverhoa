# Document Repository Feature Architecture Guide

## Overview

This document provides architectural guidance for implementing the Document Repository feature in the Wendover HOA application, mapping components to the Clean Architecture layers, CQRS/MediatR patterns, and identifying cross-cutting concerns.

## Domain Layer Components

### Entities
- **Document**: Core entity with metadata about the document
- **DocumentCategory**: Categories for organizing documents
- **DocumentTag**: Tags for document classification
- **DocumentVersion**: Represents different versions of a document
- **DocumentAccess**: Records document access permissions
- **DocumentAcknowledgment**: Records user acknowledgments of documents

### Value Objects
- **AccessLevel**: Enum (Public, Residents, Committee, Board, Admin)
- **FileMetadata**: Contains file type, size, hash information
- **VersionInfo**: Contains version number and change information
- **StoragePath**: Value object for file storage location

### Domain Events
- **DocumentCreatedEvent**: Raised when document is created
- **DocumentUpdatedEvent**: Raised when document is updated
- **DocumentDeletedEvent**: Raised when document is deleted
- **DocumentVersionAddedEvent**: Raised when new version is added
- **DocumentAccessedEvent**: Raised when document is accessed/downloaded
- **DocumentCategoryCreatedEvent**: Raised when category is created
- **DocumentCategoryUpdatedEvent**: Raised when category is updated
- **DocumentCategoryDeletedEvent**: Raised when category is deleted

### Domain Services
- **DocumentAccessService**: Determines access permissions
- **DocumentVersioningService**: Handles versioning logic
- **DocumentSearchService**: Core search functionality
- **FileValidationService**: Validates file types and content

### Domain Interfaces
- **IDocumentRepository**: Repository for Document entities
- **IDocumentCategoryRepository**: Repository for DocumentCategory entities
- **IDocumentTagRepository**: Repository for DocumentTag entities
- **IDocumentVersionRepository**: Repository for DocumentVersion entities
- **IFileStorageService**: Service for file storage operations

## Application Layer Components

### Commands
- **UploadDocumentCommand**: Uploads a new document
- **UpdateDocumentMetadataCommand**: Updates document metadata
- **UpdateDocumentContentCommand**: Updates document content (new version)
- **DeleteDocumentCommand**: Deletes a document
- **CreateDocumentCategoryCommand**: Creates a new category
- **UpdateDocumentCategoryCommand**: Updates a category
- **DeleteDocumentCategoryCommand**: Deletes a category
- **CreateDocumentTagCommand**: Creates a new tag
- **DeleteDocumentTagCommand**: Deletes a tag
- **AddTagToDocumentCommand**: Adds a tag to a document
- **RemoveTagFromDocumentCommand**: Removes a tag from a document
- **RestoreDocumentVersionCommand**: Restores a previous version
- **UpdateRepositorySettingsCommand**: Updates global settings
- **AcknowledgeDocumentCommand**: Records user acknowledgment

### Queries
- **GetDocumentByIdQuery**: Gets a specific document
- **GetDocumentsQuery**: Gets a filtered list of documents
- **GetDocumentContentQuery**: Gets document content for download
- **GetDocumentCategoriesQuery**: Gets all categories
- **GetDocumentCategoryByIdQuery**: Gets a specific category
- **GetDocumentTagsQuery**: Gets all tags
- **GetDocumentVersionsQuery**: Gets versions of a document
- **GetDocumentVersionByIdQuery**: Gets a specific version
- **SearchDocumentsQuery**: Searches documents with filters
- **GetRecentDocumentsQuery**: Gets recently accessed documents
- **GetPopularDocumentsQuery**: Gets most accessed documents
- **GetRepositorySettingsQuery**: Gets global settings
- **GetDocumentAcknowledgmentsQuery**: Gets acknowledgments for a document

### DTOs
- **DocumentDto**: Data transfer object for Document
- **DocumentSummaryDto**: Summary DTO with limited fields
- **DocumentCategoryDto**: DTO for DocumentCategory
- **DocumentTagDto**: DTO for DocumentTag
- **DocumentVersionDto**: DTO for DocumentVersion
- **DocumentSearchResultDto**: DTO for search results
- **RepositorySettingsDto**: DTO for repository settings
- **DocumentUploadResultDto**: DTO for upload results
- **DocumentAccessDto**: DTO for document access information

### Validators
- **UploadDocumentCommandValidator**: Validates document uploads
- **UpdateDocumentMetadataCommandValidator**: Validates metadata updates
- **CreateDocumentCategoryCommandValidator**: Validates category creation
- **SearchDocumentsQueryValidator**: Validates search parameters
- **UpdateRepositorySettingsCommandValidator**: Validates settings updates

### Mapping Profiles
- **DocumentMappingProfile**: Maps between entities and DTOs

## Infrastructure Layer Components

### Repositories
- **DocumentRepository**: Implements IDocumentRepository
- **DocumentCategoryRepository**: Implements IDocumentCategoryRepository
- **DocumentTagRepository**: Implements IDocumentTagRepository
- **DocumentVersionRepository**: Implements IDocumentVersionRepository

### Persistence Configurations
- **DocumentConfiguration**: EF Core configuration for Document
- **DocumentCategoryConfiguration**: EF Core configuration for DocumentCategory
- **DocumentTagConfiguration**: EF Core configuration for DocumentTag
- **DocumentVersionConfiguration**: EF Core configuration for DocumentVersion
- **DocumentTagMappingConfiguration**: EF Core configuration for document-tag mapping

### External Services
- **BlobStorageService**: Implements IFileStorageService for file storage
- **VirusScanningService**: Scans files for malware
- **FullTextSearchService**: Provides advanced search capabilities
- **DocumentPreviewService**: Generates document previews
- **EmailNotificationService**: Sends notifications about documents
- **DocumentEncryptionService**: Handles encryption for sensitive documents

## Presentation Layer Components

### API Controllers
- **DocumentsController**: API endpoints for documents
- **DocumentCategoriesController**: API endpoints for categories
- **DocumentTagsController**: API endpoints for tags
- **DocumentVersionsController**: API endpoints for versions
- **DocumentSearchController**: API endpoints for search
- **RepositorySettingsController**: API endpoints for settings

### Blazor Components
- **DocumentBrowser**: Main document browsing component
- **DocumentCategoryTree**: Displays category hierarchy
- **DocumentDetail**: Displays document details
- **DocumentViewer**: In-browser document viewer
- **DocumentUploader**: Component for uploading documents
- **DocumentVersionHistory**: Displays version history
- **DocumentSearchBox**: Search interface
- **DocumentTagManager**: Interface for managing tags
- **CategoryManager**: Interface for managing categories
- **RepositorySettingsManager**: Interface for managing settings
- **DocumentAccessManager**: Interface for managing access permissions

### View Models
- **DocumentBrowserViewModel**: View model for document browser
- **DocumentDetailViewModel**: View model for document details
- **DocumentUploadViewModel**: View model for document upload
- **CategoryManagerViewModel**: View model for category management
- **RepositorySettingsViewModel**: View model for settings management
- **DocumentSearchViewModel**: View model for document search

## Cross-Cutting Concerns

### Logging
- Log document uploads, updates, and deletions
- Log document access and downloads
- Log category and tag management
- Log settings changes
- Log security-related events

### Caching
- Cache document metadata (short duration)
- Cache category tree (longer duration)
- Cache settings (longer duration)
- Cache document search results (short duration)
- Cache document previews (medium duration)

### Exception Handling
- Handle file not found
- Handle unauthorized access
- Handle storage service failures
- Handle virus detection
- Handle file size/type validation failures

## Security Considerations

### Role-Based Access Control
- View public documents: All users
- View resident documents: Authenticated users
- Upload committee documents: Committee Members
- Manage most documents: Board Members
- Manage all documents and settings: Administrators

### Document-Level Access Control
- Public: Accessible to all users
- Residents: Accessible to authenticated users
- Committee: Accessible to specific committee members
- Board: Accessible to board members
- Admin: Accessible to administrators only

### Data Protection
- Validate and sanitize document metadata
- Scan uploaded files for viruses/malware
- Encrypt sensitive documents
- Implement secure file storage
- Audit logging for all document operations

### API Security
- Implement rate limiting on API endpoints
- Use CSRF protection for form submissions
- Apply appropriate caching headers
- Validate file content types

## Implementation Checklist

- [ ] **Domain Layer**
  - [ ] Create entities (Document, DocumentCategory, DocumentTag, etc.)
  - [ ] Define value objects (AccessLevel, FileMetadata, etc.)
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
  - [ ] Implement virus scanning service
  - [ ] Implement full-text search service
  - [ ] Implement document preview service

- [ ] **Presentation Layer**
  - [ ] Create API controllers
  - [ ] Implement document browser components
  - [ ] Implement document viewer components
  - [ ] Implement document management components
  - [ ] Create view models

- [ ] **Cross-Cutting Concerns**
  - [ ] Configure logging
  - [ ] Implement caching
  - [ ] Set up exception handling

- [ ] **Security**
  - [ ] Configure role-based access control
  - [ ] Implement document-level access control
  - [ ] Configure file validation and scanning
  - [ ] Implement audit logging
  - [ ] Configure API security

- [ ] **Testing**
  - [ ] Unit tests for domain entities and services
  - [ ] Unit tests for command and query handlers
  - [ ] Integration tests for repositories
  - [ ] Integration tests for file storage
  - [ ] Integration tests for API endpoints
  - [ ] Security tests for file upload/download
