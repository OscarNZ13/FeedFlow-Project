using FF_ModelsDB.Models;
using FF_Repositories;
using System.Text.Json;

namespace FF_Business
{
    public interface ISourceItemBusiness
    {
        Task<bool> SaveJsonAsync(string json, int sourceId);
        Task<IEnumerable<SourceItem>> GetAllAsync();
        Task<SourceItem> GetByIdAsync(int id);
        Task<IEnumerable<SourceItem>> GetBySourceIdAsync(int sourceId);
        Task<SourceItem?> GetWithSourceAsync(int id);
    }
    public class SourceItemBusiness : ISourceItemBusiness
    {
        private readonly ISourceItemRepository _sourceItemRepository;

        public SourceItemBusiness(ISourceItemRepository sourceItemRepository)
        {
            _sourceItemRepository = sourceItemRepository;
        }

        public async Task<bool> SaveJsonAsync(string json, int sourceId)
        {
            try
            {
                using JsonDocument document = JsonDocument.Parse(json);
                SourceItem item = new SourceItem()
                {
                    SourceId = sourceId,
                    Json = json,
                    CreatedAt = DateTime.Now
                };
                return await _sourceItemRepository.CreateAsync(item);
            }
            catch (JsonException)
            {
                return false;
            }
        }

        //GET
        public async Task<IEnumerable<SourceItem>> GetAllAsync()
        {
            return await _sourceItemRepository
                .ReadAsync();
        }

        //GET by Id
        public async Task<SourceItem> GetByIdAsync(int id)
        {
            return await _sourceItemRepository
                .FindAsync(id);
        }

        //GET by SourceId
        public async Task<IEnumerable<SourceItem>> GetBySourceIdAsync(int sourceId)
        {
            return await _sourceItemRepository
                .FindBySourceIdAsync(sourceId);
        }

        //Get with Source
        public async Task<SourceItem?> GetWithSourceAsync(int id)
        {
            return await _sourceItemRepository
                .FindWithSourceAsync(id);
        }
    }
}