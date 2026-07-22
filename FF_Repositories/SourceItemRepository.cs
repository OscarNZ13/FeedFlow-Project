using FF_DataDB;
using FF_ModelsDB;
using FF_DataDB.Context;
using FF_ModelsDB.Models;
using Microsoft.EntityFrameworkCore;

namespace FF_Repositories;

public interface ISourceItemRepository : IRepositoryBase<SourceItem>
{
    Task<IEnumerable<SourceItem>> ReadBySourceAsync(int sourceId);
    Task<IEnumerable<SourceItem>> ReadLatestAsync(int take);
    Task<bool> AnyAsync();
    Task<bool> CreateManyAsync(IEnumerable<SourceItem> entities);
}

public class SourceItemRepository(FeedFlowDbContext context) : RepositoryBase<SourceItem>(context), ISourceItemRepository
{
    public async Task<IEnumerable<SourceItem>> ReadBySourceAsync(int sourceId)
    {
        return await Context.SourceItems
            .Where(i => i.SourceId == sourceId)
            .OrderByDescending(i => i.CreatedAt)
            .ToListAsync();
    }

    public async Task<IEnumerable<SourceItem>> ReadLatestAsync(int take)
    {
        return await Context.SourceItems
            .OrderByDescending(i => i.CreatedAt)
            .Take(take)
            .ToListAsync();
    }

    public async Task<bool> AnyAsync()
    {
        return await Context.SourceItems.AnyAsync();
    }

    public async Task<bool> CreateManyAsync(IEnumerable<SourceItem> entities)
    {
        await Context.SourceItems.AddRangeAsync(entities);
        return await SaveAsync();
    }
}

public interface ISourceItemRepository
{
    Task<bool> UpsertAsync(SourceItem entity, bool isUpdating);
    Task<bool> CreateAsync(SourceItem entity);
    Task<bool> DeleteAsync(SourceItem entity);
    Task<IEnumerable<SourceItem>> ReadAsync();
    Task<SourceItem> FindAsync(int id);
    Task<bool> UpdateAsync(SourceItem entity);

    Task<bool> UpdateManyAsync(IEnumerable<SourceItem> entities);
    Task<bool> ExistsAsync(SourceItem entity);
    Task<IEnumerable<SourceItem>> FindBySourceIdAsync(int sourceId);
    Task<SourceItem?> FindWithSourceAsync(int id);
}

public class SourceItemRepository(FF_DbContext context)
    : RepositoryBase<SourceItem>(context), ISourceItemRepository
{

    public async Task<IEnumerable<SourceItem>> FindBySourceIdAsync(int sourceId)
    {
        return await DbContext.SourceItems
            .Include(x => x.Source)
            .Where(x => x.SourceId == sourceId)
            .ToListAsync();
    }

    public async Task<SourceItem?> FindWithSourceAsync(int id)
    {
        return await DbContext.SourceItems
            .Include(x => x.Source)
            .FirstOrDefaultAsync(x => x.Id == id);
    }
}