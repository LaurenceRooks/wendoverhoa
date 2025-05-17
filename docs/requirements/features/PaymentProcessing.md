# Payment Processing

## Overview
This document outlines the requirements for the payment processing feature of the Wendover HOA web application. This feature will provide secure, efficient methods for residents to make payments for HOA dues and fees, while giving administrators tools to manage and reconcile payments for the Wendover Homeowners Association in Bedford, Texas.

## User Roles
1. **Guest** - No access to payment processing
2. **Resident** - Can make payments for dues and fees
3. **Committee Member** - Same access as Resident
4. **Board Member** - Can view payment history and manage payment configurations
5. **Administrator** - Can configure payment methods, process refunds, and manage the payment system

## Use Cases

### UC-PAY-01: Make Online Payment
**Primary Actor:** Resident
**Description:** Allow residents to make payments online
**Preconditions:** User is authenticated as a resident
**Postconditions:** Payment is processed and recorded

**Main Flow:**
1. Resident navigates to the payment section
2. System displays current balance and payment options
3. Resident selects payment amount and method (credit card, ACH/bank transfer)
4. Resident enters payment details or selects saved payment method
5. System securely processes payment through payment gateway
6. System records transaction and updates resident's balance
7. System generates receipt and confirmation

**Alternative Flows:**
- Save payment method for future use
- Set up recurring automatic payments
- Cancel payment before final submission

### UC-PAY-02: Configure Payment Methods
**Primary Actor:** Administrator
**Description:** Configure available payment methods and processing settings
**Preconditions:** User is authenticated as Administrator
**Postconditions:** Payment methods are configured

**Main Flow:**
1. Administrator navigates to payment configuration section
2. Administrator can:
   - Enable/disable payment methods (credit card, ACH, etc.)
   - Configure payment gateway settings
   - Set processing fees and convenience fees
   - Configure receipt templates
   - Set up payment notification settings
3. System validates configuration changes
4. System applies new configuration

**Alternative Flows:**
- Test payment gateway connection
- View payment gateway transaction logs
- Configure backup payment processor

### UC-PAY-03: Process Refunds
**Primary Actor:** Board Member or Administrator
**Description:** Process refunds for residents
**Preconditions:** User is authenticated as Board Member or Administrator
**Postconditions:** Refund is processed and recorded

**Main Flow:**
1. Administrator navigates to refund processing section
2. Administrator searches for the original payment
3. Administrator enters:
   - Refund amount (full or partial)
   - Reason for refund
   - Refund method
   - Notes
4. System validates refund information
5. System processes refund through payment gateway
6. System records refund transaction and updates resident's balance
7. System generates refund confirmation and notification

**Alternative Flows:**
- Process refund to alternative payment method
- Process batch refunds for multiple residents
- Cancel refund before final submission

### UC-PAY-04: Payment Reconciliation
**Primary Actor:** Board Member or Administrator
**Description:** Reconcile payments with bank statements and financial records
**Preconditions:** User is authenticated as Board Member or Administrator
**Postconditions:** Payments are reconciled

**Main Flow:**
1. Administrator navigates to payment reconciliation section
2. Administrator selects reconciliation period
3. System displays all payments and refunds for the period
4. Administrator can:
   - Mark payments as reconciled
   - Flag discrepancies for investigation
   - Add notes to transactions
   - Generate reconciliation report
5. System updates reconciliation status for all transactions
6. System generates final reconciliation report

**Alternative Flows:**
- Import bank statement data for automated matching
- Export unreconciled transactions for external review
- Schedule automatic reconciliation reports

### UC-PAY-05: Payment Reporting
**Primary Actor:** Board Member or Administrator
**Description:** Generate reports on payment activity
**Preconditions:** User is authenticated as Board Member or Administrator
**Postconditions:** Payment reports are generated

**Main Flow:**
1. Administrator navigates to payment reporting section
2. Administrator selects:
   - Report type (transaction summary, payment method analysis, etc.)
   - Date range
   - Grouping and filtering options
