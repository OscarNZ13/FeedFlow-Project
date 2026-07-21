using FF_DataDB;
using FF_ModelsDB;
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
