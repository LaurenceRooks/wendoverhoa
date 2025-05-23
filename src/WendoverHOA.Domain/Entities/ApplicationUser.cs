using Microsoft.AspNetCore.Identity;
using WendoverHOA.Domain.Common;
using WendoverHOA.Domain.Enums;
using WendoverHOA.Domain.ValueObjects;

namespace WendoverHOA.Domain.Entities;

/// <summary>
/// Represents a user in the Wendover HOA application
/// </summary>
public class ApplicationUser : IdentityUser<int>
{
    /// <summary>
    /// User's first name
    /// </summary>
    public string FirstName { get; set; } = string.Empty;
    
    /// <summary>
    /// User's last name
    /// </summary>
    public string LastName { get; set; } = string.Empty;
    
    /// <summary>
    /// Date when the user account was locked out, if applicable
    /// </summary>
    public new DateTime? LockoutEnd { get; set; }
    
    /// <summary>
    /// Indicates if the user account can be locked out
    /// </summary>
    public new bool LockoutEnabled { get; set; }
    
    /// <summary>
    /// Number of failed access attempts
    /// </summary>
    public new int AccessFailedCount { get; set; }
    
    /// <summary>
    /// Collection of user roles
    /// </summary>
    public ICollection<UserRole> Roles { get; set; } = new List<UserRole>();
    
    /// <summary>
    /// Collection of user permissions
    /// </summary>
    public ICollection<Permission> Permissions { get; set; } = new List<Permission>();
    
    /// <summary>
    /// Last login date and time
    /// </summary>
    public DateTime? LastLoginAt { get; set; }
    
    /// <summary>
    /// User's profile picture URL
    /// </summary>
    public string? ProfilePictureUrl { get; set; }
    
    /// <summary>
    /// User's full name (calculated property)
    /// </summary>
    public string FullName => $"{FirstName} {LastName}".Trim();
    
    /// <summary>
    /// Checks if the user has a specific role
    /// </summary>
    /// <param name="role">The role to check</param>
    /// <returns>True if the user has the role, false otherwise</returns>
    public bool HasRole(UserRole role) => Roles.Contains(role);
    
    /// <summary>
    /// Checks if the user has a specific permission
    /// </summary>
    /// <param name="permission">The permission to check</param>
    /// <returns>True if the user has the permission, false otherwise</returns>
    public bool HasPermission(Permission permission) => Permissions.Contains(permission);
    
    /// <summary>
    /// Adds a role to the user if they don't already have it
    /// </summary>
    /// <param name="role">The role to add</param>
    public void AddRole(UserRole role)
    {
        if (!HasRole(role))
        {
            Roles.Add(role);
        }
    }
    
    /// <summary>
    /// Removes a role from the user if they have it
    /// </summary>
    /// <param name="role">The role to remove</param>
    public void RemoveRole(UserRole role)
    {
        if (HasRole(role))
        {
            Roles.Remove(role);
        }
    }
    
    /// <summary>
    /// Adds a permission to the user if they don't already have it
    /// </summary>
    /// <param name="permission">The permission to add</param>
    public void AddPermission(Permission permission)
    {
        if (!HasPermission(permission))
        {
            Permissions.Add(permission);
        }
    }
    
    /// <summary>
    /// Removes a permission from the user if they have it
    /// </summary>
    /// <param name="permission">The permission to remove</param>
    public void RemovePermission(Permission permission)
    {
        if (HasPermission(permission))
        {
            Permissions.Remove(permission);
        }
    }
}
