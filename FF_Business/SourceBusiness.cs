using FF_ModelsDB.Models;
using FF_Repositories;

namespace FF_Business
{
    public interface ISourceBusiness
    {
        Task<bool> CreateAsync(Source source);
        Task<IEnumerable<Source>> GetAllAsync();
        Task<Source> GetByIdAsync(int id);
        Task<Source?> GetByUrlAsync(string url);
    }

    public class SourceBusiness : ISourceBusiness
    {
        private readonly ISourceRepository _sourceRepository;

        public SourceBusiness(ISourceRepository sourceRepository)
        {
            _sourceRepository = sourceRepository;
        }

        public async Task<bool> CreateAsync(Source source)
        {
            return await _sourceRepository
                .CreateAsync(source);
        }

        public async Task<IEnumerable<Source>> GetAllAsync()
        {
            return await _sourceRepository
                .ReadAsync();
        }

        public async Task<Source> GetByIdAsync(int id)
        {
            return await _sourceRepository
                .FindAsync(id);
        }

        public async Task<Source?> GetByUrlAsync(string url)
        {
            return await _sourceRepository
                .FindByUrlAsync(url);
        }
    }
}