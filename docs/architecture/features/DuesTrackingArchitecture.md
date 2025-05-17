# Dues Tracking Feature Architecture Guide

## Overview

This document provides architectural guidance for implementing the Dues Tracking feature in the Wendover HOA application, mapping components to the Clean Architecture layers, CQRS/MediatR patterns, and identifying cross-cutting concerns. The Dues Tracking feature provides tools for managing and tracking HOA dues and assessments, supporting both administrative functions and resident services.

## Domain Layer Components

### Entities
- **DuesConfiguration**: Entity for dues amounts, frequencies, and policies
- **DuesTransaction**: Core entity for dues charges and payments
- **DuesStatement**: Entity for resident statements
- **SpecialAssessment**: Entity for special one-time assessments
- **PaymentPlan**: Entity for payment plans for delinquent accounts
- **DuesAccount**: Entity representing a property's dues account
- **LateFee**: Entity for late fee charges

### Value Objects
- **Money**: Value object for monetary amounts
- **DuesFrequency**: Enum (Monthly, Quarterly, Annual)
- **TransactionType**: Enum (Regular Dues, Special Assessment, Late Fee, Payment)
- **PaymentMethod**: Enum (Online, Check, Cash, BankTransfer, Other)
- **StatementPeriod**: Value object with start and end dates
- **DuesStatus**: Enum (Current, Overdue, Delinquent, InCollection)
- **LateFeeType**: Enum (Fixed, Percentage)

### Domain Events
- **DuesConfigurationChangedEvent**: Raised when dues configuration changes
- **DuesTransactionCreatedEvent**: Raised when transaction is created
- **PaymentRecordedEvent**: Raised when payment is recorded
- **StatementGeneratedEvent**: Raised when statement is generated
- **AccountDelinquentEvent**: Raised when account becomes delinquent
- **LateFeeAppliedEvent**: Raised when late fee is applied
- **PaymentPlanCreatedEvent**: Raised when payment plan is created
- **SpecialAssessmentCreatedEvent**: Raised when special assessment is created

### Domain Services
- **DuesCalculationService**: Calculates dues amounts and balances
- **LateFeeService**: Determines and applies late fees
- **PaymentPlanService**: Manages payment plan calculations
- **DuesReportingService**: Generates dues reports
- **DelinquencyService**: Manages delinquent account rules

### Domain Interfaces
- **IDuesConfigurationRepository**: Repository for DuesConfiguration entities
- **IDuesTransactionRepository**: Repository for DuesTransaction entities
- **IDuesStatementRepository**: Repository for DuesStatement entities
- **ISpecialAssessmentRepository**: Repository for SpecialAssessment entities
- **IPaymentPlanRepository**: Repository for PaymentPlan entities
- **IDuesAccountRepository**: Repository for DuesAccount entities

## Application Layer Components

### Commands
- **UpdateDuesConfigurationCommand**: Updates dues configuration
- **CreateDuesTransactionCommand**: Creates a dues transaction
- **RecordPaymentCommand**: Records a payment
- **GenerateStatementsCommand**: Generates dues statements
- **CreateSpecialAssessmentCommand**: Creates a special assessment
- **ApplyLateFeesCommand**: Applies late fees to overdue accounts
- **CreatePaymentPlanCommand**: Creates a payment plan
- **UpdatePaymentPlanCommand**: Updates a payment plan
- **DeleteDuesTransactionCommand**: Deletes a dues transaction
- **SendPaymentReminderCommand**: Sends payment reminder
- **FlagAccountForCollectionCommand**: Flags account for collection
- **RecordCollectionActivityCommand**: Records collection activity

### Queries
- **GetDuesConfigurationQuery**: Gets current dues configuration
- **GetDuesConfigurationHistoryQuery**: Gets configuration history
- **GetResidentDuesQuery**: Gets dues for a resident
- **GetPropertyDuesQuery**: Gets dues for a property
- **GetDuesTransactionsQuery**: Gets dues transactions
- **GetDuesStatementQuery**: Gets a specific statement
- **GetDuesStatementsQuery**: Gets dues statements
- **GetDelinquentAccountsQuery**: Gets delinquent accounts
- **GetSpecialAssessmentsQuery**: Gets special assessments
- **GetPaymentPlansQuery**: Gets payment plans
- **GenerateDuesCollectionReportQuery**: Generates collection report
- **GenerateDuesDelinquencyReportQuery**: Generates delinquency report
- **GenerateDuesForecastReportQuery**: Generates forecast report

### DTOs
- **DuesConfigurationDto**: Data transfer object for DuesConfiguration
- **DuesTransactionDto**: DTO for DuesTransaction
- **DuesStatementDto**: DTO for DuesStatement
- **SpecialAssessmentDto**: DTO for SpecialAssessment
- **PaymentPlanDto**: DTO for PaymentPlan
- **DuesAccountDto**: DTO for DuesAccount
- **DelinquentAccountDto**: DTO for delinquent accounts
- **DuesReportDto**: DTO for dues reports
- **DuesBalanceDto**: DTO for dues balance information

### Validators
- **UpdateDuesConfigurationCommandValidator**: Validates configuration updates
- **CreateDuesTransactionCommandValidator**: Validates transaction creation
- **RecordPaymentCommandValidator**: Validates payment recording
- **GenerateStatementsCommandValidator**: Validates statement generation
- **CreateSpecialAssessmentCommandValidator**: Validates assessment creation
- **CreatePaymentPlanCommandValidator**: Validates payment plan creation

