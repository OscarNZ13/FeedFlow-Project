using FF_DataDB.Context;
using Microsoft.EntityFrameworkCore;

namespace FF_Repositories;

/// <summary>
/// Interface for basic repository operations.
/// </summary>
/// <typeparam name="T">The type of entity.</typeparam>
public interface IRepositoryBase<T>
{
    Task<bool> UpsertAsync(T entity, bool isUpdating);
    Task<bool> CreateAsync(T entity);
    Task<bool> DeleteAsync(T entity);
    Task<IEnumerable<T>> ReadAsync();
    Task<T> FindAsync(int id);
    Task<bool> UpdateAsync(T entity);
    Task<bool> UpdateManyAsync(IEnumerable<T> entities);
    Task<bool> ExistsAsync(T entity);
}

/// <summary>
/// Base class for repository operations.
/// </summary>
/// <typeparam name="T">Entity type.</typeparam>
public abstract class RepositoryBase<T> : IRepositoryBase<T> where T : class
{
    private readonly FF_DbContext _context;
    protected FF_DbContext DbContext => _context;
    protected DbSet<T> DbSet;

    public RepositoryBase(FF_DbContext context)
    {
        _context = context;
        DbSet = _context.Set<T>();
    }

    public async Task<bool> UpsertAsync(T entity, bool isUpdating)
    {
        return isUpdating
            ? await UpdateAsync(entity)
            : await CreateAsync(entity);
    }

    public async Task<bool> CreateAsync(T entity)
    {
        await _context.AddAsync(entity);
        return await SaveAsync();
    }

    public async Task<bool> UpdateAsync(T entity)
    {
        _context.Update(entity);
        return await SaveAsync();
    }

    public async Task<bool> UpdateManyAsync(IEnumerable<T> entities)
    {
        _context.UpdateRange(entities);
        return await SaveAsync();
    }

    public async Task<bool> DeleteAsync(T entity)
    {
        _context.Remove(entity);
        return await SaveAsync();
    }

    public async Task<IEnumerable<T>> ReadAsync()
    {
        return await _context.Set<T>().ToListAsync();
    }

    public async Task<T> FindAsync(int id)
    {
        return await _context.Set<T>().FindAsync(id);
    }

    public async Task<bool> ExistsAsync(T entity)
    {
        var items = await ReadAsync();
        return items.Any(x => x.Equals(entity));
    }

    protected async Task<bool> SaveAsync()
    {
        var result = await _context.SaveChangesAsync();
        return result > 0;
    }
}
