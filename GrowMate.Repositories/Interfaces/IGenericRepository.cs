using System.Linq.Expressions;

namespace GrowMate.Repositories.Interfaces;

/// <summary>
/// Generic repository interface that provides basic CRUD operations
/// </summary>
/// <typeparam name="T">Entity type</typeparam>
public interface IGenericRepository<T> where T : class
{
    /// <summary>
    /// Gets all entities
    /// </summary>
    Task<IEnumerable<T>> GetAllAsync();
    
    /// <summary>
    /// Gets entities based on a condition
    /// </summary>
    Task<IEnumerable<T>> FindAsync(Expression<Func<T, bool>> predicate);
    
    /// <summary>
    /// Gets a single entity by ID
    /// </summary>
    Task<T> GetByIdAsync(int id);
    
    /// <summary>
    /// Adds a new entity
    /// </summary>
    Task AddAsync(T entity);
    
    /// <summary>
    /// Updates an existing entity
    /// </summary>
    void Update(T entity);
    
    /// <summary>
    /// Removes an entity
    /// </summary>
    void Remove(T entity);
    
    /// <summary>
    /// Checks if entities exist based on a condition
    /// </summary>
    Task<bool> ExistsAsync(Expression<Func<T, bool>> predicate);
}