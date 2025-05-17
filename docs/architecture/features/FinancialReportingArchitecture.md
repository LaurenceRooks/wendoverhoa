# Financial Reporting Feature Architecture Guide

## Overview

This document provides architectural guidance for implementing the Financial Reporting feature in the Wendover HOA application, mapping components to the Clean Architecture layers, CQRS/MediatR patterns, and identifying cross-cutting concerns. The Financial Reporting feature provides a comprehensive system for creating, publishing, and archiving financial reports, ensuring transparency and proper financial oversight for the homeowners association.

## Domain Layer Components

### Entities
- **FinancialReport**: Core entity for financial reports
- **Budget**: Entity for annual budgets
- **BudgetCategory**: Entity for budget categories
- **ReserveFund**: Entity for reserve fund tracking
- **ReserveExpenditure**: Entity for reserve fund expenditures
- **FinancialChart**: Entity for financial visualizations
- **FinancialReportVersion**: Entity for report version history
- **FinancialReportTemplate**: Entity for report templates
- **FinancialReportSection**: Entity for report sections
- **FinancialReportApproval**: Entity for approval workflow

### Value Objects
- **Money**: Value object for monetary amounts
- **ReportPeriod**: Value object for report time periods
- **ReportStatus**: Enum (Draft, PendingApproval, Published, Archived)
- **ReportType**: Enum (Monthly, Quarterly, Annual, Budget, Audit)
- **BudgetAllocation**: Value object for budget allocations
- **ChartType**: Enum (Bar, Line, Pie, Area, etc.)
- **FinancialMetric**: Value object for financial metrics
- **VarianceExplanation**: Value object for budget variance explanations
- **ApprovalStatus**: Enum (Pending, Approved, Rejected)

### Domain Events
- **FinancialReportCreatedEvent**: Raised when report is created
- **FinancialReportPublishedEvent**: Raised when report is published
- **FinancialReportArchivedEvent**: Raised when report is archived
- **BudgetCreatedEvent**: Raised when budget is created
- **BudgetUpdatedEvent**: Raised when budget is updated
- **ReserveFundUpdatedEvent**: Raised when reserve fund is updated
- **FinancialChartCreatedEvent**: Raised when chart is created
- **FinancialReportApprovedEvent**: Raised when report is approved

### Domain Services
- **FinancialReportingService**: Core service for report operations
- **BudgetAnalysisService**: Analyzes budget vs. actual
- **ReserveFundCalculationService**: Calculates reserve fund metrics
- **FinancialChartGenerationService**: Generates financial charts
- **FinancialMetricsService**: Calculates financial metrics
- **FinancialReportVersioningService**: Manages report versions

### Domain Interfaces
- **IFinancialReportRepository**: Repository for FinancialReport entities
- **IBudgetRepository**: Repository for Budget entities
- **IReserveFundRepository**: Repository for ReserveFund entities
- **IFinancialChartRepository**: Repository for FinancialChart entities
- **IFinancialReportTemplateRepository**: Repository for FinancialReportTemplate entities
- **IFinancialReportVersionRepository**: Repository for FinancialReportVersion entities

## Application Layer Components

### Commands
- **CreateFinancialReportCommand**: Creates a new financial report
- **UpdateFinancialReportCommand**: Updates an existing report
- **PublishFinancialReportCommand**: Publishes a report
- **ArchiveFinancialReportCommand**: Archives a report
- **DeleteFinancialReportCommand**: Deletes a draft report
- **CreateBudgetCommand**: Creates a new budget
- **UpdateBudgetCommand**: Updates an existing budget
- **RecordReserveFundUpdateCommand**: Updates reserve fund
- **RecordReserveExpenditureCommand**: Records reserve expenditure
- **CreateFinancialChartCommand**: Creates a financial chart
- **UpdateFinancialChartCommand**: Updates a financial chart
- **ApproveFinancialReportCommand**: Approves a financial report
- **RejectFinancialReportCommand**: Rejects a financial report
- **ConfigureReportingSettingsCommand**: Configures reporting settings
- **ImportFinancialDataCommand**: Imports financial data

