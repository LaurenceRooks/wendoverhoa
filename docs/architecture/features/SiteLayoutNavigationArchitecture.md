# Site Layout and Navigation Feature Architecture Guide

## Overview

This document provides architectural guidance for implementing the Site Layout and Navigation feature in the Wendover HOA application, mapping components to the Clean Architecture layers, CQRS/MediatR patterns, and identifying cross-cutting concerns. The Site Layout and Navigation feature provides the core UI framework, navigation structure, user preferences, notifications, and search functionality.

## Domain Layer Components

### Entities
- **NavigationItem**: Represents an item in the navigation menu
- **UserPreference**: Stores user preferences for UI
- **Notification**: Represents a user notification
- **SearchIndex**: Represents searchable content
- **Theme**: Represents a UI theme
- **Banner**: Represents a site-wide banner or announcement
- **SiteConfiguration**: Stores global site configuration

### Value Objects
- **BreadcrumbTrail**: Represents a breadcrumb navigation path
- **BreadcrumbItem**: Represents a single breadcrumb item
- **NotificationType**: Enum (Info, Success, Warning, Error)
- **SearchResult**: Represents a search result item
- **ThemeColors**: Value object for theme color scheme
- **BannerType**: Enum (Info, Success, Warning, Error, Maintenance)
- **LayoutType**: Enum (Default, Compact, Expanded)

### Domain Events
- **UserPreferenceChangedEvent**: Raised when a user changes a preference
- **NotificationCreatedEvent**: Raised when a notification is created
- **NotificationReadEvent**: Raised when a notification is read
- **SearchPerformedEvent**: Raised when a search is performed
- **ThemeChangedEvent**: Raised when a user changes theme
- **BannerCreatedEvent**: Raised when a banner is created
- **BannerRemovedEvent**: Raised when a banner is removed

### Domain Services
- **NavigationService**: Manages navigation structure
- **SearchService**: Handles search functionality
- **NotificationService**: Manages user notifications
- **BreadcrumbService**: Generates breadcrumb trails
- **ThemeService**: Manages themes and user preferences
- **BannerService**: Manages site-wide banners

### Domain Interfaces
- **INavigationRepository**: Repository for NavigationItem entities
- **IUserPreferenceRepository**: Repository for UserPreference entities
- **INotificationRepository**: Repository for Notification entities
- **ISearchRepository**: Repository for search functionality
- **IThemeRepository**: Repository for Theme entities
- **IBannerRepository**: Repository for Banner entities
- **ISiteConfigurationRepository**: Repository for SiteConfiguration entity

## Application Layer Components

### Commands
- **UpdateUserPreferenceCommand**: Updates a user preference
- **SetThemePreferenceCommand**: Sets user's theme preference
- **CreateNotificationCommand**: Creates a notification
- **MarkNotificationAsReadCommand**: Marks a notification as read
- **DeleteNotificationCommand**: Deletes a notification
- **IndexSearchContentCommand**: Indexes content for search
- **CreateBannerCommand**: Creates a site-wide banner
- **UpdateBannerCommand**: Updates a banner
- **RemoveBannerCommand**: Removes a banner
- **UpdateNavigationItemCommand**: Updates navigation item
- **CreateNavigationItemCommand**: Creates navigation item
- **DeleteNavigationItemCommand**: Deletes navigation item
- **UpdateSiteConfigurationCommand**: Updates site configuration

### Queries
- **GetNavigationItemsQuery**: Gets navigation items for user
- **GetUserPreferencesQuery**: Gets all preferences for a user
- **GetUserPreferenceByKeyQuery**: Gets specific preference for a user
- **GetNotificationsQuery**: Gets notifications for a user
- **GetUnreadNotificationCountQuery**: Gets count of unread notifications
- **SearchQuery**: Performs a search
- **GetThemesQuery**: Gets all available themes
- **GetCurrentThemeQuery**: Gets current theme for user
- **GetBreadcrumbTrailQuery**: Gets breadcrumb trail for URL
- **GetActiveBannersQuery**: Gets active banners
- **GetSiteConfigurationQuery**: Gets site configuration

### DTOs
- **NavigationItemDto**: DTO for navigation item
- **BreadcrumbTrailDto**: DTO for breadcrumb trail
- **BreadcrumbItemDto**: DTO for breadcrumb item
- **UserPreferenceDto**: DTO for user preference
- **NotificationDto**: DTO for notification
- **SearchResultDto**: DTO for search result
- **ThemeDto**: DTO for theme
- **BannerDto**: DTO for banner
- **SiteConfigurationDto**: DTO for site configuration

### Validators
- **UpdateUserPreferenceCommandValidator**: Validates preference updates
- **SetThemePreferenceCommandValidator**: Validates theme preference
- **CreateNotificationCommandValidator**: Validates notification creation
- **MarkNotificationAsReadCommandValidator**: Validates marking as read
- **DeleteNotificationCommandValidator**: Validates notification deletion
- **IndexSearchContentCommandValidator**: Validates search indexing
- **CreateBannerCommandValidator**: Validates banner creation
- **UpdateBannerCommandValidator**: Validates banner updates
- **SearchQueryValidator**: Validates search queries

### Mapping Profiles
- **NavigationMappingProfile**: Maps between entities and DTOs

