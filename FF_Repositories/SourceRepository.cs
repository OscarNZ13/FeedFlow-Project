using FF_DataDB.Context;
using FF_ModelsDB.Models;
using Microsoft.EntityFrameworkCore;


namespace FF_Repositories;
public interface ISourceRepository
{
    Task<bool> UpsertAsync(Source entity, bool isUpdating);
    Task<bool> CreateAsync(Source entity);
    Task<bool> DeleteAsync(Source entity);
    Task<IEnumerable<Source>> ReadAsync();
    Task<Source> FindAsync(int id);
    Task<bool> UpdateAsync(Source entity);
    Task<bool> UpdateManyAsync(IEnumerable<Source> entities);
    Task<bool> ExistsAsync(Source entity);
    Task<Source?> FindByUrlAsync(string url);
}

public class SourceRepository(FF_DbContext context)
    : RepositoryBase<Source>(context), ISourceRepository
{
    public async Task<Source?> FindByUrlAsync(string url)
    {
        return await DbContext.Sources
            .FirstOrDefaultAsync(x => x.Url == url);
    }
}