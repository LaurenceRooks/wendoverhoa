# User Feedback

## Overview
This document outlines the requirements for the user feedback feature of the Wendover HOA web application. This feature will provide a structured way for residents to submit feedback about the community, amenities, and HOA operations, allowing the board to collect valuable insights and improve community services.

## User Roles
1. **Guest** - Can submit limited feedback without tracking
2. **Resident** - Can submit feedback and view their own feedback history
3. **Committee Member** - Can view feedback related to their committee
4. **Board Member** - Can view, categorize, and respond to feedback
5. **Administrator** - Can manage all feedback and configure feedback settings

## Use Cases

### UC-FEED-01: Submit Feedback
**Primary Actor:** Guest, Resident, Committee Member, Board Member, or Administrator
**Description:** Allow users to submit feedback about various aspects of the community
**Preconditions:** User has appropriate access level (Guest for limited feedback, authenticated users for full feedback)
**Postconditions:** Feedback is submitted and recorded

**Main Flow:**
1. User navigates to the feedback submission page
2. System displays feedback form with:
   - Category selection (e.g., Community, Website, Amenities, Board, Other)
   - Subject field
   - Detailed feedback text area with rich text support
   - Satisfaction rating (optional)
   - Attachment upload option (optional)
   - Anonymous submission option (identity still recorded but hidden from reports)
3. User completes the form and submits
4. System validates the input
5. System records the feedback with timestamp and user information
6. System sends confirmation to the user
7. System notifies appropriate administrators based on category

**Alternative Flows:**
- If validation fails, display specific error messages
- If user selects anonymous submission, confirm understanding of limitations
- Option to save draft for later submission

### UC-FEED-02: View Feedback History
**Primary Actor:** Resident, Committee Member, Board Member, or Administrator
**Description:** Allow users to view their feedback submission history
**Preconditions:** User is authenticated as Resident, Committee Member, Board Member, or Administrator
**Postconditions:** User views their feedback history

**Main Flow:**
1. User navigates to their feedback history page
2. System displays list of user's previous feedback submissions with:
   - Submission date
   - Category
   - Subject
   - Status (New, In Review, Responded, Closed)
   - Response (if available)
3. User can:
   - Sort by date, category, or status
   - Filter by category or status
   - Search within their feedback
4. User selects a feedback item to view details
5. System displays complete feedback details including any responses

**Alternative Flows:**
- If no feedback exists, display appropriate message
- Option to submit new feedback from history page

### UC-FEED-03: Manage Feedback
**Primary Actor:** Board Member or Administrator
**Description:** Allow authorized users to manage submitted feedback
**Preconditions:** User is authenticated as Board Member or Administrator
**Postconditions:** Feedback is managed as requested

**Main Flow:**
1. User navigates to feedback management page
2. System displays list of feedback submissions with:
   - Submission date
   - User information (unless anonymous)
   - Category
   - Subject
   - Status
   - Priority
3. User can:
   - Sort by date, category, status, or priority
   - Filter by various criteria
   - Search within feedback
   - Assign feedback to team members
   - Change status and priority
4. User selects a feedback item to view details
5. System displays complete feedback details
6. User can:
   - Add internal notes
   - Categorize or recategorize feedback
   - Respond to the feedback
   - Change status
   - Link to related feedback items

**Alternative Flows:**
- Bulk update status or category for multiple items
- Export feedback reports in various formats
- Generate statistics and trends from feedback data

### UC-FEED-04: Respond to Feedback
**Primary Actor:** Board Member or Administrator
**Description:** Allow authorized users to respond to user feedback
**Preconditions:** User is authenticated as Board Member or Administrator; feedback exists
**Postconditions:** Response is recorded and sent to user

**Main Flow:**
1. User navigates to feedback management page
2. User selects a feedback item to respond to
3. System displays feedback details and response form
4. User enters response with rich text formatting
5. User submits response
6. System validates input
7. System records response with timestamp
8. System updates feedback status
9. System sends notification to the feedback submitter
10. System confirms successful response