### Queries
- **GetFinancialReportQuery**: Gets a specific financial report
- **GetFinancialReportsQuery**: Gets a list of financial reports
- **GetFinancialReportVersionsQuery**: Gets versions of a report
- **GetBudgetQuery**: Gets a specific budget
- **GetBudgetsQuery**: Gets a list of budgets
- **GetBudgetVsActualQuery**: Gets budget vs. actual comparison
- **GetReserveFundQuery**: Gets reserve fund information
- **GetReserveExpendituresQuery**: Gets reserve expenditures
- **GetFinancialChartsQuery**: Gets financial charts
- **GetFinancialReportTemplatesQuery**: Gets report templates
- **GetFinancialMetricsQuery**: Gets financial metrics
- **GetArchivedReportsQuery**: Gets archived reports
- **GetFinancialDashboardQuery**: Gets dashboard data
- **GetFinancialReportingSettingsQuery**: Gets reporting settings

### DTOs
- **FinancialReportDto**: Data transfer object for FinancialReport
- **BudgetDto**: DTO for Budget
- **BudgetCategoryDto**: DTO for BudgetCategory
- **ReserveFundDto**: DTO for ReserveFund
- **ReserveExpenditureDto**: DTO for ReserveExpenditure
- **FinancialChartDto**: DTO for FinancialChart
- **FinancialReportVersionDto**: DTO for FinancialReportVersion
- **FinancialReportTemplateDto**: DTO for FinancialReportTemplate
- **FinancialReportSectionDto**: DTO for FinancialReportSection
- **BudgetVsActualDto**: DTO for budget vs. actual comparison
- **FinancialMetricsDto**: DTO for financial metrics
- **FinancialDashboardDto**: DTO for financial dashboard

### Validators
- **CreateFinancialReportCommandValidator**: Validates report creation
- **UpdateFinancialReportCommandValidator**: Validates report updates
- **PublishFinancialReportCommandValidator**: Validates report publication
- **CreateBudgetCommandValidator**: Validates budget creation
- **UpdateBudgetCommandValidator**: Validates budget updates
- **RecordReserveFundUpdateCommandValidator**: Validates reserve fund updates
- **CreateFinancialChartCommandValidator**: Validates chart creation

### Mapping Profiles
- **FinancialReportingMappingProfile**: Maps between entities and DTOs

## Infrastructure Layer Components

### Repositories
- **FinancialReportRepository**: Implements IFinancialReportRepository
- **BudgetRepository**: Implements IBudgetRepository
- **ReserveFundRepository**: Implements IReserveFundRepository
- **FinancialChartRepository**: Implements IFinancialChartRepository
- **FinancialReportTemplateRepository**: Implements IFinancialReportTemplateRepository
- **FinancialReportVersionRepository**: Implements IFinancialReportVersionRepository

### Persistence Configurations
- **FinancialReportConfiguration**: EF Core configuration for FinancialReport
- **BudgetConfiguration**: EF Core configuration for Budget
- **BudgetCategoryConfiguration**: EF Core configuration for BudgetCategory
- **ReserveFundConfiguration**: EF Core configuration for ReserveFund
- **ReserveExpenditureConfiguration**: EF Core configuration for ReserveExpenditure
- **FinancialChartConfiguration**: EF Core configuration for FinancialChart
- **FinancialReportVersionConfiguration**: EF Core configuration for FinancialReportVersion
- **FinancialReportTemplateConfiguration**: EF Core configuration for FinancialReportTemplate
- **FinancialReportSectionConfiguration**: EF Core configuration for FinancialReportSection

### External Services
- **ChartGenerationService**: Generates chart visualizations
- **PDFReportService**: Generates PDF reports
- **ExcelExportService**: Exports data to Excel
- **EmailNotificationService**: Sends report notifications
- **DocumentStorageService**: Stores report attachments
- **FinancialDataImportService**: Imports financial data from external sources
- **DuesService**: Integrates with Dues Tracking feature
- **PaymentService**: Integrates with Payment Processing feature
- **ExpenseService**: Integrates with Expense Tracking feature

## Presentation Layer Components

### API Controllers
- **FinancialReportController**: API endpoints for financial reports
- **BudgetController**: API endpoints for budgets
- **ReserveFundController**: API endpoints for reserve funds
- **FinancialChartController**: API endpoints for financial charts
- **FinancialReportTemplateController**: API endpoints for report templates
- **FinancialReportSettingsController**: API endpoints for reporting settings

