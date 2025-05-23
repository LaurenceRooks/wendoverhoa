using System.Linq.Expressions;
using WendoverHOA.Domain.Entities;
using WendoverHOA.Domain.Enums;

namespace WendoverHOA.Domain.Interfaces;

/// <summary>
/// Repository interface for role permission operations
/// </summary>
public interface IRolePermissionRepository
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
    
    /// <summary>
    /// Gets a role permission by its unique identifier
    /// </summary>
    /// <param name="id">The unique identifier</param>
    /// <returns>The role permission if found, otherwise null</returns>
    Task<RolePermission?> GetByIdAsync(Guid id);
    
    /// <summary>
    /// Gets all role permissions
    /// </summary>
    /// <returns>A collection of all role permissions</returns>
    Task<IReadOnlyList<RolePermission>> GetAllAsync();
    
    /// <summary>
    /// Finds role permissions based on a predicate
    /// </summary>
    /// <param name="predicate">The filter expression</param>
    /// <returns>A collection of matching role permissions</returns>
    Task<IReadOnlyList<RolePermission>> FindAsync(Expression<Func<RolePermission, bool>> predicate);
    
    /// <summary>
    /// Adds a new role permission
    /// </summary>
    /// <param name="entity">The role permission to add</param>
    /// <returns>The added role permission</returns>
    Task<RolePermission> AddAsync(RolePermission entity);
    
    /// <summary>
    /// Updates an existing role permission
    /// </summary>
    /// <param name="entity">The role permission to update</param>
    /// <returns>The updated role permission</returns>
    Task<RolePermission> UpdateAsync(RolePermission entity);
    
    /// <summary>
    /// Deletes a role permission
    /// </summary>
    /// <param name="entity">The role permission to delete</param>
    /// <returns>True if deletion was successful, otherwise false</returns>
    Task<bool> DeleteAsync(RolePermission entity);
    
    /// <summary>
    /// Deletes a role permission by its unique identifier
    /// </summary>
    /// <param name="id">The unique identifier</param>
    /// <returns>True if deletion was successful, otherwise false</returns>
    Task<bool> DeleteByIdAsync(Guid id);
    
    /// <summary>
    /// Checks if any role permission matches the specified predicate
    /// </summary>
    /// <param name="predicate">The filter expression</param>
    /// <returns>True if any role permission matches, otherwise false</returns>
    Task<bool> ExistsAsync(Expression<Func<RolePermission, bool>> predicate);
    
    /// <summary>
    /// Counts role permissions based on an optional predicate
    /// </summary>
    /// <param name="predicate">The optional filter expression</param>
    /// <returns>The count of matching role permissions</returns>
    Task<int> CountAsync(Expression<Func<RolePermission, bool>>? predicate = null);
}
