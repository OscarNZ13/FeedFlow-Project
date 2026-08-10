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

    Task<IEnumerable<SourceListItemViewModel>> GetSourcesAsync();
    Task<SourceListItemViewModel?> CreateSourceAsync(SourceFormViewModel form);
    Task<(bool Success, string? Error)> DeleteSourceAsync(int sourceId);
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

    public async Task<IEnumerable<SourceListItemViewModel>> GetSourcesAsync()
    {
        var content = await restProvider.GetAsync($"{ApiBaseUrl}/sources", null);
        return JsonProvider.DeserializeSimple<IEnumerable<SourceListItemViewModel>>(content) ?? [];
    }

    public async Task<SourceListItemViewModel?> CreateSourceAsync(SourceFormViewModel form)
    {
        var payload = JsonProvider.Serialize(form);
        var content = await restProvider.PostAsync($"{ApiBaseUrl}/sources", payload);
        return JsonProvider.DeserializeSimple<SourceListItemViewModel>(content);
    }

    public async Task<(bool Success, string? Error)> DeleteSourceAsync(int sourceId)
    {
        try
        {
            await restProvider.DeleteAsync($"{ApiBaseUrl}/sources/{sourceId}", "");
            return (true, null);
        }
        catch (Exception ex)
        {
            return (false, ex.Message);
        }
    }
}