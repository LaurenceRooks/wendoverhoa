# Document Repository

## Overview
This document outlines the requirements for the document repository feature of the Wendover HOA web application. This feature will provide a secure, organized system for storing, categorizing, and sharing important HOA documents with residents and board members, ensuring proper access control and version management.

## User Roles
1. **Guest** - Can view and download public documents only
2. **Resident** - Can view and download documents based on their access permissions
3. **Committee Member** - Can upload and manage documents related to their committee
4. **Board Member** - Can upload, edit metadata, and manage most documents
5. **Administrator** - Can manage all documents, configure repository settings, and control access permissions

## Use Cases

### UC-DOC-01: Browse Document Repository
**Primary Actor:** Guest, Resident, Committee Member, Board Member, or Administrator
**Description:** Allow users to browse available documents
**Preconditions:** User has appropriate access level (Guest for public documents, authenticated users for restricted documents)
**Postconditions:** User has viewed document listings

**Main Flow:**
1. User navigates to the document repository page
2. System displays document categories and collections
3. User can:
   - Browse by category (e.g., Bylaws, Financial, Meeting Minutes)
   - View featured or recent documents
   - See document metadata (title, date, type, size)
   - Sort documents by various attributes
   - Filter documents by type, date, or category
   - Search for specific documents
4. System only displays documents the user has permission to access

**Alternative Flows:**
- If no documents exist or user has no access to any documents, display appropriate message
- User can toggle between list and grid views

### UC-DOC-02: View and Download Document
**Primary Actor:** Guest, Resident, Committee Member, Board Member, or Administrator
**Description:** Allow users to view and download documents
**Preconditions:** Document exists and user has appropriate access permission
**Postconditions:** User has viewed or downloaded document

**Main Flow:**
1. User selects a document from the repository
2. System displays document details:
   - Title and description
   - Category and tags
   - Upload/modification dates
   - Version information
   - File type and size
   - Access permissions
3. User can:
   - Preview the document (if supported file type)
   - Download the document
   - View document history/versions
   - See related documents
4. System logs the document access

**Alternative Flows:**
- If document requires acknowledgment, prompt user before allowing download
- If document is not in a format that can be previewed, only offer download option
- If newer version exists, notify user

### UC-DOC-03: Upload Document
**Primary Actor:** Committee Member, Board Member, or Administrator
**Description:** Allow authorized users to upload new documents
**Preconditions:** User is authenticated as Committee Member, Board Member, or Administrator
**Postconditions:** New document is uploaded to repository

**Main Flow:**
1. User navigates to document management page
2. User selects "Upload New Document"
3. System displays upload form with:
   - File selection (with drag-and-drop support)
   - Title and description fields
   - Category and tag selection
   - Access permission settings
   - Version information (if replacing existing document)
   - Notification options
   - Acknowledgment requirement option
4. User completes the form and submits
5. System validates the input and file
6. System uploads and processes the document
7. System confirms successful upload
8. System sends notifications based on settings

**Alternative Flows:**
- If validation fails, display specific error messages
- If file size exceeds limits, notify user
- If document is a new version of existing document, prompt for version notes

### UC-DOC-04: Edit Document Metadata
**Primary Actor:** Committee Member, Board Member, or Administrator
**Description:** Allow authorized users to edit document metadata
**Preconditions:** User is authenticated as Committee Member, Board Member, or Administrator; document exists
**Postconditions:** Document metadata is updated

**Main Flow:**
1. User navigates to document management page
2. User selects a document to edit
3. System displays document metadata in edit mode
4. User makes changes to metadata:
   - Title and description
   - Category and tags
   - Access permissions
   - Featured status
   - Expiration date (if applicable)
5. User submits the changes
6. System validates the input
7. System updates the document metadata
8. System confirms successful update

**Alternative Flows:**
- If validation fails, display specific error messages
- If user cancels, prompt for confirmation if changes were made

