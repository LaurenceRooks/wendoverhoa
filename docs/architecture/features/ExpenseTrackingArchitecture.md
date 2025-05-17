# Expense Tracking Feature Architecture Guide

## Overview

This document provides architectural guidance for implementing the Expense Tracking feature in the Wendover HOA application, mapping components to the Clean Architecture layers, CQRS/MediatR patterns, and identifying cross-cutting concerns. The Expense Tracking feature provides tools for managing, tracking, and reporting HOA expenses, supporting financial transparency and accountability.

## Domain Layer Components

### Entities
- **Expense**: Core entity for expense records
- **ExpenseCategory**: Entity for expense categories
- **ExpenseDocument**: Entity for expense receipts and invoices
- **ExpenseApproval**: Entity for approval workflow
- **ExpensePayment**: Entity for expense payments
- **Vendor**: Entity for expense vendors
- **RecurringExpense**: Entity for recurring expenses
- **ExpenseReconciliation**: Entity for reconciliation records

### Value Objects
- **Money**: Value object for monetary amounts
- **ExpenseStatus**: Enum (Draft, Pending, Approved, Rejected, Paid)
- **ApprovalStatus**: Enum (Pending, Approved, Rejected, InfoRequested)
- **DocumentType**: Enum (Receipt, Invoice, Contract, Other)
- **PaymentMethod**: Enum (Check, CreditCard, BankTransfer, Cash, Other)
- **RecurrencePattern**: Value object for expense recurrence
- **ApprovalLevel**: Value object for approval hierarchy
- **ReconciliationStatus**: Enum (Pending, Reconciled, Discrepancy)

### Domain Events
- **ExpenseCreatedEvent**: Raised when expense is created
- **ExpenseSubmittedEvent**: Raised when expense is submitted for approval
- **ExpenseApprovedEvent**: Raised when expense is approved
- **ExpenseRejectedEvent**: Raised when expense is rejected
- **ExpensePaidEvent**: Raised when expense is paid
- **ExpenseCategoryCreatedEvent**: Raised when category is created
- **ExpenseDocumentUploadedEvent**: Raised when document is uploaded
- **RecurringExpenseCreatedEvent**: Raised when recurring expense is created
- **ExpenseReconciledEvent**: Raised when expense is reconciled

### Domain Services
- **ExpenseApprovalService**: Manages approval workflows
- **ExpensePaymentService**: Manages expense payments
- **RecurringExpenseService**: Manages recurring expenses
- **ExpenseReconciliationService**: Reconciles expenses
- **ExpenseBudgetService**: Tracks expenses against budget
- **ExpenseReportingService**: Generates expense reports

### Domain Interfaces
- **IExpenseRepository**: Repository for Expense entities
- **IExpenseCategoryRepository**: Repository for ExpenseCategory entities
- **IExpenseDocumentRepository**: Repository for ExpenseDocument entities
- **IExpenseApprovalRepository**: Repository for ExpenseApproval entities
- **IExpensePaymentRepository**: Repository for ExpensePayment entities
- **IVendorRepository**: Repository for Vendor entities
- **IRecurringExpenseRepository**: Repository for RecurringExpense entities

## Application Layer Components

### Commands
- **CreateExpenseCommand**: Creates a new expense
- **UpdateExpenseCommand**: Updates an existing expense
- **SubmitExpenseCommand**: Submits expense for approval
- **ApproveExpenseCommand**: Approves an expense
- **RejectExpenseCommand**: Rejects an expense
- **RequestInfoExpenseCommand**: Requests more information
- **PayExpenseCommand**: Records payment for expense
- **CreateExpenseCategoryCommand**: Creates a new category
- **UpdateExpenseCategoryCommand**: Updates a category
- **UploadExpenseDocumentCommand**: Uploads a document
- **CreateRecurringExpenseCommand**: Creates a recurring expense
- **UpdateRecurringExpenseCommand**: Updates a recurring expense
- **CancelRecurringExpenseCommand**: Cancels a recurring expense
- **ReconcileExpenseCommand**: Reconciles an expense
- **DeleteExpenseCommand**: Deletes a draft expense
- **ArchiveExpenseCommand**: Archives an expense
- **CreateVendorCommand**: Creates a new vendor
- **UpdateVendorCommand**: Updates a vendor
- **BatchPayExpensesCommand**: Processes batch payment

