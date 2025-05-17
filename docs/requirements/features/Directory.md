# Directory

## Overview
This document outlines the requirements for the directory feature of the Wendover HOA web application. This feature provides a comprehensive system for managing both properties and residents within the Wendover Homeowners Association in Bedford, Texas, with a one-to-many relationship between properties and residents. The directory serves as the central data repository for community information, supporting both administrative functions and resident services.

## User Roles
1. **Guest** - No access to directory information
2. **Resident** - Can view their own property and resident information, and view other residents based on privacy settings
3. **Committee Member** - Same access as Resident
4. **Board Member** - Can manage property and resident records and process change requests
5. **Administrator** - Can manage all directory data and configure directory settings

## Use Cases

### UC-DIR-01: View Directory
**Primary Actor:** Any authenticated user
**Description:** Allow users to browse the community directory
**Preconditions:** User is authenticated
**Postconditions:** User has viewed directory listings

**Main Flow:**
1. User navigates to the directory page
2. System displays directory with two main sections:
   - Properties section showing property listings
   - Residents section showing resident listings
3. For properties, the system displays:
   - Property address
   - Property type
   - Number of residents
   - Last update date
4. For residents, the system displays (based on privacy settings):
   - Name
   - Associated property address
   - Contact information (if permitted)
   - Profile photo (if available and permitted)
5. User can:
   - Toggle between property-centric and resident-centric views
   - Sort by address, name, or move-in date
   - Filter by street, section, or resident status
   - Search for specific properties or residents
   - View in list or grid format

**Alternative Flows:**
- If user has restricted their own visibility, show notice about their current privacy settings
- Option to view only occupied properties or properties with specific characteristics

### UC-DIR-02: View Property Details
**Primary Actor:** Any authenticated user
**Description:** Allow users to view detailed property information
**Preconditions:** User is authenticated, property exists
**Postconditions:** User has viewed property details

**Main Flow:**
1. User selects a property from the directory
2. System displays property details with:
   - Full property information (address, lot number, square footage)
   - Property features and amenities
   - Current residents (with links to resident profiles based on privacy settings)
   - Ownership history (if authorized)
   - Architectural modifications history (if authorized)
   - Documents related to the property (if authorized)
3. User can:
   - Navigate to resident profiles
   - View property location on map
   - Access related property documents (if authorized)

**Alternative Flows:**
- If user has limited access rights, show only permitted information
- If property has special status (foreclosure, vacant), display appropriate indicators

### UC-DIR-03: View Resident Profile
**Primary Actor:** Any authenticated user
**Description:** Allow users to view detailed resident information
**Preconditions:** User is authenticated, resident exists and has permitted profile viewing
**Postconditions:** User has viewed resident profile

**Main Flow:**
1. User selects a resident from the directory
2. System displays resident profile with:
   - Name and profile photo (if available and permitted)
   - Associated property address with link to property details
   - Contact information (based on privacy settings)
   - Household members (based on privacy settings)
   - Move-in date (based on privacy settings)
   - Committees/roles (if applicable)
   - Interests/hobbies (if provided and permitted)
3. User can:
   - Contact resident via email or phone (if permitted)
   - Navigate to associated property details
   - Return to directory listing

**Alternative Flows:**
- If resident has limited profile visibility, show only permitted information
- If resident has completely restricted profile, show minimal information

### UC-DIR-04: Manage Personal Directory Information
**Primary Actor:** Any authenticated user
**Description:** Allow users to manage their own directory information
**Preconditions:** User is authenticated
**Postconditions:** User's directory information is updated

**Main Flow:**
1. User navigates to profile settings
2. User selects "Directory Information"
3. System displays current directory information and privacy settings
4. User can update:
   - Display name preference
   - Contact information (email, phone)
   - Profile photo
   - Household members
   - Interests/hobbies
   - Privacy settings for each information category:
     - Visible to all residents
     - Visible to board members only
     - Not visible in directory
5. User saves changes
6. System validates input
7. System updates directory information
8. System confirms successful update

**Alternative Flows:**
- If validation fails, display specific error messages
- User can opt out of directory entirely
- User can preview how their profile appears to others

### UC-DIR-05: Add New Property
**Primary Actor:** Board Member or Administrator
**Description:** Allow authorized users to add new properties to the system
**Preconditions:** User is authenticated as Board Member or Administrator
**Postconditions:** New property is added to the system

**Main Flow:**
1. User navigates to directory management section
2. User selects "Add New Property"
3. System displays property creation form with:
   - Address information
   - Property details (type, size, lot number)
   - Legal description
   - Initial ownership information
   - Property features
   - HOA fee structure