### UC-DOC-05: Manage Document Versions
**Primary Actor:** Committee Member, Board Member, or Administrator
**Description:** Allow authorized users to manage document versions
**Preconditions:** User is authenticated as Committee Member, Board Member, or Administrator; document exists
**Postconditions:** Document versions are managed as requested

**Main Flow:**
1. User navigates to document management page
2. User selects a document to manage versions
3. System displays version history
4. User can:
   - Upload a new version
   - Set a specific version as current
   - Add version notes
   - Download specific versions
   - Delete specific versions (with proper authorization)
5. System updates version information
6. System confirms successful action

**Alternative Flows:**
- If deleting the current version, prompt to select a new current version
- Option to notify users of new versions

### UC-DOC-06: Delete Document
**Primary Actor:** Committee Member, Board Member, or Administrator
**Description:** Allow authorized users to delete documents
**Preconditions:** User is authenticated as Committee Member, Board Member, or Administrator; document exists
**Postconditions:** Document is deleted

**Main Flow:**
1. User navigates to document management page
2. User selects a document to delete
3. System prompts for confirmation
4. User confirms deletion
5. System deletes the document
6. System confirms successful deletion

**Alternative Flows:**
- If user cancels deletion, return to previous state
- Administrators can permanently delete documents

### UC-DOC-07: Configure Repository Settings
**Primary Actor:** Administrator
**Description:** Allow administrators to configure global repository settings
**Preconditions:** User is authenticated as Administrator
**Postconditions:** Repository settings are updated

**Main Flow:**
1. User navigates to repository settings page
2. System displays configuration options:
   - Document categories and tags
   - Default access permissions
   - Storage quotas and limits
   - Allowed file types
   - Retention policies
   - Notification settings
3. User modifies settings as needed
4. User saves changes
5. System updates the configuration
6. System confirms successful update

**Alternative Flows:**
- If validation fails, display specific error messages
- Option to reset to default settings

### UC-DOC-08: Search Documents
**Primary Actor:** Guest, Resident, Committee Member, Board Member, or Administrator
**Description:** Allow users to search for documents
**Preconditions:** User has appropriate access level (Guest for public documents, authenticated users for restricted documents)
**Postconditions:** User views search results

**Main Flow:**
1. User enters search terms in document repository search
2. System searches:
   - Document titles and descriptions
   - Content (for supported file types)
   - Categories and tags
   - Metadata
3. System displays results, showing:
   - Document title and snippet/preview
   - Relevance indicators
   - Category and date
   - Quick access to view/download
4. User can:
   - Filter results further
   - Sort by various attributes
   - Select a document to view details

**Alternative Flows:**
- If no results found, suggest alternative search terms
- Advanced search options for power users

## Technical Requirements

1. **Architecture**
   - Implement using Clean Architecture principles
   - Follow SOLID, DRY, KISS, and YAGNI principles
   - Use CQRS pattern with MediatR for document operations
   - Create comprehensive unit and integration tests for all functionality
   - Pass security scanning using GitHub's free tools (CodeQL, dependency scanning, secret scanning)

2. **Data Storage**
   - Store document metadata in Microsoft SQL Server 2022
   - Store document files in secure blob storage
   - Use Entity Framework Core 9 for data access
   - Implement proper indexing for efficient searches
   - Support for document versioning

3. **Security**
   - Role-based and document-level access control
   - Encryption for sensitive documents
   - Secure file handling to prevent unauthorized access
   - Protection against common file upload vulnerabilities
   - Audit logging for all document operations
   - Virus/malware scanning for uploaded files

4. **Integration**
   - RESTful API endpoints for all document operations
   - Swagger/OpenAPI documentation for all endpoints
   - Integration with popular document viewers
   - Support for document sharing via secure links

## API Endpoints

