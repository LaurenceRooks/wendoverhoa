# Dues Tracking

## Overview
This document outlines the requirements for the dues tracking feature of the Wendover HOA web application. This feature will provide tools for managing and tracking HOA dues and assessments, supporting both administrative functions and resident services for the Wendover Homeowners Association in Bedford, Texas.

## User Roles
1. **Guest** - No access to dues information
2. **Resident** - Can view their own dues history, current balance, and make payments
3. **Committee Member** - Same access as Resident
4. **Board Member** - Can view all resident dues information and generate reports
5. **Administrator** - Can manage dues configurations, payment schedules, and system settings

## Use Cases

### UC-DUES-01: View Dues Information
**Primary Actor:** Resident
**Description:** Allow residents to view their dues information
**Preconditions:** User is authenticated as a resident
**Postconditions:** Resident views their dues information

**Main Flow:**
1. Resident navigates to the dues section of their account
2. System displays:
   - Current dues balance
   - Payment history
   - Upcoming dues schedule
   - Payment options
   - Any late fees or special assessments
3. Resident can filter history by date range or payment type
4. Resident can download statements or receipts

**Alternative Flows:**
- Resident can set up email notifications for upcoming dues
- Resident can dispute a charge or request clarification

### UC-DUES-02: Configure Dues Structure
**Primary Actor:** Board Member or Administrator
**Description:** Allow authorized users to configure the dues structure
**Preconditions:** User is authenticated as Board Member or Administrator
**Postconditions:** Dues structure is configured

**Main Flow:**
1. Administrator navigates to dues configuration section
2. Administrator can:
   - Set regular dues amounts and frequency (monthly, quarterly, annual)
   - Configure late fee policies and grace periods
   - Set up special assessments
   - Configure payment options and processing fees
   - Define proration rules for new residents
3. System validates configuration changes
4. System applies new configuration according to effective dates

**Alternative Flows:**
- Import dues configuration from previous years
- Create different dues structures for different property types

### UC-DUES-03: Generate Dues Statements
**Primary Actor:** Board Member or Administrator
**Description:** Generate and distribute dues statements to residents
**Preconditions:** User is authenticated as Board Member or Administrator
**Postconditions:** Dues statements are generated and distributed

**Main Flow:**
1. Administrator navigates to statement generation section
2. Administrator selects:
   - Statement period
   - Resident group (all, specific properties, or individual)
   - Statement format and delivery method
3. System generates statements based on dues configuration and resident payment history
4. System distributes statements via selected method (email, portal notification, etc.)
5. System records statement generation in the audit log

**Alternative Flows:**
- Schedule automatic statement generation
- Generate statements for specific special assessments
- Regenerate statements for specific residents

### UC-DUES-04: Record Manual Payments
**Primary Actor:** Board Member or Administrator
**Description:** Record payments made outside the online system
**Preconditions:** User is authenticated as Board Member or Administrator
**Postconditions:** Manual payment is recorded

**Main Flow:**
1. Administrator navigates to payment recording section
2. Administrator enters:
   - Resident information
   - Payment amount
   - Payment date
   - Payment method
   - Reference number
   - Notes
3. System validates payment information
4. System records payment and updates resident's dues balance
5. System generates receipt and notification to resident

**Alternative Flows:**
- Batch import payments from external sources
- Void or adjust incorrectly recorded payments

### UC-DUES-05: Manage Delinquent Accounts
**Primary Actor:** Board Member or Administrator
**Description:** Identify and manage delinquent accounts
**Preconditions:** User is authenticated as Board Member or Administrator
**Postconditions:** Delinquent accounts are managed

**Main Flow:**
1. Administrator navigates to delinquent accounts section
2. System displays accounts with overdue balances, sorted by age and amount
3. Administrator can:
   - Send payment reminders
   - Apply late fees
   - Create payment plans
   - Flag accounts for legal action
   - Record collection activities
4. System tracks all collection activities and communications
5. System updates account status based on administrator actions

**Alternative Flows:**
- Generate delinquency reports for board review
- Batch process delinquent accounts
- Configure automated reminder schedules

### UC-DUES-06: Generate Dues Reports
**Primary Actor:** Board Member or Administrator
**Description:** Generate reports on dues collection and status
**Preconditions:** User is authenticated as Board Member or Administrator
**Postconditions:** Dues reports are generated

**Main Flow:**
1. Administrator navigates to dues reporting section
2. Administrator selects:
   - Report type (collection summary, delinquency, forecasting, etc.)
   - Date range
   - Grouping and filtering options
3. System generates the requested report
4. Administrator can view, print, or export the report

**Alternative Flows:**
- Schedule recurring reports
- Save report templates for future use
- Generate custom reports with specific parameters

### UC-DUES-07: Delete Dues Record
**Primary Actor:** Board Member or Administrator
**Description:** Allow authorized users to delete dues records
**Preconditions:** User is authenticated as Board Member or Administrator; dues record exists
**Postconditions:** Dues record is deleted

**Main Flow:**
1. User navigates to dues management page
2. User selects a dues record to delete
3. System prompts for confirmation with warning about financial implications
4. User confirms deletion
5. System deletes the dues record
6. System confirms successful deletion
7. System logs the deletion action with timestamp and user ID

**Alternative Flows:**
- If user cancels deletion, return to previous state
- Option to archive instead of delete for historical reference
- Only Administrator can permanently delete dues records
- System prevents deletion of dues records with associated payments

