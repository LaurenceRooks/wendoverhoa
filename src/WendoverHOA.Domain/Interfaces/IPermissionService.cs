using WendoverHOA.Domain.Entities;
using WendoverHOA.Domain.Enums;

namespace WendoverHOA.Domain.Interfaces;

/// <summary>
/// Service interface for managing permissions in the application
/// </summary>
public interface IPermissionService
{
    /// <summary>
    /// Gets all permissions for a specific user role
    /// </summary>
    /// <param name="role">The user role</param>
    /// <returns>A collection of permissions assigned to the role</returns>
    IReadOnlyList<Permission> GetPermissionsForRole(UserRole role);
    
    /// <summary>
    /// Checks if a user has a specific permission
    /// </summary>
    /// <param name="user">The user to check</param>
    /// <param name="permission">The permission to check for</param>
    /// <returns>True if the user has the permission, otherwise false</returns>
    bool HasPermission(ApplicationUser user, Permission permission);
    
    /// <summary>
    /// Checks if a user has any of the specified permissions
    /// </summary>
    /// <param name="user">The user to check</param>
    /// <param name="permissions">The permissions to check for</param>
    /// <returns>True if the user has any of the permissions, otherwise false</returns>
    bool HasAnyPermission(ApplicationUser user, IEnumerable<Permission> permissions);
    
    /// <summary>
    /// Checks if a user has all of the specified permissions
    /// </summary>
    /// <param name="user">The user to check</param>
    /// <param name="permissions">The permissions to check for</param>
    /// <returns>True if the user has all of the permissions, otherwise false</returns>
    bool HasAllPermissions(ApplicationUser user, IEnumerable<Permission> permissions);
    
    /// <summary>
    /// Gets all permissions for a specific user based on their roles and explicit permissions
    /// </summary>
    /// <param name="user">The user to get permissions for</param>
    /// <returns>A collection of all permissions the user has</returns>
    IReadOnlyList<Permission> GetAllPermissionsForUser(ApplicationUser user);
    
    /// <summary>
    /// Assigns a permission to a user
    /// </summary>
    /// <param name="user">The user to assign the permission to</param>
    /// <param name="permission">The permission to assign</param>
    void AssignPermissionToUser(ApplicationUser user, Permission permission);
    
    /// <summary>
    /// Revokes a permission from a user
    /// </summary>
    /// <param name="user">The user to revoke the permission from</param>
    /// <param name="permission">The permission to revoke</param>
    void RevokePermissionFromUser(ApplicationUser user, Permission permission);
}
