using WendoverHOA.Domain.Entities;
using WendoverHOA.Domain.Enums;

namespace WendoverHOA.Domain.Interfaces;

/// <summary>
/// Repository interface for role permission operations
/// </summary>
public interface IRolePermissionRepository : IRepository<RolePermission>
{
    /// <summary>
    /// Gets a role permission by user role
    /// </summary>
    /// <param name="role">The user role</param>
    /// <returns>The role permission if found, otherwise null</returns>
    Task<RolePermission?> GetByRoleAsync(UserRole role);
    
    /// <summary>
    /// Gets all permissions for a specific role
    /// </summary>
    /// <param name="role">The user role</param>
    /// <returns>A collection of permissions assigned to the role</returns>
    Task<IReadOnlyList<Permission>> GetPermissionsForRoleAsync(UserRole role);
    
    /// <summary>
    /// Gets all roles that have a specific permission
    /// </summary>
    /// <param name="permission">The permission</param>
    /// <returns>A collection of user roles that have the permission</returns>
    Task<IReadOnlyList<UserRole>> GetRolesWithPermissionAsync(Permission permission);
}
