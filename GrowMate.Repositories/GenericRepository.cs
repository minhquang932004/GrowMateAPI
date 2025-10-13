using System.Linq.Expressions;
using GrowMate.Repositories.Data;
using GrowMate.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace GrowMate.Repositories;

/// <summary>
/// Generic repository implementation that provides basic CRUD operations
/// </summary>
/// <typeparam name="T">Entity type</typeparam>
public class GenericRepository<T> : IGenericRepository<T> where T : class
{
    protected readonly EXE201_GrowMateContext _context;
    protected readonly DbSet<T> _dbSet;

    public GenericRepository(EXE201_GrowMateContext context)
    {
        _context = context;
        _dbSet = context.Set<T>();
    }

    /// <summary>
    /// Gets all entities
    /// </summary>
    public async Task<IEnumerable<T>> GetAllAsync()
    {
        return await _dbSet.ToListAsync();
    }

    /// <summary>
    /// Gets entities based on a condition
    /// </summary>
    public async Task<IEnumerable<T>> FindAsync(Expression<Func<T, bool>> predicate)
    {
        return await _dbSet.Where(predicate).ToListAsync();
    }

    /// <summary>
    /// Gets a single entity by ID
    /// </summary>
    public virtual async Task<T> GetByIdAsync(int id)
    {
        // This assumes the entity has an Id property
        // For entities with different primary key names, this method should be overridden
        return await _dbSet.FindAsync(id);
    }

    /// <summary>
    /// Adds a new entity
    /// </summary>
    public async Task AddAsync(T entity)
    {
        await _dbSet.AddAsync(entity);
    }

    /// <summary>
    /// Updates an existing entity
    /// </summary>
    public void Update(T entity)
    {
        _dbSet.Attach(entity);
        _context.Entry(entity).State = EntityState.Modified;
    }

    /// <summary>
    /// Removes an entity
    /// </summary>
    public void Remove(T entity)
    {
        _dbSet.Remove(entity);
    }

    /// <summary>
    /// Checks if entities exist based on a condition
    /// </summary>
    public async Task<bool> ExistsAsync(Expression<Func<T, bool>> predicate)
    {
        return await _dbSet.AnyAsync(predicate);
    }
}