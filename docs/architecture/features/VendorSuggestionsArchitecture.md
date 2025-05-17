# Vendor Suggestions Feature Architecture Guide

## Overview

This document provides architectural guidance for implementing the Vendor Suggestions feature in the Wendover HOA application, mapping components to the Clean Architecture layers, CQRS/MediatR patterns, and identifying cross-cutting concerns. The Vendor Suggestions feature allows residents to submit, view, and vote on suggested vendors for community services, creating a community-vetted resource.

## Domain Layer Components

### Entities
- **Vendor**: Core entity representing a vendor suggestion
- **VendorCategory**: Entity representing service categories
- **VendorReview**: Entity representing user reviews of vendors
- **VendorVote**: Entity representing user votes on vendors
- **VendorPhoto**: Entity representing photos of vendor work
- **VendorModeration**: Entity tracking moderation actions

### Value Objects
- **VendorStatus**: Enum (Pending, Approved, Rejected)
- **VoteType**: Enum (Upvote, Downvote)
- **ModerationType**: Enum (Approval, Rejection, Feature, Unfeature)
- **Rating**: Value object for star ratings (1-5)
- **ContactInfo**: Value object for vendor contact information
- **Address**: Value object for vendor address

### Domain Events
- **VendorSuggestionSubmittedEvent**: Raised when vendor is submitted
- **VendorSuggestionApprovedEvent**: Raised when vendor is approved
- **VendorSuggestionRejectedEvent**: Raised when vendor is rejected
- **VendorReviewSubmittedEvent**: Raised when review is submitted
- **VendorReviewApprovedEvent**: Raised when review is approved
- **VendorVoteCastEvent**: Raised when vote is cast
- **VendorFeaturedEvent**: Raised when vendor is featured
- **VendorCategoryCreatedEvent**: Raised when category is created

### Domain Services
- **VendorValidationService**: Validates vendor data
- **ReviewValidationService**: Validates review content
- **VotingService**: Manages voting logic and restrictions
- **ModerationService**: Handles moderation workflows
- **RatingCalculationService**: Calculates vendor ratings

### Domain Interfaces
- **IVendorRepository**: Repository for Vendor entities
- **IVendorCategoryRepository**: Repository for VendorCategory entities
- **IVendorReviewRepository**: Repository for VendorReview entities
- **IVendorVoteRepository**: Repository for VendorVote entities
- **IVendorPhotoRepository**: Repository for VendorPhoto entities
- **IVendorModerationRepository**: Repository for VendorModeration entities

## Application Layer Components

### Commands
- **SubmitVendorSuggestionCommand**: Submits new vendor
- **UpdateVendorSuggestionCommand**: Updates existing vendor
- **DeleteVendorSuggestionCommand**: Deletes vendor
- **ApproveVendorSuggestionCommand**: Approves vendor
- **RejectVendorSuggestionCommand**: Rejects vendor
- **FeatureVendorCommand**: Features vendor
- **UnfeatureVendorCommand**: Removes featured status
- **SubmitVendorReviewCommand**: Submits review
- **UpdateVendorReviewCommand**: Updates review
- **DeleteVendorReviewCommand**: Deletes review
- **ApproveVendorReviewCommand**: Approves review
- **RejectVendorReviewCommand**: Rejects review
- **CastVendorVoteCommand**: Casts vote
- **UpdateVendorVoteCommand**: Updates vote
- **RemoveVendorVoteCommand**: Removes vote
- **CreateVendorCategoryCommand**: Creates category
- **UpdateVendorCategoryCommand**: Updates category
- **DeleteVendorCategoryCommand**: Deletes category
- **UploadVendorPhotoCommand**: Uploads photo
- **DeleteVendorPhotoCommand**: Deletes photo
- **UpdateVendorSettingsCommand**: Updates settings

