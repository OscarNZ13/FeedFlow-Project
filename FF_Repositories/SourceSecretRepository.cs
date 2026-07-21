using FF_DataDB;
using FF_ModelsDB;
using Microsoft.EntityFrameworkCore;

namespace FF_Repositories;

public interface ISourceSecretRepository : IRepositoryBase<SourceSecret>
{
    Task<IEnumerable<SourceSecret>> ReadBySourceAsync(int sourceId);
}

public class SourceSecretRepository(FeedFlowDbContext context) : RepositoryBase<SourceSecret>(context), ISourceSecretRepository
{
    public async Task<IEnumerable<SourceSecret>> ReadBySourceAsync(int sourceId)
    {
        return await Context.SourceSecrets
            .Where(s => s.SourceId == sourceId)
            .ToListAsync();
    }
}
