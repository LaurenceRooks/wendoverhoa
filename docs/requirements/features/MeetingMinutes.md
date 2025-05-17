# Meeting Minutes

## Overview
This document outlines the requirements for the meeting minutes feature of the Wendover HOA web application. This feature will provide a structured system for creating, publishing, and archiving meeting minutes for board meetings, committee meetings, and annual homeowner meetings, ensuring transparency and proper record-keeping for the Wendover Homeowners Association in Bedford, Texas.

## User Roles
1. **Guest** - Can view public meeting minutes summaries
2. **Resident** - Can view all published meeting minutes
3. **Committee Member** - Can create and edit minutes for their committee meetings
4. **Board Member** - Can create, edit, and approve all meeting minutes
5. **Administrator** - Can manage all aspects of the meeting minutes system and configure settings

## Use Cases

### UC-MIN-01: View Meeting Minutes
**Primary Actor:** Any authenticated user
**Description:** Allow users to view published meeting minutes
**Preconditions:** User is authenticated
**Postconditions:** User has viewed meeting minutes

**Main Flow:**
1. User navigates to the meeting minutes page
2. System displays a list of published meeting minutes with:
   - Meeting date
   - Meeting type (board, committee, annual)
   - Committee name (if applicable)
   - Publication date
   - Brief summary or agenda topics
3. User can:
   - Sort minutes by date or type
   - Filter minutes by date range, type, or committee
   - Search for specific content within minutes
4. User selects meeting minutes to view details
5. System displays the full meeting minutes with:
   - Meeting details (date, time, location)
   - Attendees and absentees
   - Agenda items
   - Discussions
   - Motions and votes
   - Action items
   - Supporting documents (if any)
   - Approval status

**Alternative Flows:**
- If no minutes exist or user has no access to any minutes, display appropriate message
- Board members can see draft minutes with clear draft status indicators

### UC-MIN-02: Create Meeting Minutes
**Primary Actor:** Committee Member, Board Member, or Administrator
**Description:** Allow authorized users to create new meeting minutes
**Preconditions:** User is authenticated as Committee Member, Board Member, or Administrator
**Postconditions:** New meeting minutes are created

**Main Flow:**
1. User navigates to meeting minutes management page
2. User selects "Create New Minutes"
3. System displays minutes creation form with:
   - Meeting details section (date, time, location, type)
   - Attendee management
   - Agenda builder
   - Minutes content editor with sections for:
     - Call to order
     - Roll call
     - Approval of previous minutes
     - Reports
     - Old business
     - New business
     - Announcements
     - Adjournment
   - Motion and vote recorder
   - Action item tracker
   - Document attachment option
4. User completes the form and submits
5. System validates the input
6. System creates the minutes with draft status
7. System confirms successful creation

**Alternative Flows:**
- Option to use meeting template
- Option to import agenda from previous meeting
- Option to save as draft for later completion
- If validation fails, display specific error messages

### UC-MIN-03: Edit Meeting Minutes
**Primary Actor:** Committee Member, Board Member, or Administrator
**Description:** Allow authorized users to edit existing meeting minutes
**Preconditions:** User is authenticated as Committee Member, Board Member, or Administrator; minutes exist
**Postconditions:** Meeting minutes are updated

**Main Flow:**
1. User navigates to meeting minutes management page
2. User selects minutes to edit
3. System displays the minutes in edit mode
4. User makes changes to the minutes
5. User submits the changes
6. System validates the input
7. System updates the minutes
8. System confirms successful update
9. System maintains version history of changes

**Alternative Flows:**
- If validation fails, display specific error messages
- If minutes are already published, create new revision
- Option to revert to previous version

### UC-MIN-04: Publish Meeting Minutes
**Primary Actor:** Committee Member, Board Member, or Administrator
**Description:** Allow authorized users to publish meeting minutes
**Preconditions:** User is authenticated as Committee Member, Board Member, or Administrator; minutes exist in draft state
**Postconditions:** Minutes are published and available to appropriate users

**Main Flow:**
1. User navigates to meeting minutes management page
2. User selects draft minutes to publish
3. System displays publication confirmation dialog with:
   - Final review of minutes content
   - Option to notify residents
   - Option to add publication notes
4. User confirms publication
5. System changes minutes status to published
6. System makes minutes available to authorized users
7. System sends notifications based on settings
8. System confirms successful publication

**Alternative Flows:**
- Option to schedule publication for future date
- Option to require board approval before publication
- If minutes have validation issues, prevent publication and display warnings

### UC-MIN-05: Record Motions and Votes
**Primary Actor:** Committee Member, Board Member, or Administrator
**Description:** Allow authorized users to record motions and votes within meeting minutes
**Preconditions:** User is authenticated as Committee Member, Board Member, or Administrator; minutes exist in edit mode
**Postconditions:** Motion and vote are recorded in minutes

**Main Flow:**
1. User edits meeting minutes
2. User selects "Add Motion" in appropriate section
3. System displays motion recording form with:
   - Motion text
   - Motion maker
   - Motion seconder
   - Discussion notes
   - Vote type (unanimous, roll call, show of hands)
   - Vote results (yes, no, abstain counts)
   - Motion outcome (passed, failed, tabled)