4. User completes the form and submits
5. System validates input
6. System creates new property record
7. System confirms successful creation
8. System prompts to associate with residents

**Alternative Flows:**
- If validation fails, display specific error messages
- Option to duplicate existing property as starting point
- Bulk import properties from CSV/Excel file

### UC-DIR-06: Add New Resident
**Primary Actor:** Board Member or Administrator
**Description:** Allow authorized users to add new residents to the system
**Preconditions:** User is authenticated as Board Member or Administrator
**Postconditions:** New resident is added to the system

**Main Flow:**
1. User navigates to directory management section
2. User selects "Add New Resident"
3. System displays resident creation form with:
   - Name and contact information
   - Associated property selection (from existing properties)
   - Resident type (owner, tenant, family member)
   - Account credentials (if creating user account)
   - Initial privacy settings
   - Move-in date
4. User completes the form and submits
5. System validates input
6. System creates new resident record
7. System associates resident with selected property
8. System confirms successful creation
9. System sends welcome email to new resident (if email provided)

**Alternative Flows:**
- If validation fails, display specific error messages
- Option to create multiple residents for same property
- Option to create new property if it doesn't exist

### UC-DIR-07: Manage Property Ownership and Residency
**Primary Actor:** Board Member or Administrator
**Description:** Allow authorized users to manage property ownership and residency changes
**Preconditions:** User is authenticated as Board Member or Administrator, property exists
**Postconditions:** Property ownership and residency is updated

**Main Flow:**
1. User navigates to property details
2. User selects "Manage Ownership/Residency"
3. System displays management options:
   - Current owner and resident information
   - Ownership history
   - Residency history
   - Transfer ownership function
   - Update residents function
4. User selects desired management action
5. System displays appropriate form based on selection
6. User completes the form and submits
7. System validates input
8. System records changes with effective date
9. System updates property and resident records
10. System confirms successful update
11. System sends notifications to relevant parties

**Alternative Flows:**
- Option to record partial ownership transfers
- Record property in foreclosure or other special status
- Handle complex residency situations (roommates, multi-family)

### UC-DIR-08: Record Architectural Modification
**Primary Actor:** Board Member or Administrator
**Description:** Allow authorized users to record architectural modifications for properties
**Preconditions:** User is authenticated as Board Member or Administrator, property exists
**Postconditions:** Architectural modification is recorded

**Main Flow:**
1. User navigates to property details
2. User selects "Architectural Modifications"
3. User selects "Add Modification"
4. System displays modification form:
   - Modification type and description
   - Approval information (date, approving body)
   - Start and completion dates
   - Contractor information
   - Permit information
   - Documentation upload
5. User completes the form and submits
6. System validates input
7. System records modification with timestamp
8. System updates property record
9. System confirms successful update

**Alternative Flows:**
- Link to existing architectural request if applicable
- Record modification status (planned, in progress, completed)
- Option to schedule inspection for completed modifications

### UC-DIR-09: Generate Directory Reports
**Primary Actor:** Board Member or Administrator
**Description:** Allow authorized users to generate directory-related reports
**Preconditions:** User is authenticated as Board Member or Administrator
**Postconditions:** Reports are generated as requested

**Main Flow:**
1. User navigates to directory reports section
2. System displays available report types:
   - Property inventory
   - Resident roster
   - Ownership summary
   - Occupancy statistics
   - Custom reports
3. User selects desired report type
4. System displays report parameters:
   - Date ranges
   - Property/resident filters
   - Data fields to include
   - Sorting and grouping options
   - Output format
5. User configures report parameters
6. User generates report
7. System processes report request
8. System displays report and offers download options

**Alternative Flows:**
- Schedule recurring reports
- Save report configurations for future use
- Export in multiple formats (PDF, Excel, CSV)

### UC-DIR-10: Configure Directory Settings
**Primary Actor:** Administrator
**Description:** Allow administrators to configure directory settings
**Preconditions:** User is authenticated as Administrator
**Postconditions:** Directory settings are updated

**Main Flow:**
1. User navigates to directory settings page
2. System displays configuration options:
   - Property types and classifications
   - Resident types and roles
   - Required and optional fields
   - Default privacy settings
   - Directory appearance options
   - Integration settings
3. User modifies settings as needed
4. User saves changes
5. System updates the configuration
6. System confirms successful update

**Alternative Flows:**
- If validation fails, display specific error messages
- Option to reset to default settings

### UC-DIR-11: Search Directory
**Primary Actor:** Any authenticated user
**Description:** Allow users to search for properties and residents
**Preconditions:** User is authenticated
**Postconditions:** User views search results

**Main Flow:**
1. User enters search terms in directory search
2. System searches across:
   - Property addresses and details
   - Resident names
   - Property features
   - Resident interests/hobbies (if permitted)
