using FF_DataDB.Context;
using FF_ModelsDB.Models;
using Microsoft.EntityFrameworkCore;

namespace FF_Repositories;

public interface ISourceSecretRepository : IRepositoryBase<SourceSecret>
{
    Task<IEnumerable<SourceSecret>> ReadBySourceAsync(int sourceId);
}

public class SourceSecretRepository(FF_DbContext context)
    : RepositoryBase<SourceSecret>(context), ISourceSecretRepository
{
    public async Task<IEnumerable<SourceSecret>> ReadBySourceAsync(int sourceId)
    {
        return await DbContext.SourceSecrets
            .Where(s => s.SourceId == sourceId)
            .ToListAsync();
    }
}