1. **Document Management API**
   - `GET /api/documents` - Get all documents with filtering options
   - `GET /api/documents/{id}` - Get specific document metadata
   - `GET /api/documents/{id}/content` - Download document content
   - `POST /api/documents` - Upload new document
   - `PUT /api/documents/{id}` - Update document metadata
   - `PUT /api/documents/{id}/content` - Update document content (new version)
   - `DELETE /api/documents/{id}` - Delete document

2. **Document Category API**
   - `GET /api/documents/categories` - Get all document categories
   - `GET /api/documents/categories/{id}` - Get specific category
   - `POST /api/documents/categories` - Create new category
   - `PUT /api/documents/categories/{id}` - Update category
   - `DELETE /api/documents/categories/{id}` - Delete category

3. **Document Version API**
   - `GET /api/documents/{id}/versions` - Get all versions of a document
   - `GET /api/documents/{id}/versions/{versionId}` - Get specific version
   - `POST /api/documents/{id}/versions/{versionId}/restore` - Restore previous version

4. **Document Search API**
   - `POST /api/documents/search` - Search documents with advanced filters
   - `GET /api/documents/recent` - Get recently accessed documents
   - `GET /api/documents/popular` - Get most accessed documents

## Database Schema

1. **Documents Table**
   - `DocumentId` (PK)
   - `Title`
   - `Description`
   - `CategoryId` (FK to DocumentCategories)
   - `FileName`
   - `FileType`
   - `FileSize`
   - `StoragePath`
   - `UploadedBy` (FK to Users)
   - `UploadedAt`
   - `UpdatedAt`
   - `AccessLevel` (Public, Residents, Committee, Board, Admin)
   - `VersionNumber`
   - `IsLatestVersion` (Boolean)

2. **DocumentCategories Table**
   - `CategoryId` (PK)
   - `Name`
   - `Description`
   - `ParentCategoryId` (FK to DocumentCategories, for hierarchical categories)
   - `DisplayOrder`
   - `CreatedAt`
   - `UpdatedAt`

3. **DocumentTags Table**
   - `TagId` (PK)
   - `Name`
   - `CreatedAt`

4. **DocumentTagMapping Table**
   - `DocumentId` (FK to Documents)
   - `TagId` (FK to DocumentTags)
   - `CreatedAt`

5. **DocumentVersions Table**
   - `VersionId` (PK)
   - `DocumentId` (FK to Documents)
   - `VersionNumber`
   - `FileName`
   - `FileSize`
   - `StoragePath`
   - `CreatedBy` (FK to Users)
   - `CreatedAt`
   - `ChangeNotes`



5. **Performance**
   - Efficient document browsing and searching
   - Optimized file uploads and downloads
   - Document preview generation
   - Caching for frequently accessed documents
   - Support for large files (up to 50MB)
   - Swagger/OpenAPI documentation for all endpoints
   - Integration with Microsoft Office for online viewing (if applicable)
   - Full-text search capabilities
   - Email notification integration

## UI/UX Requirements

1. **Document Browser**
   - Clean, responsive grid or list view
   - Clear categorization and organization
   - Intuitive navigation between categories
   - Consistent document representation
   - Responsive design for all device sizes

2. **Document Viewer**
   - In-browser preview for supported formats
   - Clear display of document metadata
   - Easy access to download options
   - Version history display
   - Related documents section

3. **Document Management**
   - Drag-and-drop file upload
   - Bulk operations for efficiency
   - Clear feedback on operations
   - Intuitive version management
   - Mobile-friendly controls

4. **Accessibility**
   - WCAG 2.1 Level AA compliance
   - Keyboard navigation support
   - Screen reader compatibility
   - Appropriate color contrast
   - Text resizing support

## Acceptance Criteria

1. All use cases can be successfully completed
2. Document repository displays correctly on all device sizes
3. Documents can be uploaded, viewed, and downloaded
4. Document versioning works correctly
5. Search functionality returns relevant results quickly
6. Access controls properly restrict document access
7. Notifications are sent according to settings
8. Performance requirements are met
9. All API endpoints function correctly
10. Security requirements are met, including passing security scans