3. System displays results in categorized sections:
   - Property matches
   - Resident matches
4. Each result shows:
   - Basic identifying information
   - Relevance to search terms
   - Quick access to view full details
5. User can:
   - Filter results further
   - Sort by various attributes
   - Select an item to view details

**Alternative Flows:**
- If no results found, suggest alternative search terms
- Advanced search options for specific criteria

### UC-DIR-12: Delete Property Record
**Primary Actor:** Administrator
**Description:** Allow administrators to delete property records from the system
**Preconditions:** User is authenticated as Administrator; property record exists
**Postconditions:** Property record is deleted

**Main Flow:**
1. Administrator navigates to directory management section
2. Administrator searches for the property to delete
3. Administrator selects property and chooses "Delete Property"
4. System checks for dependencies (residents, payments, architectural records)
5. System displays warning about impact of deletion and dependent records
6. Administrator confirms deletion
7. System deletes the property record and associated dependencies
8. System confirms successful deletion
9. System logs the deletion action with timestamp and user ID

**Alternative Flows:**
- If Administrator cancels deletion, return to previous state
- If property has active residents or financial records, system prevents deletion
- System requires confirmation of understanding that deletion is permanent

### UC-DIR-14: Delete Resident Record
**Primary Actor:** Administrator
**Description:** Allow administrators to delete resident records from the system
**Preconditions:** User is authenticated as Administrator; resident record exists
**Postconditions:** Resident record is deleted

**Main Flow:**
1. Administrator navigates to directory management section
2. Administrator searches for the resident to delete
3. Administrator selects resident and chooses "Delete Resident"
4. System checks for dependencies (user accounts, payments, committee memberships)
5. System displays warning about impact of deletion and dependent records
6. Administrator confirms deletion
7. System deletes the resident record and associated dependencies
8. System confirms successful deletion
9. System logs the deletion action with timestamp and user ID

**Alternative Flows:**
- If Administrator cancels deletion, return to previous state
- If resident has active user account or financial records, system prevents deletion
- System requires confirmation of understanding that deletion is permanent

## Technical Requirements

1. **Architecture**
   - Implement using Clean Architecture principles
   - Follow SOLID, DRY, KISS, and YAGNI principles
   - Use CQRS pattern with MediatR for directory operations
   - Create comprehensive unit and integration tests for all functionality
   - Pass security scanning using GitHub's free tools (CodeQL, dependency scanning, secret scanning)

2. **Data Model**
   - Implement one-to-many relationship between properties and residents
   - Property entity with comprehensive property attributes
   - Resident entity with personal information and privacy settings
   - Historical tracking for both property and resident changes
   - Support for complex ownership and residency scenarios

3. **Data Storage**
   - Store directory data in Microsoft SQL Server 2022
   - Use Entity Framework Core 9 for data access
   - Implement proper indexing for efficient queries
   - Support for document attachments
   - Encrypt sensitive personal information

4. **Security**
   - Role-based and field-level access control
   - Privacy controls for resident information
   - Audit logging for all directory operations
   - Compliance with data protection regulations
   - Prevention of data scraping and bulk exports
   - Secure handling of profile photos

5. **Performance**
   - Directory should load within 1 second
   - Efficient search functionality
   - Pagination for large directories
   - Optimized for property-resident relationship queries
   - Responsive performance on all devices

6. **Integration**
   - RESTful API endpoints for all directory operations
   - Swagger/OpenAPI documentation for all endpoints
   - Integration with user management system
   - Integration with document repository
   - Email notification integration

## API Endpoints

1. **Property Management API**
   - `GET /api/directory/properties` - Get all properties with filtering options
   - `GET /api/directory/properties/{id}` - Get specific property
   - `POST /api/directory/properties` - Create new property
   - `PUT /api/directory/properties/{id}` - Update property
   - `DELETE /api/directory/properties/{id}` - Delete property

2. **Resident Management API**
   - `GET /api/directory/residents` - Get all residents with filtering options
   - `GET /api/directory/residents/{id}` - Get specific resident
   - `POST /api/directory/residents` - Create new resident
   - `PUT /api/directory/residents/{id}` - Update resident
   - `DELETE /api/directory/residents/{id}` - Delete resident

3. **Property-Resident Relationship API**
   - `GET /api/directory/properties/{id}/residents` - Get residents for property
   - `POST /api/directory/properties/{id}/residents` - Add resident to property
   - `DELETE /api/directory/properties/{id}/residents/{residentId}` - Remove resident from property
   - `PUT /api/directory/properties/{id}/ownership` - Update property ownership