## Infrastructure Layer Components

### Repositories
- **NavigationRepository**: Implements INavigationRepository
- **UserPreferenceRepository**: Implements IUserPreferenceRepository
- **NotificationRepository**: Implements INotificationRepository
- **SearchRepository**: Implements ISearchRepository
- **ThemeRepository**: Implements IThemeRepository
- **BannerRepository**: Implements IBannerRepository
- **SiteConfigurationRepository**: Implements ISiteConfigurationRepository

### Persistence Configurations
- **NavigationItemConfiguration**: EF Core configuration for NavigationItem
- **UserPreferenceConfiguration**: EF Core configuration for UserPreference
- **NotificationConfiguration**: EF Core configuration for Notification
- **SearchIndexConfiguration**: EF Core configuration for SearchIndex
- **ThemeConfiguration**: EF Core configuration for Theme
- **BannerConfiguration**: EF Core configuration for Banner
- **SiteConfigurationConfiguration**: EF Core configuration for SiteConfiguration

### External Services
- **FullTextSearchService**: Service for full-text search
- **WeatherService**: Service for weather widget
- **NotificationHubService**: Service for real-time notifications
- **LocalizationService**: Service for localization
- **CookieConsentService**: Service for cookie consent
- **AnalyticsService**: Service for usage analytics

## Presentation Layer Components

### API Controllers
- **NavigationController**: API endpoints for navigation
- **UserPreferenceController**: API endpoints for user preferences
- **NotificationController**: API endpoints for notifications
- **SearchController**: API endpoints for search
- **ThemeController**: API endpoints for themes
- **BannerController**: API endpoints for banners
- **SiteConfigurationController**: API endpoints for site configuration

### Blazor Components
- **MainLayout**: Main layout component
- **NavigationMenu**: Navigation menu component
- **Breadcrumbs**: Breadcrumb navigation component
- **NotificationCenter**: Notification center component
- **SearchBox**: Search component
- **ThemeSelector**: Theme selection component
- **UserPreferencesPanel**: User preferences panel
- **SiteBanner**: Site-wide banner component
- **Footer**: Site footer component
- **Header**: Site header component
- **Sidebar**: Sidebar component
- **MobileNavigation**: Mobile navigation component
- **WeatherWidget**: Weather widget component
- **CookieConsent**: Cookie consent component

### View Models
- **NavigationViewModel**: View model for navigation
- **BreadcrumbViewModel**: View model for breadcrumbs
- **NotificationViewModel**: View model for notifications
- **SearchViewModel**: View model for search
- **ThemeViewModel**: View model for themes
- **BannerViewModel**: View model for banners
- **UserPreferenceViewModel**: View model for user preferences
- **SiteConfigurationViewModel**: View model for site configuration

## Cross-Cutting Concerns

### Logging
- Log preference changes
- Log notification events
- Log search queries
- Log theme changes
- Log banner creation and removal
- Log navigation structure changes
- Log site configuration changes

### Caching
- Cache navigation structure (medium duration)
- Cache user preferences (short duration)
- Cache themes (longer duration)
- Cache search results (very short duration)
- Cache site configuration (medium duration)
- Cache breadcrumb trails (short duration)

### Exception Handling
- Handle navigation errors
- Handle search errors
- Handle preference storage errors
- Handle notification delivery failures
- Handle theme application errors
- Handle banner display issues

## Security Considerations

### Role-Based Access Control
- View navigation: All users (filtered by role)
- Manage preferences: Authenticated users (own preferences)
- Manage notifications: Authenticated users (own notifications)
- Perform searches: All users (results filtered by role)
- Manage themes: Administrators
- Manage banners: Board Members, Administrators
- Configure site: Administrators

### Data Protection
- Validate and sanitize all user preferences
- Secure storage of user preferences
- Proper authorization for notification access
- Sanitize search queries and results
- Validate banner content

### API Security
- Implement rate limiting for search API
- Use CSRF protection for preference updates
- Apply appropriate caching headers
- Validate all input parameters

## Implementation Checklist

- [ ] **Domain Layer**
  - [ ] Create entities (NavigationItem, UserPreference, etc.)
  - [ ] Define value objects (BreadcrumbTrail, NotificationType, etc.)
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
  - [ ] Implement full-text search service
  - [ ] Implement notification hub service
  - [ ] Implement weather service
  - [ ] Implement localization service

- [ ] **Presentation Layer**
  - [ ] Create API controllers
  - [ ] Implement main layout component
  - [ ] Implement navigation menu component
  - [ ] Implement breadcrumb component
  - [ ] Implement notification center component
  - [ ] Implement search box component
  - [ ] Implement theme selector component
  - [ ] Implement banner component
  - [ ] Create view models

- [ ] **Cross-Cutting Concerns**
  - [ ] Configure logging
  - [ ] Implement caching
  - [ ] Set up exception handling

- [ ] **Security**
  - [ ] Configure role-based access control
  - [ ] Implement input validation
  - [ ] Configure API security
  - [ ] Implement content sanitization

- [ ] **Testing**
  - [ ] Unit tests for domain entities and services
  - [ ] Unit tests for command and query handlers
  - [ ] Integration tests for repositories
  - [ ] Integration tests for API endpoints
  - [ ] UI component tests