4. User completes the form and submits
5. System validates the input
6. System adds the motion and vote to the minutes
7. System confirms successful addition

**Alternative Flows:**
- Option to record amendments to motions
- Option to record multiple votes for complex motions
- Option to link to related documents or policies

### UC-MIN-06: Track Action Items
**Primary Actor:** Committee Member, Board Member, or Administrator
**Description:** Allow authorized users to track action items from meetings
**Preconditions:** User is authenticated as Committee Member, Board Member, or Administrator; minutes exist
**Postconditions:** Action items are recorded and tracked

**Main Flow:**
1. User edits meeting minutes
2. User selects "Add Action Item" in appropriate section
3. System displays action item form with:
   - Description
   - Assignee(s)
   - Due date
   - Priority
   - Status
   - Notes
4. User completes the form and submits
5. System validates the input
6. System adds the action item to the minutes
7. System adds the action item to the action item tracker
8. System confirms successful addition

**Alternative Flows:**
- Option to link action items to previous action items
- Option to send notifications to assignees

### UC-MIN-07: Delete Meeting Minutes
**Primary Actor:** Committee Member, Board Member, or Administrator
**Description:** Allow authorized users to delete meeting minutes
**Preconditions:** User is authenticated as Committee Member, Board Member, or Administrator; minutes exist
**Postconditions:** Minutes are deleted

**Main Flow:**
1. User navigates to meeting minutes management page
2. User selects minutes to delete
3. System prompts for confirmation
4. User confirms deletion
5. System deletes the minutes
6. System confirms successful deletion

**Alternative Flows:**
- If user cancels deletion, return to previous state
- Administrators can permanently delete minutes

### UC-MIN-08: Generate Meeting Minutes Reports
**Primary Actor:** Committee Member, Board Member, or Administrator
**Description:** Allow authorized users to generate reports from meeting minutes
**Preconditions:** User is authenticated as Committee Member, Board Member, or Administrator
**Postconditions:** Reports are generated as requested

**Main Flow:**
1. User navigates to meeting minutes reports page
2. System displays available report types:
   - Action item status report
   - Motion and vote summary
   - Attendance report
   - Custom reports
3. User selects desired report type
4. System displays report parameters:
   - Date ranges
   - Meeting types
   - Committees
   - Status filters
   - Output format
5. User configures report parameters
6. User generates report
7. System processes report request
8. System displays report and offers download options

**Alternative Flows:**
- Schedule recurring reports
- Save report configurations for future use
- Export in multiple formats (PDF, Word, HTML)

### UC-MIN-08: Configure Meeting Minutes Settings
**Primary Actor:** Administrator
**Description:** Allow administrators to configure meeting minutes system settings
**Preconditions:** User is authenticated as Administrator
**Postconditions:** Meeting minutes settings are updated

**Main Flow:**
1. User navigates to meeting minutes settings page
2. System displays configuration options:
   - Meeting types and templates
   - Required and optional sections
   - Approval workflow settings
   - Notification settings
   - Archive policies
   - Access permissions
3. User modifies settings as needed
4. User saves changes
5. System updates the configuration
6. System confirms successful update

**Alternative Flows:**
- If validation fails, display specific error messages
- Option to reset to default settings

## Technical Requirements

1. **Architecture**
   - Implement using Clean Architecture principles
   - Follow SOLID, DRY, KISS, and YAGNI principles
   - Use CQRS pattern with MediatR for meeting minutes operations
   - Create comprehensive unit and integration tests for all functionality
   - Pass security scanning using GitHub's free tools (CodeQL, dependency scanning, secret scanning)

2. **Data Storage**
   - Store meeting minutes data in Microsoft SQL Server 2022
   - Use Entity Framework Core 9 for data access
   - Implement proper indexing for efficient queries
   - Support for document attachments
   - Maintain complete version history of minutes

3. **Security**
   - Role-based access control for meeting minutes management
   - Audit logging for all meeting minutes operations
   - Data validation to ensure content integrity
   - Secure handling of confidential meeting information
   - Prevention of unauthorized modifications

4. **Performance**
   - Minutes listings should load within 1 second
   - Minutes details should load within 2 seconds
   - Search functionality should return results within 1 second
   - Responsive performance on all devices
   - Efficient handling of document attachments

5. **Integration**
   - RESTful API endpoints for all meeting minutes operations
   - Swagger/OpenAPI documentation for all endpoints
   - Integration with document repository
   - Integration with action item tracking system
   - Email notification integration

## API Endpoints

1. **Meeting Minutes Management API**
   - `GET /api/minutes` - Get all meeting minutes with filtering options
   - `GET /api/minutes/{id}` - Get specific meeting minutes
   - `POST /api/minutes` - Create new meeting minutes
   - `PUT /api/minutes/{id}` - Update meeting minutes
   - `DELETE /api/minutes/{id}` - Delete meeting minutes
   - `POST /api/minutes/{id}/publish` - Publish meeting minutes

