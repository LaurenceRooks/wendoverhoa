# Entity Relationship Diagrams

This document provides comprehensive Entity Relationship Diagrams (ERDs) for the Wendover HOA application, detailing the core domain entities, their relationships, cardinality, and key constraints. These diagrams are organized by feature area but also show cross-feature relationships to provide a complete picture of the database schema.

## Core Domain Entities

The Wendover HOA application is built using Clean Architecture principles, with domain entities forming the core of the system. The following diagram shows the primary domain entities and their relationships:

```
+-------------------+       +-------------------+       +-------------------+
|  ApplicationUser  |<----->|      Property     |<----->|     Resident      |
+-------------------+       +-------------------+       +-------------------+
        ^                          ^                           ^
        |                          |                           |
        v                          v                           v
+-------------------+       +-------------------+       +-------------------+
|  RefreshToken     |       |  DuesTransaction  |       |  PaymentMethod    |
+-------------------+       +-------------------+       +-------------------+
                                    ^
                                    |
                                    v
+-------------------+       +-------------------+       +-------------------+
|  NavigationItem   |       |  FinancialReport  |<----->|     Expense       |
+-------------------+       +-------------------+       +-------------------+
        ^                          ^                           ^
        |                          |                           |
        v                          v                           v
+-------------------+       +-------------------+       +-------------------+
|  UserPreference   |       |      Budget       |       |      Vendor       |
+-------------------+       +-------------------+       +-------------------+
```

### User and Authentication Entities

```
+-------------------+
|  ApplicationUser  |
+-------------------+
| PK: Id (int)      |
| FirstName         |
| LastName          |
| Email             |
| UserName          |
| PasswordHash      |
| LastPasswordChange|
| TwoFactorEnabled  |
| LockoutEnd        |
+-------------------+
        |
        | 1:N
        v
+-------------------+
|   RefreshToken    |
+-------------------+
| PK: Id (int)      |
| FK: UserId        |
| Token             |
| ExpiryDate        |
| IsRevoked         |
| CreatedAt         |
+-------------------+
        |
        | 1:N
        v
+-------------------+
|  UserPreference   |
+-------------------+
| PK: Id (int)      |
| FK: UserId        |
| PreferenceKey     |
| PreferenceValue   |
| CreatedAt         |
| UpdatedAt         |
+-------------------+
```

### Property and Resident Entities

```
+-------------------+       +-------------------+
|     Property      |<----->|     Resident      |
+-------------------+       +-------------------+
| PK: Id (int)      |       | PK: Id (int)      |
| Address           |       | FK: PropertyId    |
| City              |       | FK: UserId        |
| State             |       | FirstName         |
| ZipCode           |       | LastName          |
| LotNumber         |       | Email             |
| SquareFootage     |       | Phone             |
| YearBuilt         |       | IsOwner           |
| Status            |       | MoveInDate        |
| CreatedAt         |       | MoveOutDate       |
| UpdatedAt         |       | Status            |
+-------------------+       | CreatedAt         |
        |                   | UpdatedAt         |
        |                   +-------------------+
        | 1:N                       |
        v                           | 1:N
+-------------------+               v
|  PropertyHistory  |       +-------------------+
+-------------------+       |  ResidentHistory  |
| PK: Id (int)      |       +-------------------+
| FK: PropertyId    |       | PK: Id (int)      |
| ChangeType        |       | FK: ResidentId    |
| ChangeDate        |       | ChangeType        |
| ChangedBy         |       | ChangeDate        |
| OldValues (JSON)  |       | ChangedBy         |
| NewValues (JSON)  |       | OldValues (JSON)  |
+-------------------+       | NewValues (JSON)  |
                            +-------------------+
```

### Financial Entities