3. System generates the requested report
4. Administrator can view, print, or export the report

**Alternative Flows:**
- Schedule recurring reports
- Save report templates for future use
- Generate custom reports with specific parameters

### UC-PAY-06: View Payment History
**Primary Actor:** Resident
**Description:** Allow residents to view their payment history
**Preconditions:** User is authenticated as a resident
**Postconditions:** Resident views their payment history

**Main Flow:**
1. Resident navigates to payment history section
2. System displays:
   - List of all payments with dates, amounts, and status
   - Payment method used for each transaction
   - Receipt/confirmation numbers
   - Running balance
3. Resident can filter history by date range or payment type
4. Resident can download receipts for individual payments

**Alternative Flows:**
- Generate payment summary for tax purposes
- Report payment discrepancies
- View pending scheduled payments

### UC-PAY-07: Delete Payment Record
**Primary Actor:** Administrator
**Description:** Allow administrators to delete erroneous payment records
**Preconditions:** User is authenticated as Administrator; payment record exists and has not been reconciled
**Postconditions:** Payment record is deleted

**Main Flow:**
1. Administrator navigates to payment management page
2. Administrator searches for the payment record to delete
3. System verifies payment has not been reconciled or processed
4. System prompts for confirmation with warning about financial implications
5. Administrator confirms deletion
6. System deletes the payment record
7. System confirms successful deletion
8. System logs the deletion action with timestamp and user ID

**Alternative Flows:**
- If Administrator cancels deletion, return to previous state
- If payment has been processed or reconciled, system prevents deletion and suggests voiding or archiving
- System requires dual authorization for deletion of payments above configured threshold
- System generates audit record for compliance purposes

### UC-PAY-08: Archive Payment Record
**Primary Actor:** Board Member or Administrator
**Description:** Allow authorized users to archive payment records for historical reference
**Preconditions:** User is authenticated as Board Member or Administrator; payment record exists
**Postconditions:** Payment record is archived and no longer appears in active listings

**Main Flow:**
1. User navigates to payment management page
2. User selects a payment record to archive
3. System prompts for confirmation
4. User confirms archiving
5. System moves the payment record to the archive
6. System confirms successful archiving
7. System logs the archiving action with timestamp and user ID

**Alternative Flows:**
- If user cancels archiving, return to previous state
- Option to add archive notes or reason for archiving
- Administrators can access archived payment records through the Administrative Controls interface
- System maintains financial integrity by preserving payment relationships in reports

## Technical Requirements

1. **Implementation**
   - Follow Clean Architecture principles
   - Follow SOLID, DRY, KISS, and YAGNI principles
   - Use CQRS pattern with MediatR for payment operations
   - Create comprehensive unit and integration tests for all functionality
   - Pass security scanning using GitHub's free tools (CodeQL, dependency scanning, secret scanning)

2. **Data Storage**
   - Store payment data in Microsoft SQL Server 2022
   - Implement proper data relationships between residents and payments
   - Maintain historical records of all payment transactions
   - Implement audit logging for all payment operations
   - Ensure PCI DSS compliance for payment data storage

3. **Integration**
   - Integrate with secure payment gateway (Stripe, PayPal, or similar)
   - Integrate with Dues Tracking for balance updates
   - Integrate with Financial Reporting for financial analysis
   - Integrate with Authentication for role-based access control
   - Implement webhook handling for payment status updates

4. **Security**
   - Implement PCI DSS compliant payment processing
   - Ensure secure handling of financial information
   - Provide audit trails for all payment activities
   - Implement fraud detection mechanisms
   - Secure all payment API endpoints with proper authentication

## UI/UX Requirements

1. **Payment Interface**
   - Clean, intuitive payment form
   - Clear display of payment amount and breakdown
   - Mobile-responsive design for all payment interfaces
   - Accessible design following WCAG 2.1 AA standards
   - Real-time validation of payment information

