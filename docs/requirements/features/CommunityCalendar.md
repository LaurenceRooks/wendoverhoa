# Community Calendar

## Overview
This document outlines the requirements for the community calendar feature of the Wendover HOA web application. This feature will provide a centralized system for managing and displaying community events, meetings, and important dates, allowing residents to stay informed about activities within the Wendover HOA community in Bedford, Texas.

## User Roles
1. **Guest** - Can view public community events
2. **Resident** - Can view all events and register for participation
3. **Committee Member** - Can create and edit events related to their committee
4. **Board Member** - Can create, edit, and approve all community events
5. **Administrator** - Can manage all events and configure calendar settings

## Use Cases

### UC-CAL-01: View Calendar
**Primary Actor:** Any authenticated user
**Description:** Allow users to view the community calendar
**Preconditions:** User is authenticated
**Postconditions:** User has viewed calendar events

**Main Flow:**
1. User navigates to the calendar page
2. System displays the calendar in month view by default
3. Calendar shows:
   - Community events
   - Board meetings
   - Maintenance schedules
   - Important deadlines
   - Amenity reservations (if applicable)
4. User can:
   - Switch between day, week, month, and list views
   - Navigate to different time periods
   - Filter events by category
   - Search for specific events
5. Events are color-coded by category for easy identification

**Alternative Flows:**
- If no events exist for the selected period, display appropriate message
- User can export calendar to personal calendar applications

### UC-CAL-02: View Event Details
**Primary Actor:** Any authenticated user
**Description:** Allow users to view detailed information about calendar events
**Preconditions:** User is authenticated, event exists
**Postconditions:** User has viewed event details

**Main Flow:**
1. User selects an event from the calendar
2. System displays event details:
   - Event title
   - Date and time (start and end)
   - Location (physical or virtual)
   - Description
   - Organizer/contact information
   - Category
   - Attachments (if any)
   - RSVP status (if applicable)
3. User can:
   - Add event to personal calendar
   - RSVP to the event (if applicable)
   - View location on map (if physical location)
   - Access virtual meeting link (if virtual)

**Alternative Flows:**
- If event has been canceled, display cancellation notice
- If event requires registration, provide registration link

### UC-CAL-03: Create Event
**Primary Actor:** Committee Member, Board Member, or Administrator
**Description:** Allow authorized users to create new calendar events
**Preconditions:** User is authenticated as Committee Member, Board Member, or Administrator
**Postconditions:** New event is created

**Main Flow:**
1. User navigates to the calendar management page
2. User selects "Create New Event"
3. System displays event creation form with:
   - Title field
   - Date and time selection (start and end)
   - Location field (with map integration)
   - Virtual meeting option
   - Description editor with rich text formatting
   - Category selection
   - Recurrence options
   - Attachment upload option
   - RSVP/registration options
   - Notification options
4. User completes the form and submits
5. System validates the input
6. System creates the event
7. System confirms successful creation
8. System sends notifications based on settings

**Alternative Flows:**
- If validation fails, display specific error messages
- If user selects "Save as Draft," save without publishing
- If user cancels, prompt for confirmation if changes were made

### UC-CAL-04: Edit Event
**Primary Actor:** Committee Member, Board Member, or Administrator
**Description:** Allow authorized users to edit existing calendar events
**Preconditions:** User is authenticated as Committee Member, Board Member, or Administrator; event exists
**Postconditions:** Event is updated

**Main Flow:**
1. User navigates to the calendar management page
2. User selects an event to edit
3. System displays the event in edit mode
4. User makes changes to the event
5. User submits the changes
6. System validates the input
7. System updates the event
8. System confirms successful update
9. System sends update notifications based on settings

**Alternative Flows:**
- If validation fails, display specific error messages
- If user cancels, prompt for confirmation if changes were made
- If event is recurring, prompt whether to update all instances or just this one

### UC-CAL-05: Delete Event
**Primary Actor:** Committee Member, Board Member, or Administrator
**Description:** Allow authorized users to delete calendar events
**Preconditions:** User is authenticated as Committee Member, Board Member, or Administrator; event exists
**Postconditions:** Event is deleted

**Main Flow:**
1. User navigates to the calendar management page
2. User selects an event to delete
3. System prompts for confirmation
4. User confirms deletion
5. System deletes the event
6. System confirms successful deletion
7. System sends cancellation notifications based on settings

**Alternative Flows:**
- If user cancels deletion, return to previous state
- If event is recurring, prompt whether to delete all instances or just this one
- Option to cancel event instead of deleting (maintains record but marks as canceled)

### UC-CAL-06: Configure Calendar Settings
**Primary Actor:** Administrator
**Description:** Allow administrators to configure global calendar settings
**Preconditions:** User is authenticated as Administrator
**Postconditions:** Calendar settings are updated

**Main Flow:**
1. User navigates to the calendar settings page
2. System displays configuration options:
   - Default event categories
   - Calendar display options
   - Default notification settings
   - Integration settings
   - Permission settings
3. User modifies settings as needed
4. User saves changes
5. System updates the configuration
6. System confirms successful update

**Alternative Flows:**
- If validation fails, display specific error messages
- Option to reset to default settings

### UC-CAL-07: RSVP to Event
**Primary Actor:** Any authenticated user
**Description:** Allow users to RSVP to events that require attendance tracking
**Preconditions:** User is authenticated, event exists and allows RSVPs
**Postconditions:** User's RSVP is recorded