2. **Motion and Vote API**
   - `GET /api/minutes/{id}/motions` - Get motions for specific minutes
   - `POST /api/minutes/{id}/motions` - Add motion to minutes
   - `PUT /api/minutes/{id}/motions/{motionId}` - Update motion
   - `DELETE /api/minutes/{id}/motions/{motionId}` - Delete motion
   - `POST /api/minutes/{id}/motions/{motionId}/votes` - Record votes for motion

3. **Action Item API**
   - `GET /api/minutes/{id}/action-items` - Get action items for specific minutes
   - `POST /api/minutes/{id}/action-items` - Add action item to minutes
   - `PUT /api/minutes/{id}/action-items/{itemId}` - Update action item
   - `DELETE /api/minutes/{id}/action-items/{itemId}` - Delete action item
   - `PUT /api/minutes/{id}/action-items/{itemId}/status` - Update action item status

4. **Attendance API**
   - `GET /api/minutes/{id}/attendance` - Get attendance for specific minutes
   - `POST /api/minutes/{id}/attendance` - Record attendance
   - `PUT /api/minutes/{id}/attendance/{userId}` - Update attendance status

5. **Minutes Report API**
   - `GET /api/minutes/reports/{type}` - Generate specified report
   - `POST /api/minutes/reports/export` - Export minutes in specified format

## Database Schema

1. **MeetingMinutes Table**
   - `MinutesId` (PK)
   - `MeetingType` (Board, Committee, Annual, Special)
   - `CommitteeId` (FK to Committees, nullable)
   - `Title`
   - `MeetingDate`
   - `StartTime`
   - `EndTime`
   - `Location`
   - `Agenda`
   - `Content`
   - `Status` (Draft, Pending Approval, Published)
   - `CreatedBy` (FK to Users)
   - `CreatedAt`
   - `UpdatedAt`
   - `PublishedAt` (nullable)
   - `PublishedBy` (FK to Users, nullable)

2. **Motions Table**
   - `MotionId` (PK)
   - `MinutesId` (FK to MeetingMinutes)
   - `MotionText`
   - `MakerName` (FK to Users)
   - `SeconderName` (FK to Users, nullable)
   - `Result` (Passed, Failed, Tabled, Withdrawn)
   - `VotesFor`
   - `VotesAgainst`
   - `VotesAbstained`
   - `Notes`
   - `CreatedAt`
   - `UpdatedAt`

3. **ActionItems Table**
   - `ActionItemId` (PK)
   - `MinutesId` (FK to MeetingMinutes)
   - `Description`
   - `AssignedTo` (FK to Users)
   - `DueDate` (nullable)
   - `Priority` (Low, Medium, High)
   - `Status` (New, In Progress, Completed, Canceled)
   - `Notes`
   - `CreatedAt`
   - `UpdatedAt`
   - `CompletedAt` (nullable)

4. **Attendance Table**
   - `AttendanceId` (PK)
   - `MinutesId` (FK to MeetingMinutes)
   - `UserId` (FK to Users)
   - `AttendanceType` (Present, Absent, Excused, Late)
   - `Notes`
   - `ArrivalTime` (nullable)
   - `DepartureTime` (nullable)

5. **MinutesAttachments Table**
   - `AttachmentId` (PK)
   - `MinutesId` (FK to MeetingMinutes)
   - `FileName`
   - `FileType`
   - `FilePath`
   - `UploadedBy` (FK to Users)
   - `UploadedAt`
   - `FileSize`



## UI/UX Requirements

1. **Minutes Listing**
   - Clean, responsive list view
   - Clear meeting type and date indicators
   - Visual indicators for minutes status
   - Intuitive sorting and filtering
   - Quick access to recent minutes

2. **Minutes Detail View**
   - Clear organization of meeting sections
   - Professional presentation of content
   - Responsive design for all device sizes
   - Print-friendly formatting
   - Prominent display of motions and action items

3. **Minutes Creation/Editing**
   - Intuitive form layout with logical sections
   - Real-time validation feedback
   - Preview capability
   - Autosave functionality
   - Mobile-friendly input controls

4. **Action Item Tracking**
   - Clear status indicators
   - Due date highlighting
   - Assignee information
   - Progress tracking
   - Filtering and sorting options

5. **Accessibility**
   - WCAG 2.1 Level AA compliance
   - Keyboard navigation support
   - Screen reader compatibility
   - Appropriate color contrast
   - Text resizing support

## Acceptance Criteria

1. All use cases can be successfully completed
2. Meeting minutes display correctly on all device sizes
3. Content is properly structured and formatted in minutes
4. Access controls properly restrict minutes access based on roles
5. Version history maintains accurate record of changes
6. Minutes can be created, edited, published, and archived effectively
7. Action items are properly tracked and managed
8. Motions and votes are accurately recorded
9. Performance requirements are met
10. All API endpoints function correctly
11. Security requirements are met, including passing security scans
12. Accessibility requirements are met
