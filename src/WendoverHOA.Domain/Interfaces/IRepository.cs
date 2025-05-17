using System.Linq.Expressions;
using WendoverHOA.Domain.Common;

namespace WendoverHOA.Domain.Interfaces;

/// <summary>
/// Generic repository interface for entity operations
/// </summary>
/// <typeparam name="T">Entity type that inherits from BaseEntity</typeparam>
public interface IRepository<T> where T : BaseEntity
{
    /// <summary>
    /// Gets an entity by its unique identifier
    /// </summary>
    /// <param name="id">The unique identifier</param>
    /// <returns>The entity if found, otherwise null</returns>
    Task<T?> GetByIdAsync(Guid id);
    
    /// <summary>
    /// Gets all entities of type T
    /// </summary>
    /// <returns>A collection of all entities</returns>
    Task<IReadOnlyList<T>> GetAllAsync();
    
    /// <summary>
    /// Finds entities based on a predicate
    /// </summary>
    /// <param name="predicate">The filter expression</param>
    /// <returns>A collection of matching entities</returns>
    Task<IReadOnlyList<T>> FindAsync(Expression<Func<T, bool>> predicate);
    
    /// <summary>
    /// Adds a new entity
    /// </summary>
    /// <param name="entity">The entity to add</param>
    /// <returns>The added entity</returns>
    Task<T> AddAsync(T entity);
    
    /// <summary>
    /// Updates an existing entity
    /// </summary>
    /// <param name="entity">The entity to update</param>
    /// <returns>The updated entity</returns>
    Task<T> UpdateAsync(T entity);
    
    /// <summary>
    /// Deletes an entity
    /// </summary>
    /// <param name="entity">The entity to delete</param>
    /// <returns>True if deletion was successful, otherwise false</returns>
    Task<bool> DeleteAsync(T entity);
    
    /// <summary>
    /// Deletes an entity by its unique identifier
    /// </summary>
    /// <param name="id">The unique identifier of the entity to delete</param>
    /// <returns>True if deletion was successful, otherwise false</returns>
    Task<bool> DeleteByIdAsync(Guid id);
    
    /// <summary>
    /// Checks if any entity matches the given predicate
    /// </summary>
    /// <param name="predicate">The predicate to check</param>
    /// <returns>True if any entity matches, otherwise false</returns>
    Task<bool> ExistsAsync(Expression<Func<T, bool>> predicate);
    
    /// <summary>
    /// Gets the count of entities matching the given predicate
    /// </summary>
    /// <param name="predicate">The predicate to filter entities</param>
    /// <returns>The count of matching entities</returns>
    Task<int> CountAsync(Expression<Func<T, bool>>? predicate = null);
}