**Alternative Flows:**
- Option to mark response as internal only (not visible to submitter)
- Option to request additional information from submitter
- Template responses for common feedback types

### UC-FEED-05: Generate Feedback Reports
**Primary Actor:** Board Member or Administrator
**Description:** Allow authorized users to generate reports from feedback data
**Preconditions:** User is authenticated as Board Member or Administrator
**Postconditions:** Reports are generated as requested

**Main Flow:**
1. User navigates to feedback reports page
2. System displays available report types:
   - Feedback summary by category
   - Feedback volume trends
   - Response time analysis
   - Satisfaction ratings
   - Custom reports
3. User selects desired report type
4. System displays report parameters:
   - Date ranges
   - Categories to include
   - Status filters
   - Grouping options
   - Output format
5. User configures report parameters
6. User generates report
7. System processes report request
8. System displays report and offers download options

**Alternative Flows:**
- Schedule recurring reports
- Save report configurations for future use
- Export in multiple formats (PDF, Excel, CSV)

### UC-FEED-06: Configure Feedback Settings
**Primary Actor:** Administrator
**Description:** Allow administrators to configure feedback system settings
**Preconditions:** User is authenticated as Administrator
**Postconditions:** Feedback settings are updated

**Main Flow:**
1. User navigates to feedback settings page
2. System displays configuration options:
   - Feedback categories
   - Required and optional fields
   - Attachment settings
   - Notification rules
   - Response templates
   - Automated workflows
3. User modifies settings as needed
4. User saves changes
5. System updates the configuration
6. System confirms successful update

**Alternative Flows:**
- If validation fails, display specific error messages
- Option to reset to default settings

### UC-FEED-07: Delete Feedback
**Primary Actor:** Board Member or Administrator
**Description:** Allow authorized users to delete feedback submissions
**Preconditions:** User is authenticated as Board Member or Administrator; feedback exists
**Postconditions:** Feedback is deleted

**Main Flow:**
1. User navigates to feedback management page
2. User selects feedback to delete
3. System prompts for confirmation
4. User confirms deletion
5. System deletes the feedback
6. System confirms successful deletion

**Alternative Flows:**
- If user cancels deletion, return to previous state
- Administrators can permanently delete feedback

## Technical Requirements

1. **Architecture**
   - Implement using Clean Architecture principles
   - Follow SOLID, DRY, KISS, and YAGNI principles
   - Use CQRS pattern with MediatR for feedback operations
   - Create comprehensive unit and integration tests for all functionality
   - Pass security scanning using GitHub's free tools (CodeQL, dependency scanning, secret scanning)

2. **Data Storage**
   - Store feedback data in Microsoft SQL Server 2022
   - Use Entity Framework Core 9 for data access
   - Implement proper indexing for efficient queries
   - Support for document attachments
   - Maintain complete history of feedback interactions

3. **Security**
   - Role-based access control for feedback management
   - Secure handling of anonymous feedback
   - Audit logging for all feedback operations
   - Protection against spam and abuse
   - Proper sanitization of user input

4. **Performance**
   - Feedback submission should complete within 2 seconds
   - Feedback listings should load within 1 second
   - Efficient search and filtering
   - Responsive performance on all devices
   - Optimized report generation

5. **Integration**
   - RESTful API endpoints for all feedback operations
   - Swagger/OpenAPI documentation for all endpoints
   - Integration with notification system
   - Integration with user management system
   - Email notification integration

## API Endpoints

1. **Feedback Management API**
   - `GET /api/feedback` - Get all feedback with filtering options
   - `GET /api/feedback/{id}` - Get specific feedback
   - `POST /api/feedback` - Submit new feedback
   - `PUT /api/feedback/{id}` - Update feedback
   - `DELETE /api/feedback/{id}` - Delete feedback

