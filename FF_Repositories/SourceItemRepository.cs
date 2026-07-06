using FF_DataDB.Context;
using FF_ModelsDB.Models;
using Microsoft.EntityFrameworkCore;

namespace FF_Repositories;


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