### Mapping Profiles
- **DuesTrackingMappingProfile**: Maps between entities and DTOs

## Infrastructure Layer Components

### Repositories
- **DuesConfigurationRepository**: Implements IDuesConfigurationRepository
- **DuesTransactionRepository**: Implements IDuesTransactionRepository
- **DuesStatementRepository**: Implements IDuesStatementRepository
- **SpecialAssessmentRepository**: Implements ISpecialAssessmentRepository
- **PaymentPlanRepository**: Implements IPaymentPlanRepository
- **DuesAccountRepository**: Implements IDuesAccountRepository

### Persistence Configurations
- **DuesConfigurationConfiguration**: EF Core configuration for DuesConfiguration
- **DuesTransactionConfiguration**: EF Core configuration for DuesTransaction
- **DuesStatementConfiguration**: EF Core configuration for DuesStatement
- **SpecialAssessmentConfiguration**: EF Core configuration for SpecialAssessment
- **PaymentPlanConfiguration**: EF Core configuration for PaymentPlan
- **DuesAccountConfiguration**: EF Core configuration for DuesAccount

### External Services
- **StatementGenerationService**: Generates PDF statements
- **EmailNotificationService**: Sends email notifications
- **PaymentProcessingService**: Processes online payments
- **ReportGenerationService**: Generates financial reports
- **PropertyService**: Integrates with Directory feature
- **ResidentService**: Integrates with Directory feature

## Presentation Layer Components

### API Controllers
- **DuesConfigurationController**: API endpoints for dues configuration
- **ResidentDuesController**: API endpoints for resident dues
- **DuesTransactionController**: API endpoints for dues transactions
- **DuesStatementController**: API endpoints for dues statements
- **SpecialAssessmentController**: API endpoints for special assessments
- **PaymentPlanController**: API endpoints for payment plans
- **DuesReportController**: API endpoints for dues reports

### Blazor Components
- **ResidentDuesView**: Displays resident dues information
- **DuesPaymentHistory**: Displays payment history
- **DuesStatementView**: Displays dues statements
- **DuesPaymentForm**: Form for making payments
- **DuesConfigurationManager**: Interface for managing dues configuration
- **SpecialAssessmentManager**: Interface for managing special assessments
- **DelinquentAccountManager**: Interface for managing delinquent accounts
- **PaymentPlanManager**: Interface for managing payment plans
- **DuesReportGenerator**: Interface for generating dues reports
- **DuesDashboard**: Dashboard for dues overview

### View Models
- **ResidentDuesViewModel**: View model for resident dues
- **DuesPaymentHistoryViewModel**: View model for payment history
- **DuesStatementViewModel**: View model for dues statements
- **DuesPaymentViewModel**: View model for payment form
- **DuesConfigurationViewModel**: View model for dues configuration
- **SpecialAssessmentViewModel**: View model for special assessments
- **DelinquentAccountViewModel**: View model for delinquent accounts
- **PaymentPlanViewModel**: View model for payment plans
- **DuesReportViewModel**: View model for dues reports
- **DuesDashboardViewModel**: View model for dues dashboard

## Cross-Cutting Concerns

### Logging
- Log dues configuration changes
- Log transaction creation and updates
- Log payment recording
- Log statement generation
- Log special assessment creation
- Log late fee application
- Log payment plan creation and updates
- Log collection activities

### Caching
- Cache dues configuration (medium duration)
- Cache resident dues summary (short duration)
- Cache statement information (medium duration)
- Cache report data (short duration)
- Cache delinquent account list (very short duration)

### Exception Handling
- Handle payment processing errors
- Handle statement generation failures
- Handle invalid transaction data
- Handle configuration conflicts
- Handle integration errors with other features

## Security Considerations

### Role-Based Access Control
- View own dues information: Residents
- View all dues information: Board Members, Administrators
- Configure dues structure: Administrators
- Generate statements: Board Members, Administrators
- Record payments: Board Members, Administrators
- Manage delinquent accounts: Board Members, Administrators
- Generate reports: Board Members, Administrators

### Financial Data Protection
- Secure storage of payment information
- Encryption of sensitive financial data
- Audit logging for all financial transactions
- Validation of all financial calculations
- Reconciliation processes for financial integrity

### API Security
- Implement rate limiting on API endpoints
- Use CSRF protection for form submissions
- Apply appropriate caching headers
- Validate all input parameters
- Implement proper authorization checks

## Implementation Checklist

- [ ] **Domain Layer**
  - [ ] Create entities (DuesConfiguration, DuesTransaction, etc.)
  - [ ] Define value objects (Money, DuesFrequency, etc.)
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
  - [ ] Implement statement generation service
  - [ ] Implement notification service
  - [ ] Implement report generation service
  - [ ] Implement integration services

- [ ] **Presentation Layer**
  - [ ] Create API controllers
  - [ ] Implement resident dues view component
  - [ ] Implement payment history component
  - [ ] Implement statement view component
  - [ ] Implement payment form component
  - [ ] Implement configuration manager component
  - [ ] Implement delinquent account manager component
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