2. **Feedback Response API**
   - `GET /api/feedback/{id}/responses` - Get responses for specific feedback
   - `POST /api/feedback/{id}/responses` - Add response to feedback
   - `PUT /api/feedback/{id}/responses/{responseId}` - Update response
   - `DELETE /api/feedback/{id}/responses/{responseId}` - Delete response

3. **Feedback Category API**
   - `GET /api/feedback/categories` - Get all feedback categories
   - `POST /api/feedback/categories` - Create feedback category
   - `PUT /api/feedback/categories/{id}` - Update feedback category
   - `DELETE /api/feedback/categories/{id}` - Delete feedback category

4. **Feedback Report API**
   - `GET /api/feedback/reports/{type}` - Generate specified report
   - `POST /api/feedback/reports/export` - Export feedback data

5. **Feedback Settings API**
   - `GET /api/feedback/settings` - Get feedback settings
   - `PUT /api/feedback/settings` - Update feedback settings

## Database Schema

1. **Feedback Table**
   - `FeedbackId` (PK)
   - `Title`
   - `Description`
   - `CategoryId` (FK to FeedbackCategories)
   - `SubmittedBy` (FK to Users, nullable for anonymous feedback)
   - `SubmittedAt`
   - `Status` (New, In Review, Responded, Closed)
   - `Priority` (Low, Medium, High)
   - `IsPublic` (Boolean)
   - `IsAnonymous` (Boolean)
   - `AssignedTo` (FK to Users, nullable)
   - `DueDate` (nullable)
   - `UpdatedAt`

2. **FeedbackCategories Table**
   - `CategoryId` (PK)
   - `Name`
   - `Description`
   - `DefaultAssignee` (FK to Users, nullable)
   - `IsActive` (Boolean)
   - `CreatedAt`
   - `UpdatedAt`

3. **FeedbackResponses Table**
   - `ResponseId` (PK)
   - `FeedbackId` (FK to Feedback)
   - `ResponseText`
   - `RespondedBy` (FK to Users)
   - `RespondedAt`
   - `IsInternalOnly` (Boolean)
   - `UpdatedAt`

4. **FeedbackAttachments Table**
   - `AttachmentId` (PK)
   - `FeedbackId` (FK to Feedback)
   - `FileName`
   - `FileType`
   - `FilePath`
   - `UploadedBy` (FK to Users)
   - `UploadedAt`
   - `FileSize`

5. **FeedbackHistory Table**
   - `HistoryId` (PK)
   - `FeedbackId` (FK to Feedback)
   - `ChangeType` (Created, Updated, Status Change, etc.)
   - `ChangedBy` (FK to Users)
   - `ChangedAt`
   - `PreviousValue` (JSON, nullable)
   - `NewValue` (JSON, nullable)
   - `Notes` (nullable)



## UI/UX Requirements

1. **Feedback Submission**
   - Clean, intuitive form layout
   - Clear category selection
   - Rich text editor for detailed feedback
   - Simple attachment upload
   - Mobile-friendly input controls
   - Clear submission confirmation

2. **Feedback History**
   - Organized list of previous submissions
   - Clear status indicators
   - Easy access to responses
   - Intuitive sorting and filtering
   - Responsive design for all device sizes

3. **Feedback Management**
   - Comprehensive dashboard for administrators
   - Clear status and priority indicators
   - Efficient batch operations
   - Intuitive response interface
   - Advanced search capabilities

4. **Accessibility**
   - WCAG 2.1 Level AA compliance
   - Keyboard navigation support
   - Screen reader compatibility
   - Appropriate color contrast
   - Text resizing support

## Acceptance Criteria

1. All use cases can be successfully completed
2. Feedback can be submitted, viewed, and managed effectively
3. Reports generate accurate data in appropriate formats
4. Notifications are sent according to settings
5. Performance requirements are met
6. All API endpoints function correctly
7. Security requirements are met, including passing security scans
8. Accessibility requirements are met
9. Anonymous feedback functions correctly
10. Feedback management workflow is efficient and intuitive