### Blazor Components
- **FinancialReportListComponent**: Lists financial reports
- **FinancialReportDetailComponent**: Displays report details
- **FinancialReportEditorComponent**: Edits financial reports
- **BudgetEditorComponent**: Edits budgets
- **BudgetVsActualComponent**: Displays budget vs. actual
- **ReserveFundDashboardComponent**: Displays reserve fund information
- **FinancialChartEditorComponent**: Edits financial charts
- **FinancialDashboardComponent**: Displays financial dashboard
- **FinancialReportTemplateEditorComponent**: Edits report templates
- **FinancialReportSettingsComponent**: Manages reporting settings

### View Models
- **FinancialReportListViewModel**: View model for report list
- **FinancialReportDetailViewModel**: View model for report details
- **FinancialReportEditorViewModel**: View model for report editor
- **BudgetEditorViewModel**: View model for budget editor
- **BudgetVsActualViewModel**: View model for budget vs. actual
- **ReserveFundDashboardViewModel**: View model for reserve fund dashboard
- **FinancialChartEditorViewModel**: View model for chart editor
- **FinancialDashboardViewModel**: View model for financial dashboard
- **FinancialReportTemplateEditorViewModel**: View model for template editor
- **FinancialReportSettingsViewModel**: View model for reporting settings

## Cross-Cutting Concerns

### Logging
- Log report creation, publication, and archiving
- Log budget creation and updates
- Log reserve fund updates
- Log chart creation
- Log report approvals
- Log configuration changes
- Log data imports and exports

### Caching
- Cache published reports (medium duration)
- Cache financial charts (short duration)
- Cache financial metrics (short duration)
- Cache dashboard data (very short duration)
- Cache report templates (long duration)

### Exception Handling
- Handle financial calculation errors
- Handle report generation failures
- Handle chart generation errors
- Handle data import/export errors
- Handle validation errors
- Handle authorization errors

## Security Considerations

### Role-Based Access Control
- View public financial summary: Guests
- View published reports: Residents
- View committee-related reports: Committee Members
- View all reports including drafts: Board Members, Administrators
- Create and edit reports: Board Members, Administrators
- Publish reports: Board Members, Administrators
- Configure reporting settings: Administrators
- Delete reports: Administrators

### Financial Data Protection
- Field-level security for sensitive financial information
- Audit logging for all financial operations
- Version history for all financial reports
- Data validation to ensure financial integrity
- Secure handling of financial documents
- Prevention of unauthorized modifications

### API Security
- Implement rate limiting on API endpoints
- Use CSRF protection for form submissions
- Apply appropriate caching headers
- Validate all input parameters
- Implement proper authorization checks

## Implementation Checklist

- [ ] **Domain Layer**
  - [ ] Create entities (FinancialReport, Budget, etc.)
  - [ ] Define value objects (Money, ReportPeriod, etc.)
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
  - [ ] Implement chart generation service
  - [ ] Implement PDF report service
  - [ ] Implement Excel export service
  - [ ] Implement notification service
  - [ ] Implement document storage service
  - [ ] Implement integration services

- [ ] **Presentation Layer**
  - [ ] Create API controllers
  - [ ] Implement report list component
  - [ ] Implement report detail component
  - [ ] Implement report editor component
  - [ ] Implement budget editor component
  - [ ] Implement budget vs. actual component
  - [ ] Implement reserve fund dashboard component
  - [ ] Implement chart editor component
  - [ ] Implement financial dashboard component
  - [ ] Create view models

- [ ] **Cross-Cutting Concerns**
  - [ ] Configure logging
  - [ ] Implement caching
  - [ ] Set up exception handling

- [ ] **Security**
  - [ ] Configure role-based access control
  - [ ] Implement financial data protection
  - [ ] Configure API security
  - [ ] Implement audit logging

- [ ] **Testing**
  - [ ] Unit tests for domain entities and services
  - [ ] Unit tests for command and query handlers
  - [ ] Integration tests for repositories
  - [ ] Integration tests for API endpoints
  - [ ] Tests for financial calculations
  - [ ] Tests for report generation
