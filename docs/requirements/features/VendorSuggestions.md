# Vendor Suggestions

## Overview
This document outlines the requirements for the vendor suggestions feature of the Wendover HOA web application. This feature will allow residents to submit, view, and vote on suggested vendors for community services, creating a valuable resource of community-vetted service providers while promoting transparency and resident participation.

## User Roles
1. **Guest** - Can view public vendor suggestions
2. **Resident** - Can submit vendor suggestions, view all suggestions, and vote
3. **Committee Member** - Can moderate suggestions related to their committee area
4. **Board Member** - Can moderate all suggestions, manage categories, and highlight verified vendors
5. **Administrator** - Can manage all aspects of the vendor suggestion system and configure settings

## Use Cases

### UC-VEND-01: Submit Vendor Suggestion
**Primary Actor:** Any authenticated user
**Description:** Allow users to submit suggestions for vendors/service providers
**Preconditions:** User is authenticated
**Postconditions:** Vendor suggestion is submitted and pending approval

**Main Flow:**
1. User navigates to the vendor suggestions page
2. User selects "Submit New Vendor"
3. System displays vendor submission form with:
   - Vendor name
   - Service category selection (e.g., Landscaping, Plumbing, Electrical)
   - Contact information (phone, email, website)
   - Description of services
   - User's experience with vendor (optional)
   - Rating (optional)
   - Photos of work performed (optional)
4. User completes the form and submits
5. System validates the input
6. System records the suggestion with pending status
7. System notifies moderators of new submission
8. System confirms submission to user

**Alternative Flows:**
- If validation fails, display specific error messages
- If vendor already exists, prompt user to vote or comment on existing entry
- Option to save draft for later submission

### UC-VEND-02: Browse Vendor Suggestions
**Primary Actor:** Any authenticated user
**Description:** Allow users to browse approved vendor suggestions
**Preconditions:** User is authenticated
**Postconditions:** User views vendor suggestions

**Main Flow:**
1. User navigates to vendor suggestions page
2. System displays categorized list of approved vendors with:
   - Vendor name and basic information
   - Service category
   - Average community rating
   - Vote count
   - Verified status (if applicable)
3. User can:
   - Browse by category
   - Sort by rating, name, or most voted
   - Filter by various criteria
   - Search for specific vendors or services
4. User selects a vendor to view details
5. System displays complete vendor information including community feedback

**Alternative Flows:**
- If no vendors exist in a category, display appropriate message
- Featured or board-approved vendors can be highlighted

### UC-VEND-03: Vote on Vendor Suggestions
**Primary Actor:** Any authenticated user
**Description:** Allow users to vote on vendor suggestions
**Preconditions:** User is authenticated, vendor suggestion exists and is approved
**Postconditions:** User's vote is recorded

**Main Flow:**
1. User views a vendor suggestion
2. User selects upvote or downvote option
3. System validates that user hasn't previously voted for this vendor
4. System records the vote with timestamp
5. System updates the vendor's vote count and ranking
6. System confirms the vote to the user

**Alternative Flows:**
- If user has already voted, allow changing vote
- Option to remove vote entirely
- If user has submitted the vendor, prevent voting on own submission

### UC-VEND-04: Add Review to Vendor
**Primary Actor:** Any authenticated user
**Description:** Allow users to add detailed reviews to vendor suggestions
**Preconditions:** User is authenticated, vendor suggestion exists and is approved
**Postconditions:** User's review is submitted and pending approval

**Main Flow:**
1. User views a vendor suggestion
2. User selects "Add Review"
3. System displays review form with:
   - Rating (1-5 stars)
   - Review text
   - Service date
   - Cost range (optional)
   - Photos of work (optional)
   - Pros and cons
4. User completes the form and submits
5. System validates the input
6. System records the review with pending status
7. System notifies moderators of new review
8. System confirms submission to user

**Alternative Flows:**
- If validation fails, display specific error messages
- If user has already reviewed, allow editing previous review
- Option to submit anonymously (identity still recorded but hidden)

### UC-VEND-05: Moderate Vendor Suggestions
**Primary Actor:** Board Member or Administrator
**Description:** Allow authorized users to moderate vendor suggestions
**Preconditions:** User is authenticated as Board Member or Administrator
**Postconditions:** Vendor suggestions are moderated as requested

**Main Flow:**
1. User navigates to vendor moderation page
2. System displays list of pending vendor suggestions and reviews
3. User can:
   - Review submission details
   - Approve or reject submissions
   - Edit submission content if needed
   - Request additional information from submitter
   - Mark vendors as verified or preferred
4. User takes moderation action
5. System updates the suggestion status
6. System notifies the submitter of the decision
7. System confirms successful moderation

