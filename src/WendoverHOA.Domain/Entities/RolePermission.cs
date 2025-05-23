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
    public UserRole Role { get; set; }
    
    /// <summary>
    /// The role name (string representation of the role)
    /// </summary>
    public string RoleName { get; set; } = string.Empty;
    
    /// <summary>
    /// The permission assigned to this role
    /// </summary>
    public Permission Permission { get; set; }
    
    /// <summary>
    /// Description of the role
    /// </summary>
    public string Description { get; private set; } = string.Empty;
    
    // Private constructor for EF Core
    private RolePermission() { }
    
    public RolePermission(UserRole role, Permission permission, string description)
    {
        Role = role;
        RoleName = role.ToString();
        Permission = permission;
        Description = description;
    }
    
    /// <summary>
    /// Sets the permission for this role permission mapping
    /// </summary>
    /// <param name="permission">The permission to set</param>
    public void SetPermission(Permission permission)
    {
        Permission = permission;
    }
    
    /// <summary>
    /// Checks if this role permission mapping has the specified permission
    /// </summary>
    /// <param name="permission">The permission to check</param>
    /// <returns>True if the role permission mapping has the specified permission, false otherwise</returns>
    public bool HasPermission(Permission permission) => Permission.Equals(permission);
    
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
