using FF_DataDB.Context;
using FF_ModelsDB.Models;
using Microsoft.EntityFrameworkCore;

namespace FF_Repositories;

public interface ISourceRepository : IRepositoryBase<Source>
{
    Task<Source?> FindByUrlAsync(string url);

    Task<IEnumerable<Source>> ReadActiveAsync();
    Task<Source?> FindWithSecretsAsync(int id);
}

public class SourceRepository(FF_DbContext context)
    : RepositoryBase<Source>(context), ISourceRepository
{
    public async Task<Source?> FindByUrlAsync(string url)
    {
        return await DbContext.Sources
            .FirstOrDefaultAsync(x => x.Url == url);
    }

    public async Task<IEnumerable<Source>> ReadActiveAsync()
    {
        return await DbContext.Sources.Where(s => s.IsActive).ToListAsync();
    }

    public async Task<Source?> FindWithSecretsAsync(int id)
    {
        return await DbContext.Sources
            .Include(s => s.Secrets)
            .FirstOrDefaultAsync(s => s.Id == id);
    }
}