### UC-DUES-08: Archive Dues Record
**Primary Actor:** Board Member or Administrator
**Description:** Allow authorized users to archive dues records for historical reference
**Preconditions:** User is authenticated as Board Member or Administrator; dues record exists
**Postconditions:** Dues record is archived and no longer appears in active listings

**Main Flow:**
1. User navigates to dues management page
2. User selects a dues record to archive
3. System prompts for confirmation
4. User confirms archiving
5. System moves the dues record to the archive
6. System confirms successful archiving
7. System logs the archiving action with timestamp and user ID

**Alternative Flows:**
- If user cancels archiving, return to previous state
- Option to add archive notes or reason for archiving
- Administrators can access archived dues records through the Administrative Controls interface
- Option to include or exclude archived records in financial reports

## Technical Requirements

1. **Implementation**
   - Follow Clean Architecture principles
   - Follow SOLID, DRY, KISS, and YAGNI principles
   - Use CQRS pattern with MediatR for dues operations
   - Create comprehensive unit and integration tests for all functionality
   - Pass security scanning using GitHub's free tools (CodeQL, dependency scanning, secret scanning)

2. **Data Storage**
   - Store dues data in Microsoft SQL Server 2022
   - Implement proper data relationships between properties, residents, and dues
   - Maintain historical records of all dues transactions
   - Implement audit logging for all dues management operations

3. **Integration**
   - Integrate with Directory feature for resident and property information
   - Integrate with Payment Processing for online payments
   - Integrate with Financial Reporting for financial analysis
   - Integrate with Authentication for role-based access control

4. **Security**
   - Implement role-based access control for dues management features
   - Ensure secure handling of financial information
   - Provide audit trails for all dues-related activities
   - Comply with financial data security standards

## UI/UX Requirements

1. **Resident Dues Interface**
   - Clean, intuitive display of dues information
   - Clear visualization of payment history and upcoming dues
   - Mobile-responsive design for all dues interfaces
   - Accessible design following WCAG 2.1 AA standards

2. **Administrator Interface**
   - Comprehensive dashboard for dues management
   - Intuitive tools for configuring dues structures
   - Clear visual indicators for delinquent accounts
   - Efficient interfaces for batch operations

3. **Reporting Interface**
   - Interactive charts and graphs for dues data visualization
   - Customizable report parameters
   - Export options in multiple formats (PDF, Excel, CSV)
   - Saved report templates for common reports

## API Endpoints

1. **Dues Configuration API**
   - `GET /api/dues/configuration` - Get current dues configuration
   - `PUT /api/dues/configuration` - Update dues configuration
   - `GET /api/dues/configuration/history` - Get configuration history

2. **Resident Dues API**
   - `GET /api/dues/residents/{id}` - Get dues for specific resident
   - `GET /api/dues/properties/{id}` - Get dues for specific property
   - `POST /api/dues/manual-payment` - Record manual payment
   - `GET /api/dues/statements/{id}` - Get specific statement

3. **Dues Management API**
   - `GET /api/dues/delinquent` - Get delinquent accounts
   - `POST /api/dues/generate-statements` - Generate statements
   - `POST /api/dues/apply-late-fees` - Apply late fees
   - `GET /api/dues/reports/{type}` - Generate specified report

## Database Schema

1. **DuesConfiguration Table**
   - ConfigurationId (PK)
   - RegularDuesAmount
   - DuesFrequency (Monthly, Quarterly, Annual)
   - DueDate
   - GracePeriod
   - LateFeeAmount
   - LateFeeType (Fixed, Percentage)
   - EffectiveStartDate
   - EffectiveEndDate
   - CreatedAt
   - UpdatedAt
   - CreatedBy

2. **DuesTransactions Table**
   - TransactionId (PK)
   - PropertyId (FK to Properties)
   - ResidentId (FK to Residents)
   - TransactionType (Regular Dues, Special Assessment, Late Fee, Payment)
   - Amount
   - Description
   - DueDate
   - TransactionDate
   - PaymentMethod
   - ReferenceNumber
   - Status
   - Notes
   - CreatedAt
   - CreatedBy

3. **DuesStatements Table**
   - StatementId (PK)
   - PropertyId (FK to Properties)
   - ResidentId (FK to Residents)
   - StatementPeriodStart
   - StatementPeriodEnd
   - TotalAmount
   - DueDate
   - GeneratedDate
   - DeliveryMethod
   - DeliveryStatus
   - StatementURL
   - CreatedAt
   - CreatedBy

4. **SpecialAssessments Table**
   - AssessmentId (PK)
   - Title
   - Description
   - Amount
   - AssessmentDate
   - DueDate
   - ApplicableProperties (All, Specific)
   - Status
   - ApprovalDate
   - ApprovedBy
   - CreatedAt
   - UpdatedAt

5. **PaymentPlans Table**
   - PaymentPlanId (PK)
   - PropertyId (FK to Properties)
   - ResidentId (FK to Residents)
   - OriginalAmount
   - NumberOfInstallments
   - InstallmentAmount
   - StartDate
   - EndDate
   - Status
   - CreatedAt
   - CreatedBy
   - UpdatedAt
   - UpdatedBy

## Acceptance Criteria

1. Residents can view their current dues balance, payment history, and upcoming dues
2. Administrators can configure dues amounts, frequencies, and late fee policies
3. The system can generate and distribute dues statements to residents
4. Administrators can record manual payments and update resident balances
5. The system identifies delinquent accounts and supports collection activities
6. Comprehensive reports on dues collection and status can be generated
7. All dues transactions are properly logged and audited
8. Integration with other system features works seamlessly
9. All dues management interfaces are responsive and accessible
10. Dues management API endpoints function correctly with proper authorization
