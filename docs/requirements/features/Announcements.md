# Announcements

## Overview
This document outlines the requirements for the announcements feature of the Wendover HOA web application. This feature will allow board members and authorized users to create, manage, and distribute announcements to the community, while providing residents with an organized way to view and interact with these announcements.

## User Roles
1. **Guest** - Can view public announcements
2. **Resident** - Can view all announcements including resident-only content
3. **Committee Member** - Can create and edit announcements related to their committee
4. **Board Member** - Can create, edit, and approve all announcements
5. **Administrator** - Can manage all announcements and configure announcement settings

## Use Cases

### UC-ANN-01: View Announcements
**Primary Actor:** Guest, Resident, Committee Member, Board Member, or Administrator
**Description:** Allow users to view announcements
**Preconditions:** User has appropriate access level (Guest for public announcements, authenticated users for all announcements)
**Postconditions:** User has viewed announcements

**Main Flow:**
1. User navigates to the announcements page
2. System displays a list of announcements with:
   - Title
   - Publication date
   - Category/tag
   - Brief summary
   - Importance indicator (if applicable)
3. User can:
   - Sort announcements by date, category, or importance
   - Filter announcements by category or date range
   - Search announcements by keyword
4. User selects an announcement to view details
5. System displays the full announcement with:
   - Complete content with rich text formatting
   - Images or attachments (if any)
   - Publication date and last update date
   - Author information
   - Related links (if any)

**Alternative Flows:**
- If no announcements exist, display appropriate message
- If announcements require acknowledgment, prompt user to acknowledge

### UC-ANN-02: Create Announcement
**Primary Actor:** Committee Member, Board Member, or Administrator
**Description:** Allow authorized users to create new announcements
**Preconditions:** User is authenticated as Committee Member, Board Member, or Administrator
**Postconditions:** New announcement is created

**Main Flow:**
1. User navigates to the announcements management page
2. User selects "Create New Announcement"
3. System displays announcement creation form with:
   - Title field
   - Content editor with rich text formatting
   - Category/tag selection
   - Publication options:
     - Immediate publication
     - Scheduled publication
     - Draft mode
   - Importance level selection
   - Attachment upload option
   - Acknowledgment requirement option
   - Notification options
4. User completes the form and submits
5. System validates the input
6. System creates the announcement according to publication settings
7. System confirms successful creation

**Alternative Flows:**
- If validation fails, display specific error messages
- If user selects "Save as Draft," save without publishing
- If user cancels, prompt for confirmation if changes were made

### UC-ANN-03: Edit Announcement
**Primary Actor:** Committee Member, Board Member, or Administrator
**Description:** Allow authorized users to edit existing announcements
**Preconditions:** User is authenticated as Committee Member, Board Member, or Administrator; announcement exists
**Postconditions:** Announcement is updated

**Main Flow:**
1. User navigates to the announcements management page
2. User selects an announcement to edit
3. System displays the announcement in edit mode
4. User makes changes to the announcement
5. User submits the changes
6. System validates the input
7. System updates the announcement
8. System confirms successful update

**Alternative Flows:**
- If validation fails, display specific error messages
- If user cancels, prompt for confirmation if changes were made
- Option to notify users of significant updates

### UC-ANN-04: Delete Announcement
**Primary Actor:** Committee Member, Board Member, or Administrator
**Description:** Allow authorized users to delete announcements
**Preconditions:** User is authenticated as Committee Member, Board Member, or Administrator; announcement exists
**Postconditions:** Announcement is deleted

**Main Flow:**
1. User navigates to the announcements management page
2. User selects an announcement to delete
3. System prompts for confirmation
4. User confirms deletion
5. System deletes the announcement
6. System confirms successful deletion

**Alternative Flows:**
- If user cancels deletion, return to previous state
- Administrators can permanently delete announcements

### UC-ANN-05: Configure Announcement Settings
**Primary Actor:** Administrator
**Description:** Allow administrators to configure global announcement settings
**Preconditions:** User is authenticated as Administrator
**Postconditions:** Announcement settings are updated

**Main Flow:**
1. User navigates to the announcement settings page
2. System displays configuration options:
   - Default announcement categories
   - Announcement retention policy
   - Default notification settings
   - Approval workflow settings
   - Display options and limits
3. User modifies settings as needed
4. User saves changes
5. System updates the configuration
6. System confirms successful update

**Alternative Flows:**
- If validation fails, display specific error messages
- Option to reset to default settings

### UC-ANN-06: Acknowledge Announcement
**Primary Actor:** Resident, Committee Member, Board Member, or Administrator
**Description:** Allow users to acknowledge important announcements
**Preconditions:** User is authenticated, announcement requires acknowledgment
**Postconditions:** User's acknowledgment is recorded

**Main Flow:**
1. User views an announcement that requires acknowledgment
2. System displays acknowledgment prompt
3. User acknowledges the announcement
4. System records the acknowledgment with timestamp
5. System confirms the acknowledgment

**Alternative Flows:**
- User can defer acknowledgment for later
- System can remind users of unacknowledged announcements

## Technical Requirements

1. **Architecture**
   - Implement using Clean Architecture principles
   - Follow SOLID, DRY, KISS, and YAGNI principles
   - Use CQRS pattern with MediatR for announcement operations
   - Create comprehensive unit and integration tests for all functionality

