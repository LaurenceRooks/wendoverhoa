# Standardized User Roles

This document defines the standardized user roles for the Wendover HOA application. These roles should be consistently used across all features and documentation to ensure proper role-based access control.

## User Roles

The Wendover HOA application uses the following standardized user roles:

1. **Guest**
   - Unauthenticated users with limited access
   - Can view public information only
   - Cannot perform any actions that modify data
   - Examples: viewing public announcements, accessing public documents

2. **Resident**
   - Authenticated homeowners with basic access
   - Can view all community information
   - Can submit feedback and requests
   - Can manage their own profile and property information
   - Can make payments for their dues
   - Cannot modify community-wide content

3. **Board Member**
   - HOA board members with administrative access
   - Can create and edit announcements
   - Can manage calendar events
   - Can upload and manage documents
   - Can create and edit meeting minutes
   - Can view and respond to user feedback
   - Can manage vendor suggestions
   - Can view financial reports

4. **Administrator**
   - Full system access for managing all aspects of the application
   - Can manage user accounts and roles
   - Can configure system settings
   - Can manage all community content
   - Can process financial transactions
   - Can generate and approve reports
   - Has access to all administrative functions

## Role Hierarchy

The roles follow a hierarchical structure where each higher role inherits all permissions from lower roles:

```
Guest < Resident < Board Member < Administrator
```

## Role-Based Access Control

When implementing features, follow these guidelines for role-based access control:

1. **Authentication Requirements**
   - Clearly specify which endpoints require authentication
   - Use the `[Authorize]` attribute for protected endpoints
   - Use the `[AllowAnonymous]` attribute for public endpoints

2. **Authorization Requirements**
   - Use role-based authorization with the `[Authorize(Roles = "...")]` attribute
   - Implement policy-based authorization for complex permission scenarios
   - Check user roles in controllers and services when necessary

3. **UI Considerations**
   - Hide UI elements that users don't have permission to access
   - Provide clear feedback when a user attempts an unauthorized action
   - Ensure navigation only shows relevant options based on the user's role

## Implementation Examples

### Controller Authorization

```csharp
// Public endpoint accessible to all users
[AllowAnonymous]
[HttpGet]
public async Task<ActionResult<List<AnnouncementDto>>> GetPublicAnnouncements()
{
    // Implementation
}

// Endpoint accessible to authenticated users (Residents and above)
[Authorize]
[HttpGet("my-profile")]
public async Task<ActionResult<UserProfileDto>> GetMyProfile()
{
    // Implementation
}

// Endpoint accessible only to Board Members and Administrators
[Authorize(Roles = "BoardMember,Administrator")]
[HttpPost]
public async Task<ActionResult<int>> CreateAnnouncement(CreateAnnouncementCommand command)
{
    // Implementation
}

// Endpoint accessible only to Administrators
[Authorize(Roles = "Administrator")]
[HttpPost("users/roles")]
public async Task<ActionResult> AssignUserRole(AssignRoleCommand command)
{
    // Implementation
}
```

### UI Conditional Rendering

```html
<!-- Public content visible to all users -->
<div class="public-content">
    <!-- Content here -->
</div>

<!-- Content only visible to authenticated users -->
@if (User.Identity.IsAuthenticated)
{
    <div class="authenticated-content">
        <!-- Content here -->
    </div>
}

<!-- Content only visible to Board Members and Administrators -->
@if (User.IsInRole("BoardMember") || User.IsInRole("Administrator"))
{
    <div class="board-member-content">
        <!-- Content here -->
    </div>
}

<!-- Content only visible to Administrators -->
@if (User.IsInRole("Administrator"))
{
    <div class="admin-content">
        <!-- Content here -->
    </div>
}
```

## Migration from Previous Role Structure

The Committee Member role has been removed from the application. Any functionality previously assigned to Committee Members should now be handled as follows:

1. For committee-specific content management:
   - Board Members now have responsibility for all committee content
   - Use feature-specific permissions rather than role-based permissions for committee-related tasks
   - Implement a committee membership attribute for Residents who are part of committees

2. For permission inheritance:
   - Any permissions previously granted to Committee Members are now granted to Board Members
   - No functionality should be lost in this transition

## Conclusion

Consistent use of these standardized user roles across the application ensures proper access control and a better user experience. All documentation, code, and UI should reflect these roles and their associated permissions.
