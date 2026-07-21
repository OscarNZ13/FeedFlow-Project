using FF_DataDB;
using FF_ModelsDB;
using Microsoft.EntityFrameworkCore;

namespace FF_Repositories;

public interface ISourceRepository : IRepositoryBase<Source>
{
    Task<IEnumerable<Source>> ReadActiveAsync();
    Task<Source?> FindWithSecretsAsync(int id);
}

public class SourceRepository(FeedFlowDbContext context) : RepositoryBase<Source>(context), ISourceRepository
{
    public async Task<IEnumerable<Source>> ReadActiveAsync()
    {
        return await Context.Sources.Where(s => s.IsActive).ToListAsync();
    }

    public async Task<Source?> FindWithSecretsAsync(int id)
    {
        return await Context.Sources
            .Include(s => s.Secrets)
            .FirstOrDefaultAsync(s => s.Id == id);
    }
}
