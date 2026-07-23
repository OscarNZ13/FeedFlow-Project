using FF_DataDB.Context;
using FF_ModelsDB.Models;
using Microsoft.EntityFrameworkCore;

namespace FF_Repositories;

public interface ISourceItemRepository : IRepositoryBase<SourceItem>
{
    Task<IEnumerable<SourceItem>> FindBySourceIdAsync(int sourceId);
    Task<SourceItem?> FindWithSourceAsync(int id);

    Task<IEnumerable<SourceItem>> ReadLatestAsync(int take);
    Task<bool> AnyAsync();
    Task<bool> CreateManyAsync(IEnumerable<SourceItem> entities);
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

    public async Task<IEnumerable<SourceItem>> ReadLatestAsync(int take)
    {
        return await DbContext.SourceItems
            .OrderByDescending(i => i.CreatedAt)
            .Take(take)
            .ToListAsync();
    }

    public async Task<bool> AnyAsync()
    {
        return await DbContext.SourceItems.AnyAsync();
    }

    public async Task<bool> CreateManyAsync(IEnumerable<SourceItem> entities)
    {
        await DbContext.SourceItems.AddRangeAsync(entities);
        return await SaveAsync();
    }

}