2. **Data Storage**
   - Store announcements in Microsoft SQL Server 2022
   - Use Entity Framework Core 9 for data access
   - Implement proper indexing for efficient queries
   - Support for rich text content storage

3. **Performance**
   - Announcement list should load within 1 second
   - Pagination for large sets of announcements
   - Efficient filtering and searching capabilities
   - Optimize image and attachment handling

4. **Security**
   - Role-based access control for announcement management
   - Audit logging for all announcement operations
   - Input validation and sanitization
   - Pass security scanning using GitHub's free tools (CodeQL, dependency scanning, secret scanning)

5. **Integration**
   - RESTful API endpoints for all announcement operations
   - Swagger/OpenAPI documentation for all endpoints
   - Email notification integration
   - Optional SMS notification for critical announcements
   - Calendar integration for date-based announcements

## API Endpoints

1. **Announcement Management API**
   - `GET /api/announcements` - Get all announcements with filtering options
   - `GET /api/announcements/{id}` - Get specific announcement
   - `POST /api/announcements` - Create new announcement
   - `PUT /api/announcements/{id}` - Update announcement
   - `DELETE /api/announcements/{id}` - Delete announcement

2. **Announcement Settings API**
   - `GET /api/announcements/settings` - Get announcement settings
   - `PUT /api/announcements/settings` - Update announcement settings
   - `GET /api/announcements/categories` - Get announcement categories
   - `POST /api/announcements/categories` - Create announcement category
   - `PUT /api/announcements/categories/{id}` - Update announcement category
   - `DELETE /api/announcements/categories/{id}` - Delete announcement category

3. **Announcement Acknowledgment API**
   - `GET /api/announcements/{id}/acknowledgments` - Get acknowledgments for an announcement
   - `POST /api/announcements/{id}/acknowledge` - Acknowledge an announcement
   - `GET /api/announcements/pending` - Get pending announcements requiring acknowledgment

4. **Announcement Notification API**
   - `POST /api/announcements/{id}/notify` - Send notifications for an announcement
   - `GET /api/announcements/notification-settings` - Get notification settings
   - `PUT /api/announcements/notification-settings` - Update notification settings

## Database Schema

1. **Announcements Table**
   - `AnnouncementId` (PK)
   - `Title`
   - `Content`
   - `CategoryId` (FK to AnnouncementCategories)
   - `ImportanceLevel` (Normal, Important, Critical)
   - `CreatedBy` (FK to Users)
   - `CreatedAt`
   - `UpdatedAt`
   - `PublishedAt` (nullable)
   - `ExpiresAt` (nullable)
   - `RequiresAcknowledgment` (Boolean)
   - `IsPublished` (Boolean)

2. **AnnouncementCategories Table**
   - `CategoryId` (PK)
   - `Name`
   - `Description`
   - `Color`
   - `IsDefault`
   - `CreatedAt`
   - `UpdatedAt`

3. **AnnouncementAcknowledgments Table**
   - `AcknowledgmentId` (PK)
   - `AnnouncementId` (FK to Announcements)
   - `UserId` (FK to Users)
   - `AcknowledgedAt`
   - `AcknowledgmentMethod` (Web, Email, Mobile)

4. **AnnouncementAttachments Table**
   - `AttachmentId` (PK)
   - `AnnouncementId` (FK to Announcements)
   - `FileName`
   - `FileType`
   - `FilePath`
   - `UploadedBy` (FK to Users)
   - `UploadedAt`
   - `FileSize`

## UI/UX Requirements

1. **Announcement List**
   - Clean, responsive grid or list view
   - Visual indicators for importance levels
   - Clear categorization and filtering options
   - Consistent date formatting
   - Responsive design for all device sizes

2. **Announcement Detail**
   - Clear, readable typography
   - Proper handling of embedded media
   - Accessible document attachments
   - Sharing options
   - Print-friendly version

3. **Announcement Display Types**
   - Standard display within announcements page
   - Collapsable notification banner across the home page and all site pages:
     - Critical announcements displayed with red background
     - Important announcements displayed with yellow background
     - Banner shows the most recent critical/important announcements
     - Users can collapse/expand the banner
     - Banner remembers collapsed state per user session
     - Banner includes a counter showing number of critical/important announcements
   - Email notifications for critical announcements
   - Optional SMS notifications for critical announcements

4. **Announcement Creation/Editing**
   - Rich text editor with:
     - Formatting options
     - Image embedding
     - Link creation
     - Table support
   - Preview functionality
   - Autosave feature
   - Responsive design for all device sizes

4. **Accessibility**
   - WCAG 2.1 Level AA compliance
   - Keyboard navigation support
   - Screen reader compatibility
   - Appropriate color contrast
   - Text resizing support

## Acceptance Criteria

1. All use cases can be successfully completed
2. Announcements display correctly on all device sizes
3. Rich text formatting works as expected
4. Attachments can be uploaded and downloaded
5. Filtering, sorting, and searching work correctly
6. Role-based permissions are enforced
7. Notifications are sent according to settings
8. Performance requirements are met
9. All API endpoints function correctly
10. Accessibility requirements are met
