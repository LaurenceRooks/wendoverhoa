using System.Linq.Expressions;
using WendoverHOA.Domain.Entities;
using WendoverHOA.Domain.Enums;

namespace WendoverHOA.Domain.Interfaces;

/// <summary>
/// Repository interface for user-related operations
/// </summary>
public interface IUserRepository
{
    /// <summary>
    /// Gets a user by their username
    /// </summary>
    /// <param name="username">The username to search for</param>
    /// <returns>The user if found, otherwise null</returns>
    Task<ApplicationUser?> GetByUsernameAsync(string username);
    
    /// <summary>
    /// Gets a user by their email address
    /// </summary>
    /// <param name="email">The email address to search for</param>
    /// <returns>The user if found, otherwise null</returns>
    Task<ApplicationUser?> GetByEmailAsync(string email);
    
    /// <summary>
    /// Gets all users with a specific role
    /// </summary>
    /// <param name="role">The role to filter by</param>
    /// <returns>A collection of users with the specified role</returns>
    Task<IReadOnlyList<ApplicationUser>> GetUsersByRoleAsync(UserRole role);
    
    /// <summary>
    /// Gets all users with a specific permission
    /// </summary>
    /// <param name="permission">The permission to filter by</param>
    /// <returns>A collection of users with the specified permission</returns>
    Task<IReadOnlyList<ApplicationUser>> GetUsersByPermissionAsync(Permission permission);
    
    /// <summary>
    /// Checks if a username is already taken
    /// </summary>
    /// <param name="username">The username to check</param>
    /// <returns>True if the username is taken, otherwise false</returns>
    Task<bool> IsUsernameTakenAsync(string username);
    
    /// <summary>
    /// Checks if an email address is already registered
    /// </summary>
    /// <param name="email">The email address to check</param>
    /// <returns>True if the email is registered, otherwise false</returns>
    Task<bool> IsEmailRegisteredAsync(string email);
    
    /// <summary>
    /// Gets an entity by its unique identifier
    /// </summary>
    /// <param name="id">The unique identifier</param>
    /// <returns>The entity if found, otherwise null</returns>
    Task<ApplicationUser?> GetByIdAsync(int id);
    
    /// <summary>
    /// Gets all entities
    /// </summary>
    /// <returns>A collection of all entities</returns>
    Task<IReadOnlyList<ApplicationUser>> GetAllAsync();
    
    /// <summary>
    /// Finds entities based on a predicate
    /// </summary>
    /// <param name="predicate">The filter expression</param>
    /// <returns>A collection of matching entities</returns>
    Task<IReadOnlyList<ApplicationUser>> FindAsync(Expression<Func<ApplicationUser, bool>> predicate);
    
    /// <summary>
    /// Adds a new entity
    /// </summary>
    /// <param name="entity">The entity to add</param>
    /// <returns>The added entity</returns>
    Task<ApplicationUser> AddAsync(ApplicationUser entity);
    
    /// <summary>
    /// Updates an existing entity
    /// </summary>
    /// <param name="entity">The entity to update</param>
    /// <returns>The updated entity</returns>
    Task<ApplicationUser> UpdateAsync(ApplicationUser entity);
    
    /// <summary>
    /// Deletes an entity
    /// </summary>
    /// <param name="entity">The entity to delete</param>
    /// <returns>True if deletion was successful, otherwise false</returns>
    Task<bool> DeleteAsync(ApplicationUser entity);
    
    /// <summary>
    /// Deletes an entity by its unique identifier
    /// </summary>
    /// <param name="id">The unique identifier</param>
    /// <returns>True if deletion was successful, otherwise false</returns>
    Task<bool> DeleteByIdAsync(int id);
    
    /// <summary>
    /// Checks if any entity matches the specified predicate
    /// </summary>
    /// <param name="predicate">The filter expression</param>
    /// <returns>True if any entity matches, otherwise false</returns>
    Task<bool> ExistsAsync(Expression<Func<ApplicationUser, bool>> predicate);
    
    /// <summary>
    /// Counts entities based on an optional predicate
    /// </summary>
    /// <param name="predicate">The optional filter expression</param>
    /// <returns>The count of matching entities</returns>
    Task<int> CountAsync(Expression<Func<ApplicationUser, bool>>? predicate = null);
}
