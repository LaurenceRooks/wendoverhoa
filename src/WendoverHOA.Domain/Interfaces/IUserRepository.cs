using WendoverHOA.Domain.Entities;
using WendoverHOA.Domain.Enums;

namespace WendoverHOA.Domain.Interfaces;

/// <summary>
/// Repository interface for user-related operations
/// </summary>
public interface IUserRepository : IRepository<ApplicationUser>
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
}
