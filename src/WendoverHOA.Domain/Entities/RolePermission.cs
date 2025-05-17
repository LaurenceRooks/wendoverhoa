using WendoverHOA.Domain.Common;
using WendoverHOA.Domain.Enums;

namespace WendoverHOA.Domain.Entities;

/// <summary>
/// Represents the mapping between user roles and their associated permissions
/// </summary>
public class RolePermission : BaseEntity
{
    /// <summary>
    /// The user role
    /// </summary>
    public UserRole Role { get; private set; }
    
    /// <summary>
    /// The permissions assigned to this role
    /// </summary>
    public ICollection<Permission> Permissions { get; private set; } = new List<Permission>();
    
    /// <summary>
    /// Description of the role
    /// </summary>
    public string Description { get; private set; } = string.Empty;
    
    // Private constructor for EF Core
    private RolePermission() { }
    
    public RolePermission(UserRole role, string description)
    {
        Role = role;
        Description = description;
    }
    
    /// <summary>
    /// Adds a permission to the role if it doesn't already exist
    /// </summary>
    /// <param name="permission">The permission to add</param>
    public void AddPermission(Permission permission)
    {
        if (!Permissions.Contains(permission))
        {
            Permissions.Add(permission);
        }
    }
    
    /// <summary>
    /// Adds multiple permissions to the role
    /// </summary>
    /// <param name="permissions">The permissions to add</param>
    public void AddPermissions(IEnumerable<Permission> permissions)
    {
        foreach (var permission in permissions)
        {
            AddPermission(permission);
        }
    }
    
    /// <summary>
    /// Removes a permission from the role if it exists
    /// </summary>
    /// <param name="permission">The permission to remove</param>
    public void RemovePermission(Permission permission)
    {
        if (Permissions.Contains(permission))
        {
            Permissions.Remove(permission);
        }
    }
    
    /// <summary>
    /// Checks if the role has a specific permission
    /// </summary>
    /// <param name="permission">The permission to check</param>
    /// <returns>True if the role has the permission, false otherwise</returns>
    public bool HasPermission(Permission permission) => Permissions.Contains(permission);
    
    /// <summary>
    /// Updates the description of the role
    /// </summary>
    /// <param name="description">The new description</param>
    public void UpdateDescription(string description)
    {
        if (string.IsNullOrWhiteSpace(description))
        {
            throw new ArgumentException("Role description cannot be empty", nameof(description));
        }
        
        Description = description;
    }
}