### Queries
- **GetVendorByIdQuery**: Gets specific vendor
- **GetVendorsQuery**: Gets filtered list of vendors
- **GetVendorReviewsQuery**: Gets reviews for vendor
- **GetVendorVotesQuery**: Gets votes for vendor
- **GetVendorPhotosQuery**: Gets photos for vendor
- **GetVendorCategoriesQuery**: Gets all categories
- **GetVendorCategoryByIdQuery**: Gets specific category
- **GetPendingVendorsQuery**: Gets pending vendors
- **GetPendingReviewsQuery**: Gets pending reviews
- **GetUserVotesQuery**: Gets votes by user
- **GetUserReviewsQuery**: Gets reviews by user
- **SearchVendorsQuery**: Searches across vendors
- **GenerateVendorPopularityReportQuery**: Generates popularity report
- **GenerateVendorCategoryReportQuery**: Generates category report
- **GenerateVendorVotingReportQuery**: Generates voting report
- **GenerateVendorReviewReportQuery**: Generates review report
- **GetVendorSettingsQuery**: Gets vendor settings

### DTOs
- **VendorDto**: Data transfer object for Vendor
- **VendorSummaryDto**: Summary DTO with limited fields
- **VendorCategoryDto**: DTO for VendorCategory
- **VendorReviewDto**: DTO for VendorReview
- **VendorVoteDto**: DTO for VendorVote
- **VendorPhotoDto**: DTO for VendorPhoto
- **VendorModerationDto**: DTO for VendorModeration
- **VendorSearchResultDto**: DTO for search results
- **VendorReportDto**: DTO for vendor reports
- **VendorSettingsDto**: DTO for vendor settings

### Validators
- **SubmitVendorSuggestionCommandValidator**: Validates vendor submission
- **UpdateVendorSuggestionCommandValidator**: Validates vendor updates
- **SubmitVendorReviewCommandValidator**: Validates review submission
- **CastVendorVoteCommandValidator**: Validates vote casting
- **CreateVendorCategoryCommandValidator**: Validates category creation
- **UploadVendorPhotoCommandValidator**: Validates photo uploads
- **SearchVendorsQueryValidator**: Validates search parameters

### Mapping Profiles
- **VendorMappingProfile**: Maps between entities and DTOs

## Infrastructure Layer Components

### Repositories
- **VendorRepository**: Implements IVendorRepository
- **VendorCategoryRepository**: Implements IVendorCategoryRepository
- **VendorReviewRepository**: Implements IVendorReviewRepository
- **VendorVoteRepository**: Implements IVendorVoteRepository
- **VendorPhotoRepository**: Implements IVendorPhotoRepository
- **VendorModerationRepository**: Implements IVendorModerationRepository

### Persistence Configurations
- **VendorConfiguration**: EF Core configuration for Vendor
- **VendorCategoryConfiguration**: EF Core configuration for VendorCategory
- **VendorReviewConfiguration**: EF Core configuration for VendorReview
- **VendorVoteConfiguration**: EF Core configuration for VendorVote
- **VendorPhotoConfiguration**: EF Core configuration for VendorPhoto
- **VendorModerationConfiguration**: EF Core configuration for VendorModeration

### External Services
- **FileStorageService**: Handles photo storage
- **EmailNotificationService**: Sends notifications about vendors
- **SpamDetectionService**: Detects and prevents spam submissions
- **ContentModerationService**: Moderates user-submitted content
- **ReportGenerationService**: Generates vendor reports
- **ImageProcessingService**: Processes and optimizes vendor photos

## Presentation Layer Components

### API Controllers
- **VendorsController**: API endpoints for vendors
- **VendorReviewsController**: API endpoints for reviews
- **VendorVotesController**: API endpoints for votes
- **VendorCategoriesController**: API endpoints for categories
- **VendorPhotosController**: API endpoints for photos
- **VendorModerationController**: API endpoints for moderation
- **VendorReportsController**: API endpoints for reports
- **VendorSettingsController**: API endpoints for settings