```
+-------------------+       +-------------------+       +-------------------+
| DuesConfiguration |<----->| DuesTransaction   |<----->| DuesStatement     |
+-------------------+       +-------------------+       +-------------------+
| PK: Id (int)      |       | PK: Id (int)      |       | PK: Id (int)      |
| RegularDuesAmount |       | FK: PropertyId    |       | FK: PropertyId    |
| DuesFrequency     |       | FK: ResidentId    |       | FK: ResidentId    |
| DueDate           |       | TransactionType   |       | StatementPeriodStart|
| GracePeriod       |       | Amount            |       | StatementPeriodEnd|
| LateFeeAmount     |       | Description       |       | TotalAmount       |
| LateFeeType       |       | DueDate           |       | DueDate           |
| EffectiveStartDate|       | TransactionDate   |       | GeneratedDate     |
| EffectiveEndDate  |       | PaymentMethod     |       | DeliveryMethod    |
| CreatedAt         |       | ReferenceNumber   |       | DeliveryStatus    |
| UpdatedAt         |       | Status            |       | StatementURL      |
| CreatedBy         |       | Notes             |       | CreatedAt         |
+-------------------+       | CreatedAt         |       | CreatedBy         |
        |                   | CreatedBy         |       +-------------------+
        | 1:N               +-------------------+               |
        v                           |                           | 1:N
+-------------------+               | 1:N                       v
| SpecialAssessment |               v                   +-------------------+
+-------------------+       +-------------------+       | PaymentTransaction|
| PK: Id (int)      |       |   PaymentPlan     |       +-------------------+
| Title             |       +-------------------+       | PK: Id (int)      |
| Description       |       | PK: Id (int)      |       | FK: ResidentId    |
| Amount            |       | FK: PropertyId    |       | FK: PropertyId    |
| AssessmentDate    |       | FK: ResidentId    |       | FK: PaymentMethodId|
| DueDate           |       | OriginalAmount    |       | TransactionType   |
| ApplicableProperties|     | NumberOfInstallments|     | Amount            |
| Status            |       | InstallmentAmount |       | ProcessingFee     |
| ApprovalDate      |       | StartDate         |       | TotalAmount       |
| ApprovedBy        |       | EndDate           |       | Description       |
| CreatedAt         |       | Status            |       | Status            |
| UpdatedAt         |       | CreatedAt         |       | GatewayTransactionId|
+-------------------+       | CreatedBy         |       | ReceiptNumber     |
                            | UpdatedAt         |       | Notes             |
                            | UpdatedBy         |       | IsReconciled      |
                            +-------------------+       | ReconciliationDate|
                                                        | CreatedAt         |
                                                        | UpdatedAt         |
                                                        +-------------------+
```

### Expense Tracking Entities

```
+-------------------+       +-------------------+       +-------------------+
| ExpenseCategory   |<----->|     Expense       |<----->| ExpenseDocument   |
+-------------------+       +-------------------+       +-------------------+
| PK: Id (int)      |       | PK: Id (int)      |       | PK: Id (int)      |
| Name              |       | FK: CategoryId    |       | FK: ExpenseId     |
| Description       |       | FK: RequestedBy   |       | DocumentType      |
| BudgetAmount      |       | FK: VendorId      |       | FileName          |
| FiscalYear        |       | Amount            |       | FileSize          |
| ParentCategoryId  |       | TaxAmount         |       | FileType          |
| ApprovalThreshold |       | TotalAmount       |       | StoragePath       |
| IsActive          |       | ExpenseDate       |       | UploadedBy        |
| CreatedAt         |       | Description       |       | UploadedAt        |
| UpdatedAt         |       | Purpose           |       +-------------------+
| CreatedBy         |       | PaymentMethod     |
+-------------------+       | Status            |       +-------------------+
        |                   | IsRecurring       |       | ExpenseApproval   |
        | 1:N               | RecurrencePattern |       +-------------------+
        v                   | Notes             |       | PK: Id (int)      |
+-------------------+       | IsReconciled      |       | FK: ExpenseId     |
|      Vendor       |       | ReconciliationDate|       | FK: ApproverId    |
+-------------------+       | CreatedAt         |       | ApprovalLevel     |
| PK: Id (int)      |       | UpdatedAt         |       | Status            |
| Name              |       +-------------------+       | Comments          |
| ContactPerson     |               |                   | ActionDate        |
| Email             |               | 1:N               | CreatedAt         |
| Phone             |               v                   | UpdatedAt         |
| Address           |       +-------------------+       +-------------------+
| TaxId             |       | ExpensePayment    |
| Category          |       +-------------------+
| PaymentTerms      |       | PK: Id (int)      |
| IsActive          |       | FK: ExpenseId     |
| CreatedAt         |       | Amount            |
| UpdatedAt         |       | PaymentDate       |
+-------------------+       | PaymentMethod     |
                            | ReferenceNumber   |
                            | ProcessedBy       |
                            | Status            |
                            | Notes             |
                            | CreatedAt         |
                            | UpdatedAt         |
                            +-------------------+
```

### Financial Reporting Entities