2. **Administrator Interface**
   - Comprehensive dashboard for payment management
   - Clear visualization of payment trends
   - Efficient tools for payment reconciliation
   - Intuitive refund processing workflow

3. **Reporting Interface**
   - Interactive charts and graphs for payment data visualization
   - Customizable report parameters
   - Export options in multiple formats (PDF, Excel, CSV)
   - Saved report templates for common reports

## API Endpoints

1. **Payment Processing API**
   - `POST /api/payments/process` - Process a new payment
   - `GET /api/payments/{id}` - Get specific payment details
   - `POST /api/payments/refund/{id}` - Process refund for payment
   - `GET /api/payments/methods` - Get available payment methods

2. **Payment Management API**
   - `GET /api/payments/history/{residentId}` - Get payment history for resident
   - `GET /api/payments/reconciliation/{period}` - Get reconciliation data
   - `PUT /api/payments/reconcile/{id}` - Update reconciliation status
   - `GET /api/payments/reports/{type}` - Generate specified report

3. **Payment Configuration API**
   - `GET /api/payments/configuration` - Get payment configuration
   - `PUT /api/payments/configuration` - Update payment configuration
   - `POST /api/payments/gateway/test` - Test payment gateway connection

## Database Schema

1. **PaymentMethods Table**
   - PaymentMethodId (PK)
   - ResidentId (FK to Residents)
   - Type (Credit Card, ACH, etc.)
   - NameOnAccount
   - Last4Digits
   - ExpirationDate
   - BillingAddress
   - IsDefault
   - Status
   - GatewayToken
   - CreatedAt
   - UpdatedAt

2. **PaymentTransactions Table**
   - TransactionId (PK)
   - ResidentId (FK to Residents)
   - PropertyId (FK to Properties)
   - PaymentMethodId (FK to PaymentMethods)
   - TransactionType (Payment, Refund)
   - Amount
   - ProcessingFee
   - TotalAmount
   - Description
   - Status
   - GatewayTransactionId
   - ReceiptNumber
   - Notes
   - IsReconciled
   - ReconciliationDate
   - CreatedAt
   - UpdatedAt

3. **PaymentConfiguration Table**
   - ConfigurationId (PK)
   - EnabledPaymentMethods
   - ProcessingFeeType (Fixed, Percentage)
   - ProcessingFeeAmount
   - MinimumPaymentAmount
   - MaximumPaymentAmount
   - GatewayType
   - GatewaySettings (JSON)
   - ReceiptTemplate
   - NotificationSettings
   - CreatedAt
   - UpdatedAt
   - UpdatedBy

4. **RecurringPayments Table**
   - RecurringPaymentId (PK)
   - ResidentId (FK to Residents)
   - PaymentMethodId (FK to PaymentMethods)
   - Amount
   - Frequency (Monthly, Quarterly, Annual)
   - StartDate
   - EndDate
   - NextPaymentDate
   - Status
   - CreatedAt
   - UpdatedAt

5. **PaymentReconciliation Table**
   - ReconciliationId (PK)
   - ReconciliationPeriodStart
   - ReconciliationPeriodEnd
   - TotalTransactions
   - TotalAmount
   - CompletedBy
   - CompletedDate
   - Notes
   - Status
   - CreatedAt
   - UpdatedAt

## Acceptance Criteria

1. Residents can make secure online payments using multiple payment methods
2. Administrators can configure available payment methods and processing settings
3. The system can process refunds for residents when necessary
4. Administrators can reconcile payments with financial records
5. Comprehensive reports on payment activity can be generated
6. Residents can view their complete payment history and download receipts
7. All payment transactions are properly logged and audited
8. Integration with payment gateway is secure and reliable
9. All payment interfaces are responsive and accessible
10. Payment processing meets PCI DSS compliance requirements
11. Payment API endpoints function correctly with proper authorization
