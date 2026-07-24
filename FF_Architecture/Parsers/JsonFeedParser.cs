using System.Text.Json;

namespace FF.Architecture.Parsers;

public class JsonFeedParser : IFeedParser
{
    public FeedFormat Format => FeedFormat.Json;

    private static readonly string[] WrapperKeys =
        { "articles", "items", "data", "results", "entries", "news", "posts" };

    private static readonly string[] IdKeys = { "id", "uuid", "guid", "_id", "articleId" };
    private static readonly string[] TitleKeys = { "title", "name", "headline" };
    private static readonly string[] DescriptionKeys = { "description", "summary", "content", "excerpt", "body" };
    private static readonly string[] CategoryKeys = { "category", "section", "topic", "genre" };
    private static readonly string[] ImageKeys = { "image_url", "imageUrl", "urlToImage", "image", "thumbnail", "media" };
    private static readonly string[] UrlKeys = { "url", "link", "source_url" };
    private static readonly string[] TagsKeys = { "tags", "keywords", "categories" };
    private static readonly string[] DateKeys = { "published_at", "publishedAt", "pubDate", "date", "createdAt" };

    public IEnumerable<NewsItemDto> Parse(string rawContent, string sourceName)
    {
        using var document = JsonDocument.Parse(rawContent);
        var itemsElement = FindItemsArray(document.RootElement);

        if (itemsElement is null)
        {
            return new[] { MapElement(document.RootElement, sourceName) };
        }

        var results = new List<NewsItemDto>();
        foreach (var element in itemsElement.Value.EnumerateArray())
        {
            if (element.ValueKind == JsonValueKind.Object)
                results.Add(MapElement(element, sourceName));
        }
        return results;
    }

    private static JsonElement? FindItemsArray(JsonElement root)
    {
        if (root.ValueKind == JsonValueKind.Array)
            return root;

        if (root.ValueKind != JsonValueKind.Object)
            return null;

        foreach (var key in WrapperKeys)
        {
            if (root.TryGetProperty(key, out var value) && value.ValueKind == JsonValueKind.Array)
                return value;
        }

        foreach (var property in root.EnumerateObject())
        {
            if (property.Value.ValueKind == JsonValueKind.Array)
                return property.Value;
        }

        return null;
    }

    private static NewsItemDto MapElement(JsonElement element, string sourceName)
    {
        var dto = new NewsItemDto
        {
            SourceName = sourceName,
            Id = GetString(element, IdKeys) ?? Guid.NewGuid().ToString("N"),
            Title = GetString(element, TitleKeys) ?? "(sin título)",
            Description = GetString(element, DescriptionKeys),
            Category = GetString(element, CategoryKeys),
            ImageUrl = GetImageValue(element),
            Url = GetString(element, UrlKeys),
            Tags = GetTags(element),
            PublishedAt = GetDate(element)
        };
        return dto;
    }

    private static string? GetString(JsonElement element, string[] candidateKeys)
    {
        foreach (var key in candidateKeys)
        {
            if (TryGetPropertyIgnoreCase(element, key, out var value) && value.ValueKind == JsonValueKind.String)
                return value.GetString();
        }
        return null;
    }

    private static string? GetImageValue(JsonElement element)
    {
        foreach (var key in ImageKeys)
        {
            if (!TryGetPropertyIgnoreCase(element, key, out var value)) continue;

            if (value.ValueKind == JsonValueKind.String)
                return value.GetString();

            if (value.ValueKind == JsonValueKind.Object && TryGetPropertyIgnoreCase(value, "url", out var nested)
                && nested.ValueKind == JsonValueKind.String)
                return nested.GetString();
        }
        return null;
    }

    private static List<string> GetTags(JsonElement element)
    {
        foreach (var key in TagsKeys)
        {
            if (TryGetPropertyIgnoreCase(element, key, out var value) && value.ValueKind == JsonValueKind.Array)
            {
                return value.EnumerateArray()
                    .Where(x => x.ValueKind == JsonValueKind.String)
                    .Select(x => x.GetString()!)
                    .ToList();
            }
        }
        return new List<string>();
    }

    private static DateTime? GetDate(JsonElement element)
    {
        foreach (var key in DateKeys)
        {
            if (TryGetPropertyIgnoreCase(element, key, out var value) && value.ValueKind == JsonValueKind.String
                && DateTime.TryParse(value.GetString(), out var parsed))
                return parsed;
        }
        return null;
    }

    private static bool TryGetPropertyIgnoreCase(JsonElement element, string propertyName, out JsonElement value)
    {
        if (element.ValueKind == JsonValueKind.Object)
        {
            foreach (var property in element.EnumerateObject())
            {
                if (string.Equals(property.Name, propertyName, StringComparison.OrdinalIgnoreCase))
                {
                    value = property.Value;
                    return true;
                }
            }
        }
        value = default;
        return false;
    }
}