```
+-------------------+       +-------------------+       +-------------------+
| FinancialReport   |<----->| FinancialReportSection|<->| FinancialChart    |
+-------------------+       +-------------------+       +-------------------+
| PK: Id (int)      |       | PK: Id (int)      |       | PK: Id (int)      |
| ReportPeriodStart |       | FK: ReportId      |       | FK: ReportId      |
| ReportPeriodEnd   |       | Title             |       | FK: SectionId     |
| ReportType        |       | Content           |       | ChartType         |
| Status            |       | Order             |       | Title             |
| PublishedDate     |       | CreatedAt         |       | Description       |
| PublishedBy       |       | UpdatedAt         |       | DataSource        |
| Notes             |       +-------------------+       | Configuration     |
| CreatedAt         |                                   | CreatedAt         |
| CreatedBy         |                                   | UpdatedAt         |
| UpdatedAt         |                                   +-------------------+
+-------------------+
        |
        | 1:N
        v
+-------------------+       +-------------------+       +-------------------+
| FinancialReportVersion|   |      Budget       |<----->|  BudgetCategory   |
+-------------------+       +-------------------+       +-------------------+
| PK: Id (int)      |       | PK: Id (int)      |       | PK: Id (int)      |
| FK: ReportId      |       | Year              |       | FK: BudgetId      |
| VersionNumber     |       | Description       |       | Name              |
| ChangedBy         |       | Status            |       | Type              |
| ChangeDate        |       | ApprovedBy        |       | BudgetedAmount    |
| ChangeReason      |       | ApprovalDate      |       | ParentCategoryId  |
| PreviousContent   |       | CreatedAt         |       | Notes             |
| CurrentContent    |       | CreatedBy         |       | CreatedAt         |
+-------------------+       | UpdatedAt         |       | UpdatedAt         |
                            +-------------------+       +-------------------+
```

### Community Features Entities

```
+-------------------+       +-------------------+       +-------------------+
|   Announcement    |       | CommunityEvent    |       |     Document      |
+-------------------+       +-------------------+       +-------------------+
| PK: Id (int)      |       | PK: Id (int)      |       | PK: Id (int)      |
| Title             |       | Title             |       | Title             |
| Content           |       | Description       |       | Description       |
| ImportanceLevel   |       | StartDate         |       | CategoryId        |
| PublishDate       |       | EndDate           |       | FileName          |
| ExpiryDate        |       | Location          |       | FileSize          |
| CreatedBy         |       | IsRecurring       |       | FileType          |
| CreatedAt         |       | RecurrencePattern |       | StoragePath       |
| UpdatedAt         |       | CreatedBy         |       | Version           |
| IsPublished       |       | CreatedAt         |       | UploadedBy        |
+-------------------+       | UpdatedAt         |       | UploadedAt        |
                            | Status            |       | Status            |
                            +-------------------+       +-------------------+
                                                                |
                                                                | 1:N
                                                                v
                                                        +-------------------+
                                                        | DocumentCategory  |
                                                        +-------------------+
                                                        | PK: Id (int)      |
                                                        | Name              |
                                                        | Description       |
                                                        | ParentCategoryId  |
                                                        | CreatedAt         |
                                                        | UpdatedAt         |
                                                        +-------------------+
```

### Board Management Entities