**Alternative Flows:**
- Bulk moderation for multiple submissions
- Add internal notes visible only to moderators
- Flag suspicious submissions for further review

### UC-VEND-06: Manage Vendor Categories
**Primary Actor:** Administrator
**Description:** Allow administrators to manage vendor service categories
**Preconditions:** User is authenticated as Administrator
**Postconditions:** Vendor categories are updated

**Main Flow:**
1. User navigates to vendor category management page
2. System displays existing categories and subcategories
3. User can:
   - Add new categories
   - Edit existing categories
   - Merge categories
   - Deactivate unused categories
   - Reorder category display
4. User makes desired changes
5. System validates input
6. System updates categories
7. System confirms successful update

**Alternative Flows:**
- If category has existing vendors, prompt for handling
- Option to create category hierarchy for better organization

### UC-VEND-07: Delete Vendor Suggestion
**Primary Actor:** Board Member or Administrator
**Description:** Allow authorized users to delete vendor suggestions
**Preconditions:** User is authenticated as Board Member or Administrator; vendor suggestion exists
**Postconditions:** Vendor suggestion is deleted

**Main Flow:**
1. User navigates to vendor management page
2. User selects a vendor suggestion to delete
3. System prompts for confirmation
4. User confirms deletion
5. System deletes the vendor suggestion
6. System confirms successful deletion

**Alternative Flows:**
- If user cancels deletion, return to previous state
- Administrators can permanently delete vendor suggestions

### UC-VEND-08: Generate Vendor Reports
**Primary Actor:** Board Member or Administrator
**Description:** Allow authorized users to generate reports about vendor suggestions
**Preconditions:** User is authenticated as Board Member or Administrator
**Postconditions:** Reports are generated as requested

**Main Flow:**
1. User navigates to vendor reports page
2. System displays available report types:
   - Most popular vendors
   - Category distribution
   - Voting activity
   - Review sentiment analysis
   - Custom reports
3. User selects desired report type
4. System displays report parameters
5. User configures report parameters
6. User generates report
7. System processes report request
8. System displays report and offers download options

**Alternative Flows:**
- Schedule recurring reports
- Save report configurations for future use
- Export in multiple formats (PDF, Excel, CSV)

### UC-VEND-08: Configure Vendor Suggestion Settings
**Primary Actor:** Administrator
**Description:** Allow administrators to configure vendor suggestion system settings
**Preconditions:** User is authenticated as Administrator
**Postconditions:** Vendor suggestion settings are updated

**Main Flow:**
1. User navigates to vendor suggestion settings page
2. System displays configuration options:
   - Required and optional fields
   - Moderation workflow settings
   - Voting rules
   - Review guidelines
   - Notification settings
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
   - Use CQRS pattern with MediatR for vendor suggestion operations
   - Create comprehensive unit and integration tests for all functionality
   - Pass security scanning using GitHub's free tools (CodeQL, dependency scanning, secret scanning)

2. **Data Storage**
   - Store vendor suggestion data in Microsoft SQL Server 2022
   - Use Entity Framework Core 9 for data access
   - Implement proper indexing for efficient queries
   - Support for image attachments
   - Maintain voting history and review history

3. **Security**
   - Role-based access control for vendor suggestion management
   - Prevention of voting fraud and manipulation
   - Audit logging for all operations
   - Protection against spam and inappropriate content
   - Proper sanitization of user input

4. **Performance**
   - Vendor listings should load within 1 second
   - Voting operations should complete within 500ms
   - Efficient search and filtering
   - Responsive performance on all devices
   - Optimized image handling for vendor photos

5. **Integration**
   - RESTful API endpoints for all vendor suggestion operations
   - Swagger/OpenAPI documentation for all endpoints
   - Integration with notification system
   - Integration with user management system
   - Email notification integration

## API Endpoints

1. **Vendor Management API**
   - `GET /api/vendors` - Get all vendor suggestions with filtering options
   - `GET /api/vendors/{id}` - Get specific vendor suggestion
   - `POST /api/vendors` - Submit new vendor suggestion
   - `PUT /api/vendors/{id}` - Update vendor suggestion
   - `DELETE /api/vendors/{id}` - Delete vendor suggestion

2. **Vendor Review API**
   - `GET /api/vendors/{id}/reviews` - Get reviews for specific vendor
   - `POST /api/vendors/{id}/reviews` - Submit review for vendor
   - `PUT /api/vendors/{id}/reviews/{reviewId}` - Update review
   - `DELETE /api/vendors/{id}/reviews/{reviewId}` - Delete review

3. **Vendor Voting API**
   - `GET /api/vendors/{id}/votes` - Get votes for specific vendor
   - `POST /api/vendors/{id}/vote` - Submit vote for vendor
   - `PUT /api/vendors/{id}/vote` - Update vote
   - `DELETE /api/vendors/{id}/vote` - Remove vote

