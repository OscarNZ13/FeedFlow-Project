using FF.Architecture.Parsers;
using FF.Architecture.Providers;
using FF_Mvc.ViewModels;

namespace FF_Mvc.Service;

public interface IFeedService
{
    Task<IEnumerable<NewsItemDto>> GetFeedAsync(int take = 10);
    Task<IEnumerable<NewsItemDto>> PreviewSourceAsync(int sourceId, int take = 10);
    Task<IEnumerable<NewsItemDto>> RefreshSourceAsync(int sourceId);
    Task<ImportResult> ImportItemAsync(string json);
}

public class FeedService(IRestProvider restProvider) : IFeedService
{
    private const string ApiBaseUrl = "https://localhost:7283/FeedApi";

    public async Task<IEnumerable<NewsItemDto>> GetFeedAsync(int take = 10)
    {
        var content = await restProvider.GetAsync($"{ApiBaseUrl}/feed?take={take}", null);
        return JsonProvider.DeserializeSimple<IEnumerable<NewsItemDto>>(content) ?? [];
    }

    public async Task<IEnumerable<NewsItemDto>> PreviewSourceAsync(int sourceId, int take = 10)
    {
        var content = await restProvider.GetAsync($"{ApiBaseUrl}/sources/{sourceId}/preview?take={take}", null);
        return JsonProvider.DeserializeSimple<IEnumerable<NewsItemDto>>(content) ?? [];
    }

    public async Task<IEnumerable<NewsItemDto>> RefreshSourceAsync(int sourceId)
    {
        var content = await restProvider.PostAsync($"{ApiBaseUrl}/sources/{sourceId}/refresh", string.Empty);
        return JsonProvider.DeserializeSimple<IEnumerable<NewsItemDto>>(content) ?? [];
    }
    public async Task<ImportResult> ImportItemAsync(string json)
    {
        try
        {
            await restProvider.PostAsync(
                "https://localhost:7283/ImportExportApi/import/item",
                json);

            return new ImportResult
            {
                Success = true,
                Message = "La noticia fue importada correctamente."
            };
        }
        catch (Exception ex)
        {
            return new ImportResult
            {
                Success = false,
                Message = ex.Message
            };
        }
    }
}