```
+-------------------+       +-------------------+       +-------------------+
|   BoardMember     |<----->|    Committee      |<----->|  MeetingMinutes   |
+-------------------+       +-------------------+       +-------------------+
| PK: Id (int)      |       | PK: Id (int)      |       | PK: Id (int)      |
| FK: ResidentId    |       | Name              |       | FK: CommitteeId   |
| Position          |       | Description       |       | MeetingDate       |
| TermStart         |       | Type              |       | Location          |
| TermEnd           |       | CreatedAt         |       | Attendees         |
| Status            |       | UpdatedAt         |       | Minutes           |
| Bio               |       +-------------------+       | Status            |
| Photo             |               |                   | ApprovedBy        |
| CreatedAt         |               | N:M               | ApprovalDate      |
| UpdatedAt         |               v                   | CreatedBy         |
+-------------------+       +-------------------+       | CreatedAt         |
        |                   | CommitteeMember   |       | UpdatedAt         |
        | N:M               +-------------------+       +-------------------+
        v                   | PK: Id (int)      |               |
+-------------------+       | FK: CommitteeId   |               | 1:N
| BoardMemberRole   |       | FK: ResidentId    |               v
+-------------------+       | Role              |       +-------------------+
| PK: Id (int)      |       | JoinDate          |       |    ActionItem     |
| FK: BoardMemberId |       | LeaveDate         |       +-------------------+
| FK: RoleId        |       | Status            |       | PK: Id (int)      |
| AssignedDate      |       | CreatedAt         |       | FK: MinutesId     |
| RemovedDate       |       | UpdatedAt         |       | Description       |
| CreatedAt         |       +-------------------+       | AssignedTo        |
| UpdatedAt         |                                   | DueDate           |
+-------------------+                                   | Status            |
                                                        | CompletionDate    |
                                                        | Notes             |
                                                        | CreatedAt         |
                                                        | UpdatedAt         |
                                                        +-------------------+
                                                                |
                                                                | 1:N
                                                                v
                                                        +-------------------+
                                                        |     Motion        |
                                                        +-------------------+
                                                        | PK: Id (int)      |
                                                        | FK: MinutesId     |
                                                        | Title             |
                                                        | Description       |
                                                        | ProposedBy        |
                                                        | SecondedBy        |
                                                        | VotesFor          |
                                                        | VotesAgainst      |
                                                        | VotesAbstain      |
                                                        | Outcome           |
                                                        | Notes             |
                                                        | CreatedAt         |
                                                        | UpdatedAt         |
                                                        +-------------------+
```

### Meeting Minutes Specific Entities

```
+-------------------+       +-------------------+       +-------------------+
|  MeetingAgenda    |<----->|  MeetingMinutes   |<----->| MeetingAttendee   |
+-------------------+       +-------------------+       +-------------------+
| PK: Id (int)      |       | PK: Id (int)      |       | PK: Id (int)      |
| FK: CommitteeId   |       | FK: AgendaId      |       | FK: MinutesId     |
| MeetingDate       |       | FK: CommitteeId   |       | FK: ResidentId    |
| Title             |       | MeetingDate       |       | AttendeeType      |
| Description       |       | StartTime         |       | IsPresent         |
| Location          |       | EndTime           |       | JoinTime          |
| Status            |       | Location          |       | LeaveTime         |
| CreatedBy         |       | QuorumReached     |       | Notes             |
| CreatedAt         |       | Minutes           |       | CreatedAt         |
| UpdatedAt         |       | Status            |       | UpdatedAt         |
+-------------------+       | ApprovedBy        |       +-------------------+
        |                   | ApprovalDate      |
        | 1:N               | PublishedDate     |
        v                   | CreatedBy         |
+-------------------+       | CreatedAt         |
|   AgendaItem      |       | UpdatedAt         |
+-------------------+       +-------------------+
| PK: Id (int)      |               |
| FK: AgendaId      |               | 1:N
| Title             |               v
| Description       |       +-------------------+
| PresenterName     |       |  MeetingDocument  |
| Duration          |       +-------------------+
| Order             |       | PK: Id (int)      |
| Status            |       | FK: MinutesId     |
| Notes             |       | Title             |
| CreatedAt         |       | Description       |
| UpdatedAt         |       | FileName          |
+-------------------+       | FileSize          |
                            | FileType          |
                            | StoragePath       |
                            | UploadedBy        |
                            | UploadedAt        |
                            | CreatedAt         |
                            | UpdatedAt         |
                            +-------------------+
```

### Feedback and Suggestions Entities

```
+-------------------+       +-------------------+
|   UserFeedback    |       | VendorSuggestion  |
+-------------------+       +-------------------+
| PK: Id (int)      |       | PK: Id (int)      |
| FK: ResidentId    |       | FK: ResidentId    |
| Subject           |       | VendorName        |
| FeedbackType      |       | Category          |
| Content           |       | ContactInfo       |
| Status            |       | Description       |
| Response          |       | Status            |
| RespondedBy       |       | ReviewNotes       |
| RespondedAt       |       | ReviewedBy        |
| CreatedAt         |       | ReviewedAt        |
| UpdatedAt         |       | CreatedAt         |
+-------------------+       | UpdatedAt         |
                            +-------------------+
                                    |
                                    | 1:N
                                    v
                            +-------------------+
                            |  VendorVote       |
                            +-------------------+
                            | PK: Id (int)      |
                            | FK: SuggestionId  |
                            | FK: ResidentId    |
                            | VoteType          |
                            | Comment           |
                            | CreatedAt         |
                            +-------------------+
```

## Key Constraints and Indexes