4. **Vendor Category API**
   - `GET /api/vendors/categories` - Get all vendor categories
   - `POST /api/vendors/categories` - Create vendor category
   - `PUT /api/vendors/categories/{id}` - Update vendor category
   - `DELETE /api/vendors/categories/{id}` - Delete vendor category

5. **Vendor Moderation API**
   - `GET /api/vendors/pending` - Get pending vendor suggestions
   - `POST /api/vendors/{id}/approve` - Approve vendor suggestion
   - `POST /api/vendors/{id}/reject` - Reject vendor suggestion
   - `POST /api/vendors/{id}/feature` - Mark vendor as featured

6. **Vendor Report API**
   - `GET /api/vendors/reports/{type}` - Generate specified report
   - `POST /api/vendors/reports/export` - Export vendor data

## Database Schema

1. **Vendors Table**
   - `VendorId` (PK)
   - `Name`
   - `CategoryId` (FK to VendorCategories)
   - `Description`
   - `ContactName`
   - `ContactPhone`
   - `ContactEmail`
   - `Website`
   - `Address`
   - `City`
   - `State`
   - `ZipCode`
   - `SubmittedBy` (FK to Users)
   - `SubmittedAt`
   - `Status` (Pending, Approved, Rejected)
   - `IsVerified` (Boolean)
   - `IsFeatured` (Boolean)
   - `AverageRating`
   - `VoteCount`
   - `ReviewCount`
   - `UpdatedAt`

2. **VendorCategories Table**
   - `CategoryId` (PK)
   - `Name`
   - `Description`
   - `ParentCategoryId` (FK to VendorCategories, for hierarchical categories)
   - `DisplayOrder`
   - `IsActive` (Boolean)
   - `CreatedAt`
   - `UpdatedAt`

3. **VendorReviews Table**
   - `ReviewId` (PK)
   - `VendorId` (FK to Vendors)
   - `ReviewedBy` (FK to Users)
   - `Rating` (1-5)
   - `ReviewText`
   - `Pros`
   - `Cons`
   - `CostRange`
   - `DateOfService`
   - `IsAnonymous` (Boolean)
   - `Status` (Pending, Approved, Rejected)
   - `SubmittedAt`
   - `UpdatedAt`

4. **VendorVotes Table**
   - `VoteId` (PK)
   - `VendorId` (FK to Vendors)
   - `UserId` (FK to Users)
   - `VoteType` (Upvote, Downvote)
   - `VotedAt`
   - `UpdatedAt`

5. **VendorPhotos Table**
   - `PhotoId` (PK)
   - `VendorId` (FK to Vendors)
   - `ReviewId` (FK to VendorReviews, nullable)
   - `FileName`
   - `FilePath`
   - `Description`
   - `UploadedBy` (FK to Users)
   - `UploadedAt`
   - `FileSize`
   - `IsApproved` (Boolean)

6. **VendorModeration Table**
   - `ModerationId` (PK)
   - `VendorId` (FK to Vendors)
   - `ModeratedBy` (FK to Users)
   - `ModerationType` (Approval, Rejection, Feature, Unfeature)
   - `ModeratedAt`
   - `Notes`



## UI/UX Requirements

1. **Vendor Listing**
   - Clean, responsive grid or list view
   - Clear categorization and filtering options
   - Visual indicators for ratings and vote counts
   - Intuitive sorting controls
   - Featured vendor highlighting

2. **Vendor Detail View**
   - Organized presentation of vendor information
   - Prominent display of community rating
   - Clear voting controls
   - Well-structured review section
   - Mobile-friendly layout

3. **Submission Forms**
   - Intuitive form layout with clear instructions
   - Progressive disclosure for optional fields
   - Real-time validation feedback
   - Mobile-friendly input controls
   - Clear submission confirmation

4. **Moderation Interface**
   - Efficient review queue
   - Clear approval/rejection controls
   - Inline editing capabilities
   - Batch operation support
   - Comprehensive audit trail

5. **Accessibility**
   - WCAG 2.1 Level AA compliance
   - Keyboard navigation support
   - Screen reader compatibility
   - Appropriate color contrast
   - Text resizing support

## Acceptance Criteria

1. All use cases can be successfully completed
2. Vendor suggestions can be submitted, viewed, and voted on effectively
3. Moderation workflow functions correctly
4. Voting system prevents manipulation and fraud
5. Reports generate accurate data in appropriate formats
6. Performance requirements are met
7. All API endpoints function correctly
8. Security requirements are met, including passing security scans
9. Accessibility requirements are met
10. The system encourages community participation and provides valuable information