4. **Architectural Modification API**
   - `GET /api/directory/properties/{id}/modifications` - Get modifications for property
   - `POST /api/directory/properties/{id}/modifications` - Add modification to property
   - `PUT /api/directory/properties/{id}/modifications/{modId}` - Update modification
   - `DELETE /api/directory/properties/{id}/modifications/{modId}` - Delete modification

5. **Directory Search API**
   - `POST /api/directory/search` - Search directory with advanced filters
   - `GET /api/directory/export` - Export directory data (authorized users only)

## Database Schema

1. **Properties Table**
   - `PropertyId` (PK)
   - `Address`
   - `City`
   - `State`
   - `ZipCode`
   - `LotNumber`
   - `Block`
   - `Section`
   - `PropertyType` (Single Family, Townhouse, etc.)
   - `SquareFootage`
   - `Bedrooms`
   - `Bathrooms`
   - `YearBuilt`
   - `PurchaseDate`
   - `LegalDescription`
   - `Notes`
   - `CreatedAt`
   - `UpdatedAt`

2. **Residents Table**
   - `ResidentId` (PK)
   - `UserId` (FK to Users, nullable)
   - `FirstName`
   - `LastName`
   - `Email`
   - `Phone`
   - `AlternatePhone`
   - `ResidentType` (Owner, Tenant, Family Member)
   - `MoveInDate`
   - `MoveOutDate` (nullable)
   - `EmergencyContactName`
   - `EmergencyContactPhone`
   - `Notes`
   - `ProfilePhotoPath` (nullable)
   - `CreatedAt`
   - `UpdatedAt`

3. **PropertyResidents Table**
   - `PropertyResidentId` (PK)
   - `PropertyId` (FK to Properties)
   - `ResidentId` (FK to Residents)
   - `IsPrimaryOwner` (Boolean)
   - `OwnershipPercentage` (nullable)
   - `StartDate`
   - `EndDate` (nullable)
   - `CreatedAt`
   - `UpdatedAt`

4. **PropertyHistory Table**
   - `PropertyHistoryId` (PK)
   - `PropertyId` (FK to Properties)
   - `ChangeType` (Created, Updated, Ownership Change, etc.)
   - `ChangeDate`
   - `ChangedBy` (FK to Users)
   - `PreviousData` (JSON of previous state)
   - `NewData` (JSON of new state)
   - `Notes`

5. **ArchitecturalModifications Table**
   - `ModificationId` (PK)
   - `PropertyId` (FK to Properties)
   - `ModificationType`
   - `Description`
   - `ApprovalDate` (nullable)
   - `ApprovedBy` (FK to Users, nullable)
   - `StartDate` (nullable)
   - `CompletionDate` (nullable)
   - `ContractorInfo` (nullable)
   - `PermitInfo` (nullable)
   - `Status` (Planned, In Progress, Completed, Rejected)
   - `CreatedAt`
   - `UpdatedAt`

6. **ResidentPrivacySettings Table**
   - `SettingId` (PK)
   - `ResidentId` (FK to Residents)
   - `FieldName`
   - `Visibility` (All Residents, Board Only, Not Visible)
   - `UpdatedAt`



## UI/UX Requirements

1. **Directory Listing**
   - Toggle between property-centric and resident-centric views
   - Clean, responsive grid or list view
   - Clear organization by street or section
   - Intuitive sorting and filtering controls
   - Map view option for geographical representation

2. **Property Detail View**
   - Organized sections for different property aspects
   - Clear presentation of resident relationships
   - Document attachments clearly accessible
   - Modification history in chronological view
   - Ownership history timeline

3. **Resident Profile View**
   - Professional, clean layout
   - Appropriate emphasis on different information types
   - Clear indication of associated property
   - Clear contact options based on privacy settings
   - Respectful presentation of personal information

4. **Directory Management**
   - Intuitive forms with appropriate validation
   - Clear workflow indicators for approval processes
   - Efficient batch operations for multiple properties or residents
   - Document upload with preview
   - Mobile-friendly input controls

5. **Accessibility**
   - WCAG 2.1 Level AA compliance
   - Keyboard navigation support
   - Screen reader compatibility
   - Appropriate color contrast
   - Text resizing support

## Acceptance Criteria

1. All use cases can be successfully completed
2. Directory displays correctly on all device sizes
3. One-to-many relationship between properties and residents functions correctly
4. Privacy settings function correctly at all levels
5. Search functionality returns relevant results quickly
6. Property and resident information can be updated by authorized users
7. Reports generate accurate data in appropriate formats
8. Performance requirements are met
9. All API endpoints function correctly
10. Security requirements are met, including passing security scans
11. Accessibility requirements are met
