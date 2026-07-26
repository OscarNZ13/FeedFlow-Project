using FF.Architecture.Parsers;
using FF.Architecture.Providers;
using FF_ModelsDB.Models;
using FF_Repositories;
using System.Text.Json;

namespace FF_Api.Business;

public interface IFeedBusiness
{
    Task<IEnumerable<NewsItemDto>> PreviewSourceAsync(int sourceId, int take = 10);

    Task<IEnumerable<NewsItemDto>> RefreshSourceAsync(int sourceId);

    Task<IEnumerable<NewsItemDto>> GetFeedAsync(int take = 10);
}

public class FeedBusiness(
    ISourceRepository sourceRepository,
    ISourceItemRepository sourceItemRepository,
    IFeedFetcher feedFetcher,
    IFeedParserFactory parserFactory) : IFeedBusiness
{
    private static readonly JsonSerializerOptions JsonOptions = new() { PropertyNameCaseInsensitive = true };

    public async Task<IEnumerable<NewsItemDto>> PreviewSourceAsync(int sourceId, int take = 10)
    {
        var source = await sourceRepository.FindWithSecretsAsync(sourceId)
            ?? throw new KeyNotFoundException($"Source {sourceId} no existe");

        var items = await FetchAndParseAsync(source);
        return items.Take(take);
    }

    public async Task<IEnumerable<NewsItemDto>> RefreshSourceAsync(int sourceId)
    {
        var source = await sourceRepository.FindWithSecretsAsync(sourceId)
            ?? throw new KeyNotFoundException($"Source {sourceId} no existe");

        var items = (await FetchAndParseAsync(source)).ToList();

        var existingIds = (await sourceItemRepository.FindBySourceIdAsync(sourceId))
            .Select(x => TryGetId(x.Json))
            .Where(id => id != null)
            .ToHashSet();

        var newEntities = items
            .Where(item => !existingIds.Contains(item.Id))
            .Select(item => new SourceItem
            {
                SourceId = source.Id,
                Json = JsonSerializer.Serialize(item, JsonOptions),
                CreatedAt = DateTime.UtcNow
            })
            .ToList();

        if (newEntities.Count > 0)
            await sourceItemRepository.CreateManyAsync(newEntities);

        source.LastFetchedAt = DateTime.UtcNow;
        await sourceRepository.UpdateAsync(source);

        return items;
    }

    public async Task<IEnumerable<NewsItemDto>> GetFeedAsync(int take = 10)
    {
        if (await sourceItemRepository.AnyAsync())
        {
            var saved = await sourceItemRepository.ReadLatestAsync(take);
            return saved
            .Where(x => !string.IsNullOrWhiteSpace(x.Json))
            .Select(x =>
            {
                var dto = JsonSerializer.Deserialize<NewsItemDto>(x.Json!, JsonOptions);

                if (dto != null)
                {
                    dto.SourceItemId = x.Id;
                }

                return dto;
            })

        .Where(x => x != null)
        .Select(x => x!)
        .ToList();
        }

        var sources = await sourceRepository.ReadActiveAsync();

        foreach (var source in sources)
        {
            try
            {
                await RefreshSourceAsync(source.Id);
            }
            catch
            {
                // Si una fuente falla, las demás siguen funcionando.
            }
        }

        var savedAfterRefresh = await sourceItemRepository.ReadLatestAsync(take);
        var results = new List<NewsItemDto>();

        return savedAfterRefresh
            .Where(x => !string.IsNullOrWhiteSpace(x.Json))
            .Select(x =>
            {
                var dto = JsonSerializer.Deserialize<NewsItemDto>(x.Json!, JsonOptions);

                if (dto is not null)
                    dto.SourceItemId = x.Id;

                return dto;
            })
            .Where(x => x is not null)
            .Select(x => x!)
            .ToList();

        return results.Take(take);
    }

    private async Task<IEnumerable<NewsItemDto>> FetchAndParseAsync(Source source)
    {
        var (headers, queryParams) = BuildSecrets(source);

        var fetchResult = await feedFetcher.FetchAsync(source.Url, headers, queryParams);
        var format = FeedFormatDetector.Detect(fetchResult.ContentType, fetchResult.Content);
        var parser = parserFactory.GetParser(format);

        return parser.Parse(fetchResult.Content, source.Name);
    }

    private static (Dictionary<string, string> headers, Dictionary<string, string> queryParams) BuildSecrets(Source source)
    {
        var headers = new Dictionary<string, string>();
        var queryParams = new Dictionary<string, string>();

        if (!source.RequiresSecret) return (headers, queryParams);

        foreach (var secret in source.Secrets)
        {
            if (secret.Location == SecretLocation.Header)
                headers[secret.KeyName] = secret.KeyValue;
            else
                queryParams[secret.KeyName] = secret.KeyValue;
        }

        return (headers, queryParams);
    }

    private static string? TryGetId(string? json)
    {
        if (string.IsNullOrWhiteSpace(json)) return null;

        try
        {
            using var doc = JsonDocument.Parse(json);
            return doc.RootElement.TryGetProperty("id", out var idProp) ? idProp.GetString() : null;
        }
        catch
        {
            return null;
        }
    }
}