### Queries
- **GetExpenseQuery**: Gets a specific expense
- **GetExpensesQuery**: Gets a list of expenses
- **GetExpenseDashboardQuery**: Gets dashboard data
- **GetPendingApprovalsQuery**: Gets expenses pending approval
- **GetExpenseCategoriesQuery**: Gets expense categories
- **GetExpenseDocumentsQuery**: Gets expense documents
- **GetExpenseApprovalHistoryQuery**: Gets approval history
- **GetExpensePaymentsQuery**: Gets expense payments
- **GetVendorsQuery**: Gets vendors
- **GetRecurringExpensesQuery**: Gets recurring expenses
- **GetExpenseReportQuery**: Generates an expense report
- **GetExpenseReconciliationQuery**: Gets reconciliation data
- **GetArchivedExpensesQuery**: Gets archived expenses
- **GetExpenseBudgetComparisonQuery**: Gets budget comparison

### DTOs
- **ExpenseDto**: Data transfer object for Expense
- **ExpenseCategoryDto**: DTO for ExpenseCategory
- **ExpenseDocumentDto**: DTO for ExpenseDocument
- **ExpenseApprovalDto**: DTO for ExpenseApproval
- **ExpensePaymentDto**: DTO for ExpensePayment
- **VendorDto**: DTO for Vendor
- **RecurringExpenseDto**: DTO for RecurringExpense
- **ExpenseDashboardDto**: DTO for expense dashboard
- **ExpenseReportDto**: DTO for expense reports
- **ExpenseReconciliationDto**: DTO for reconciliation
- **ExpenseBudgetComparisonDto**: DTO for budget comparison

### Validators
- **CreateExpenseCommandValidator**: Validates expense creation
- **UpdateExpenseCommandValidator**: Validates expense updates
- **SubmitExpenseCommandValidator**: Validates expense submission
- **ApproveExpenseCommandValidator**: Validates expense approval
- **PayExpenseCommandValidator**: Validates expense payment
- **CreateExpenseCategoryCommandValidator**: Validates category creation
- **CreateRecurringExpenseCommandValidator**: Validates recurring expense creation
- **ReconcileExpenseCommandValidator**: Validates expense reconciliation

### Mapping Profiles
- **ExpenseTrackingMappingProfile**: Maps between entities and DTOs

## Infrastructure Layer Components

### Repositories
- **ExpenseRepository**: Implements IExpenseRepository
- **ExpenseCategoryRepository**: Implements IExpenseCategoryRepository
- **ExpenseDocumentRepository**: Implements IExpenseDocumentRepository
- **ExpenseApprovalRepository**: Implements IExpenseApprovalRepository
- **ExpensePaymentRepository**: Implements IExpensePaymentRepository
- **VendorRepository**: Implements IVendorRepository
- **RecurringExpenseRepository**: Implements IRecurringExpenseRepository

### Persistence Configurations
- **ExpenseConfiguration**: EF Core configuration for Expense
- **ExpenseCategoryConfiguration**: EF Core configuration for ExpenseCategory
- **ExpenseDocumentConfiguration**: EF Core configuration for ExpenseDocument
- **ExpenseApprovalConfiguration**: EF Core configuration for ExpenseApproval
- **ExpensePaymentConfiguration**: EF Core configuration for ExpensePayment
- **VendorConfiguration**: EF Core configuration for Vendor
- **RecurringExpenseConfiguration**: EF Core configuration for RecurringExpense

### External Services
- **DocumentStorageService**: Stores expense documents
- **PDFReportService**: Generates PDF reports
- **ExcelExportService**: Exports data to Excel
- **EmailNotificationService**: Sends expense notifications
- **FinancialReportingService**: Integrates with Financial Reporting feature
- **PaymentProcessingService**: Integrates with Payment Processing feature
- **BudgetService**: Integrates with budget management
- **RecurrenceSchedulerService**: Schedules recurring expenses

## Presentation Layer Components

### API Controllers
- **ExpenseController**: API endpoints for expenses
- **ExpenseCategoryController**: API endpoints for categories
- **ExpenseApprovalController**: API endpoints for approvals
- **ExpensePaymentController**: API endpoints for payments
- **VendorController**: API endpoints for vendors
- **RecurringExpenseController**: API endpoints for recurring expenses
- **ExpenseReportController**: API endpoints for reports
- **ExpenseReconciliationController**: API endpoints for reconciliation