### Primary Keys
- All entities have an integer primary key named `Id`
- Primary keys are auto-incrementing

### Foreign Keys
- Foreign keys follow the naming convention `{EntityName}Id`
- All foreign keys have appropriate cascading delete behavior configured
- Example: `FK_Expense_ExpenseCategory` enforces referential integrity between Expense and ExpenseCategory

### Indexes

#### Performance Indexes
- **ApplicationUser**: Index on `Email`, `UserName`
- **Property**: Index on `Address`, `LotNumber`
- **Resident**: Index on `LastName`, `FirstName`, `Email`
- **DuesTransaction**: Index on `PropertyId`, `TransactionDate`
- **Expense**: Index on `CategoryId`, `ExpenseDate`, `Status`
- **Document**: Index on `CategoryId`, `UploadedAt`
- **FinancialReport**: Index on `ReportPeriodStart`, `ReportPeriodEnd`, `Status`
- **MeetingMinutes**: Index on `CommitteeId`, `MeetingDate`, `Status`
- **MeetingAgenda**: Index on `CommitteeId`, `MeetingDate`
- **ActionItem**: Index on `MinutesId`, `DueDate`, `Status`
- **Motion**: Index on `MinutesId`, `Outcome`

#### Search Indexes
- **Announcement**: Index on `Title`, `PublishDate`
- **CommunityEvent**: Index on `Title`, `StartDate`, `EndDate`
- **Document**: Index on `Title`, `FileName`
- **UserFeedback**: Index on `Subject`, `FeedbackType`, `Status`
- **VendorSuggestion**: Index on `VendorName`, `Category`, `Status`
- **MeetingMinutes**: Index on `Minutes`, `Location`
- **MeetingAgenda**: Index on `Title`, `Description`
- **AgendaItem**: Index on `Title`, `Description`
- **Motion**: Index on `Title`, `Description`

#### Relationship Indexes
- All foreign key columns are indexed
- Composite indexes on frequently joined columns
- Example: `IX_Resident_PropertyId_UserId` for efficient joins

## Cardinality and Relationships

### One-to-One Relationships
- **ApplicationUser** to **BoardMember**: A user can be at most one board member
- **Resident** to **ApplicationUser**: A resident is associated with exactly one user account

### One-to-Many Relationships
- **Property** to **Resident**: A property can have multiple residents
- **ExpenseCategory** to **Expense**: An expense category can have multiple expenses
- **Committee** to **MeetingMinutes**: A committee can have multiple meeting minutes
- **MeetingMinutes** to **ActionItem**: Meeting minutes can have multiple action items
- **MeetingMinutes** to **Motion**: Meeting minutes can have multiple motions
- **MeetingAgenda** to **AgendaItem**: A meeting agenda can have multiple agenda items
- **MeetingMinutes** to **MeetingDocument**: Meeting minutes can have multiple documents
- **Resident** to **UserFeedback**: A resident can submit multiple feedback items
- **Resident** to **VendorSuggestion**: A resident can submit multiple vendor suggestions

### Many-to-Many Relationships
- **BoardMember** to **BoardMemberRole**: Board members can have multiple roles
- **Committee** to **CommitteeMember**: Committees can have multiple members, and residents can be on multiple committees
- **VendorSuggestion** to **VendorVote**: Residents can vote on multiple vendor suggestions

## Database Constraints

### Check Constraints
- **DuesTransaction.Amount**: Must be greater than zero
- **Expense.TotalAmount**: Must be greater than or equal to zero
- **PaymentTransaction.Amount**: Must be greater than zero
- **CommunityEvent.StartDate**: Must be before EndDate

### Unique Constraints
- **ApplicationUser.Email**: Must be unique
- **ApplicationUser.UserName**: Must be unique
- **Property.Address**: Must be unique
- **Committee.Name**: Must be unique within active committees

### Default Values
- **CreatedAt**: Default to current timestamp
- **Status** fields: Default to initial status (e.g., "Draft", "Pending", "Active")
- **IsActive** flags: Default to true

## Notes on Implementation

- All entities include audit fields (`CreatedAt`, `UpdatedAt`, `CreatedBy` where appropriate)
- Soft delete pattern is implemented for entities that should not be permanently removed
- Temporal tables are used for entities requiring historical tracking
- JSON columns are used for storing flexible, schema-less data (e.g., configuration settings)
- All monetary amounts are stored as decimal(18,2) to ensure precision
