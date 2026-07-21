using FF_DataDB;
using Microsoft.EntityFrameworkCore;

namespace FF_Repositories;

public interface IRepositoryBase<T>
{
    Task<bool> CreateAsync(T entity);
    Task<bool> UpdateAsync(T entity);
    Task<bool> DeleteAsync(T entity);
    Task<IEnumerable<T>> ReadAsync();
    Task<T?> FindAsync(int id);
}

public abstract class RepositoryBase<T> : IRepositoryBase<T> where T : class
{
    protected readonly FeedFlowDbContext Context;
    protected readonly DbSet<T> DbSet;

    protected RepositoryBase(FeedFlowDbContext context)
    {
        Context = context;
        DbSet = context.Set<T>();
    }

    public async Task<bool> CreateAsync(T entity)
    {
        await DbSet.AddAsync(entity);
        return await SaveAsync();
    }

    public async Task<bool> UpdateAsync(T entity)
    {
        Context.Update(entity);
        return await SaveAsync();
    }

    public async Task<bool> DeleteAsync(T entity)
    {
        Context.Remove(entity);
        return await SaveAsync();
    }

    public async Task<IEnumerable<T>> ReadAsync()
    {
        return await DbSet.ToListAsync();
    }

    public async Task<T?> FindAsync(int id)
    {
        return await DbSet.FindAsync(id);
    }

    protected async Task<bool> SaveAsync()
    {
        return await Context.SaveChangesAsync() > 0;
    }
}