**Main Flow:**
1. User views an event that allows RSVPs
2. User selects RSVP option (Yes, No, Maybe)
3. System displays confirmation dialog with any additional fields
4. User confirms RSVP
5. System records the RSVP with timestamp
6. System confirms the RSVP
7. System sends confirmation to user

**Alternative Flows:**
- User can update RSVP status later
- If event has reached capacity, place user on waitlist
- If event requires additional information, collect during RSVP process

## Technical Requirements

1. **Architecture**
   - Implement using Clean Architecture principles
   - Follow SOLID, DRY, KISS, and YAGNI principles
   - Use CQRS pattern with MediatR for calendar operations
   - Create comprehensive unit and integration tests for all functionality
   - Pass security scanning using GitHub's free tools (CodeQL, dependency scanning, secret scanning)

2. **Data Storage**
   - Store calendar events in Microsoft SQL Server 2022
   - Use Entity Framework Core 9 for data access
   - Implement proper indexing for efficient date-based queries
   - Support for recurring event patterns
   - Efficient handling of time zones

3. **Performance**
   - Calendar view should load within 1 second
   - Efficient handling of recurring events
   - Optimized queries for date range filtering
   - Pagination for list views with many events

4. **Security**
   - Role-based access control for calendar management
   - Audit logging for all calendar operations
   - Input validation and sanitization
   - Protection against calendar flooding

5. **Integration**
   - RESTful API endpoints for all calendar operations
   - Swagger/OpenAPI documentation for all endpoints
   - iCalendar (ICS) format support for export/import
   - Integration with Microsoft Outlook and Google Calendar
   - Email notification integration

## API Endpoints

1. **Event Management API**
   - `GET /api/calendar/events` - Get all events with filtering options
   - `GET /api/calendar/events/{id}` - Get specific event
   - `POST /api/calendar/events` - Create new event
   - `PUT /api/calendar/events/{id}` - Update event
   - `DELETE /api/calendar/events/{id}` - Delete event

2. **Calendar Settings API**
   - `GET /api/calendar/settings` - Get calendar settings
   - `PUT /api/calendar/settings` - Update calendar settings
   - `GET /api/calendar/categories` - Get event categories
   - `POST /api/calendar/categories` - Create event category
   - `PUT /api/calendar/categories/{id}` - Update event category
   - `DELETE /api/calendar/categories/{id}` - Delete event category

3. **RSVP API**
   - `GET /api/calendar/events/{id}/rsvps` - Get RSVPs for an event
   - `POST /api/calendar/events/{id}/rsvp` - Submit RSVP for an event
   - `PUT /api/calendar/events/{id}/rsvp` - Update RSVP status
   - `DELETE /api/calendar/events/{id}/rsvp` - Cancel RSVP

4. **Calendar Export API**
   - `GET /api/calendar/export/ics` - Export calendar as ICS file
   - `GET /api/calendar/export/ics/{id}` - Export specific event as ICS file
   - `POST /api/calendar/import/ics` - Import events from ICS file

## Database Schema

1. **Events Table**
   - `EventId` (PK)
   - `Title`
   - `Description`
   - `StartDateTime`
   - `EndDateTime`
   - `Location`
   - `CategoryId` (FK to EventCategories)
   - `CreatedBy` (FK to Users)
   - `CreatedAt`
   - `UpdatedAt`
   - `IsRecurring`
   - `RecurrencePattern`
   - `MaxAttendees`
   - `IsPublic`
   - `Status` (Active, Canceled, Completed)

2. **EventCategories Table**
   - `CategoryId` (PK)
   - `Name`
   - `Description`
   - `Color`
   - `IsDefault`
   - `CreatedAt`
   - `UpdatedAt`

3. **EventRSVPs Table**
   - `RSVPId` (PK)
   - `EventId` (FK to Events)
   - `UserId` (FK to Users)
   - `Status` (Attending, Not Attending, Maybe)
   - `GuestCount`
   - `Comments`
   - `RSVPDate`
   - `UpdatedAt`

4. **EventAttachments Table**
   - `AttachmentId` (PK)
   - `EventId` (FK to Events)
   - `FileName`
   - `FileType`
   - `FilePath`
   - `UploadedBy` (FK to Users)
   - `UploadedAt`
   - `FileSize`

## UI/UX Requirements

1. **Calendar Views**
   - Month view (default)
   - Week view
   - Day view
   - List/agenda view
   - Responsive design for all device sizes
   - Intuitive navigation between dates and views

2. **Event Display**
   - Clear visual representation of events
   - Color-coding by category
   - Indication of event duration
   - Proper handling of overlapping events
   - Visual indicators for recurring events

3. **Event Creation/Editing**
   - Intuitive date and time selection
   - Rich text editor for descriptions
   - Map integration for location selection
   - Preview functionality
   - Mobile-friendly input controls

4. **Accessibility**
   - WCAG 2.1 Level AA compliance
   - Keyboard navigation support
   - Screen reader compatibility
   - Appropriate color contrast
   - Text resizing support

## Acceptance Criteria

1. All use cases can be successfully completed
2. Calendar displays correctly on all device sizes
3. Events can be created, edited, and deleted by authorized users
4. Recurring events function correctly
5. Calendar data can be exported to standard formats
6. RSVP functionality works as expected
7. Notifications are sent according to settings
8. Performance requirements are met
9. All API endpoints function correctly
10. Accessibility requirements are met