### Blazor Components
- **VendorList**: Displays list of vendor suggestions
- **VendorDetail**: Displays vendor details
- **VendorSubmissionForm**: Form for submitting vendors
- **VendorReviewForm**: Form for submitting reviews
- **VendorVotingControl**: Control for voting on vendors
- **VendorCategoryBrowser**: Displays vendor categories
- **VendorPhotoGallery**: Displays vendor photos
- **VendorModerationQueue**: Interface for moderating vendors
- **VendorCategoryManager**: Interface for managing categories
- **VendorReportGenerator**: Interface for generating reports
- **VendorSettingsManager**: Interface for managing settings
- **VendorSearch**: Search interface for vendors

### View Models
- **VendorListViewModel**: View model for vendor list
- **VendorDetailViewModel**: View model for vendor details
- **VendorSubmissionViewModel**: View model for vendor submission
- **VendorReviewViewModel**: View model for review submission
- **VendorCategoryViewModel**: View model for category management
- **VendorModerationViewModel**: View model for moderation
- **VendorReportViewModel**: View model for report generation
- **VendorSettingsViewModel**: View model for settings management
- **VendorSearchViewModel**: View model for vendor search

## Cross-Cutting Concerns

### Logging
- Log vendor submissions and updates
- Log review submissions and moderation
- Log vote casting and changes
- Log moderation actions
- Log category management
- Log report generation
- Log settings changes

### Caching
- Cache approved vendors (short duration)
- Cache vendor categories (longer duration)
- Cache vendor ratings (short duration)
- Cache popular vendors (medium duration)
- Cache settings (longer duration)
- Cache search results (very short duration)

### Exception Handling
- Handle vendor not found
- Handle category not found
- Handle unauthorized access
- Handle validation failures
- Handle file storage failures
- Handle duplicate votes
- Handle report generation failures

## Security Considerations

### Role-Based Access Control
- View vendors: All users
- Submit vendors: Authenticated users
- Vote on vendors: Authenticated users
- Submit reviews: Authenticated users
- Moderate vendors: Board Members, Administrators
- Manage categories: Administrators
- Generate reports: Board Members, Administrators
- Configure settings: Administrators

### Anti-Fraud Measures
- Prevent duplicate votes from same user
- Rate limiting for submissions
- Moderation workflow for all user content
- Audit trail for all moderation actions
- Prevention of self-voting on own submissions

### Data Protection
- Validate and sanitize all user input
- Secure handling of uploaded photos
- Proper authorization checks on all operations
- Protection against spam submissions
- Content moderation for inappropriate material

### API Security
- Implement rate limiting on API endpoints
- Use CSRF protection for form submissions
- Apply appropriate caching headers
- Validate file uploads

## Implementation Checklist

- [ ] **Domain Layer**
  - [ ] Create entities (Vendor, VendorCategory, VendorReview, etc.)
  - [ ] Define value objects (VendorStatus, VoteType, etc.)
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
  - [ ] Implement file storage service
  - [ ] Implement notification service
  - [ ] Implement spam detection service
  - [ ] Implement content moderation service
  - [ ] Implement report generation service
  - [ ] Implement image processing service

- [ ] **Presentation Layer**
  - [ ] Create API controllers
  - [ ] Implement vendor list component
  - [ ] Implement vendor detail component
  - [ ] Implement vendor submission form
  - [ ] Implement review form component
  - [ ] Implement voting control component
  - [ ] Implement moderation queue component
  - [ ] Implement category management component
  - [ ] Implement report generator component
  - [ ] Create view models

- [ ] **Cross-Cutting Concerns**
  - [ ] Configure logging
  - [ ] Implement caching
  - [ ] Set up exception handling

- [ ] **Security**
  - [ ] Configure role-based access control
  - [ ] Implement anti-fraud measures
  - [ ] Configure content sanitization
  - [ ] Implement rate limiting
  - [ ] Configure API security

- [ ] **Testing**
  - [ ] Unit tests for domain entities and services
  - [ ] Unit tests for command and query handlers
  - [ ] Integration tests for repositories
  - [ ] Integration tests for API endpoints
  - [ ] Tests for voting and review logic
  - [ ] Tests for moderation workflow