### Blazor Components
- **ExpenseDashboardComponent**: Displays expense dashboard
- **ExpenseFormComponent**: Form for creating/editing expenses
- **ExpenseListComponent**: Lists expenses
- **ExpenseDetailComponent**: Displays expense details
- **ExpenseApprovalComponent**: Manages expense approvals
- **ExpensePaymentComponent**: Manages expense payments
- **ExpenseCategoryComponent**: Manages expense categories
- **VendorManagementComponent**: Manages vendors
- **RecurringExpenseComponent**: Manages recurring expenses
- **ExpenseReportComponent**: Generates expense reports
- **ExpenseReconciliationComponent**: Reconciles expenses
- **ExpenseBudgetComponent**: Tracks expenses against budget

### View Models
- **ExpenseDashboardViewModel**: View model for dashboard
- **ExpenseFormViewModel**: View model for expense form
- **ExpenseListViewModel**: View model for expense list
- **ExpenseDetailViewModel**: View model for expense details
- **ExpenseApprovalViewModel**: View model for approvals
- **ExpensePaymentViewModel**: View model for payments
- **ExpenseCategoryViewModel**: View model for categories
- **VendorManagementViewModel**: View model for vendors
- **RecurringExpenseViewModel**: View model for recurring expenses
- **ExpenseReportViewModel**: View model for reports
- **ExpenseReconciliationViewModel**: View model for reconciliation
- **ExpenseBudgetViewModel**: View model for budget tracking

## Cross-Cutting Concerns

### Logging
- Log expense creation and updates
- Log approval decisions
- Log payment processing
- Log document uploads
- Log category changes
- Log reconciliation activities
- Log report generation
- Log security-related events

### Caching
- Cache expense categories (medium duration)
- Cache vendors (medium duration)
- Cache dashboard data (short duration)
- Cache report data (short duration)
- Cache budget data (short duration)

### Exception Handling
- Handle document upload errors
- Handle payment processing errors
- Handle report generation failures
- Handle validation errors
- Handle authorization errors
- Handle integration errors with other features

## Security Considerations

### Role-Based Access Control
- View expense dashboard: Board Members, Administrators
- Submit expense requests: Board Members, Committee Members (for their committee)
- Approve expenses: Board Members, Administrators
- Process payments: Board Members, Administrators
- Manage categories: Board Members, Administrators
- Generate reports: Board Members, Administrators
- Reconcile expenses: Board Members, Administrators
- Manage vendors: Administrators

### Financial Data Protection
- Secure storage of expense documents
- Audit logging for all expense operations
- Version history for expense changes
- Data validation to ensure financial integrity
- Secure handling of payment information
- Prevention of unauthorized modifications

### Approval Workflows
- Multi-level approval based on expense amount
- Segregation of duties (requestor cannot approve)
- Delegation of approval authority
- Approval audit trail
- Notification system for pending approvals

### API Security
- Implement rate limiting on API endpoints
- Use CSRF protection for form submissions
- Apply appropriate caching headers
- Validate all input parameters
- Implement proper authorization checks

## Implementation Checklist

- [ ] **Domain Layer**
  - [ ] Create entities (Expense, ExpenseCategory, etc.)
  - [ ] Define value objects (Money, ExpenseStatus, etc.)
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
  - [ ] Implement document storage service
  - [ ] Implement report generation service
  - [ ] Implement notification service
  - [ ] Implement integration services
  - [ ] Implement recurrence scheduler service

- [ ] **Presentation Layer**
  - [ ] Create API controllers
  - [ ] Implement dashboard component
  - [ ] Implement expense form component
  - [ ] Implement expense list component
  - [ ] Implement expense detail component
  - [ ] Implement approval component
  - [ ] Implement payment component
  - [ ] Implement category management component
  - [ ] Implement vendor management component
  - [ ] Implement reporting component
  - [ ] Create view models

- [ ] **Cross-Cutting Concerns**
  - [ ] Configure logging
  - [ ] Implement caching
  - [ ] Set up exception handling

- [ ] **Security**
  - [ ] Configure role-based access control
  - [ ] Implement approval workflows
  - [ ] Implement financial data protection
  - [ ] Configure API security
  - [ ] Implement audit logging

- [ ] **Testing**
  - [ ] Unit tests for domain entities and services
  - [ ] Unit tests for command and query handlers
  - [ ] Integration tests for repositories
  - [ ] Integration tests for API endpoints
  - [ ] Tests for approval workflows
  - [ ] Tests for recurring expense generation
