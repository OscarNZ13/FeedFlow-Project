using System.Xml.Linq;

namespace FF.Architecture.Parsers;
public class XmlFeedParser : IFeedParser
{
    public FeedFormat Format => FeedFormat.Xml;

    public IEnumerable<NewsItemDto> Parse(string rawContent, string sourceName)
    {
        var document = XDocument.Parse(rawContent);
        var root = document.Root;
        if (root is null) return Enumerable.Empty<NewsItemDto>();

        var rssItems = root.Descendants().Where(e => e.Name.LocalName == "item").ToList();
        if (rssItems.Count > 0)
            return rssItems.Select(item => MapElement(item, sourceName));

        var atomItems = root.Descendants().Where(e => e.Name.LocalName == "entry").ToList();
        if (atomItems.Count > 0)
            return atomItems.Select(entry => MapElement(entry, sourceName));

        var grouped = root.Descendants()
            .GroupBy(e => e.Name.LocalName)
            .Where(g => g.Count() > 1)
            .OrderByDescending(g => g.Count())
            .FirstOrDefault();

        if (grouped != null)
            return grouped.Select(item => MapElement(item, sourceName));

        return new[] { MapElement(root, sourceName) };
    }

    private static NewsItemDto MapElement(XElement element, string sourceName)
    {
        return new NewsItemDto
        {
            SourceName = sourceName,
            Id = GetValue(element, "id", "guid") ?? Guid.NewGuid().ToString("N"),
            Title = GetValue(element, "title", "name") ?? "(sin título)",
            Description = GetValue(element, "description", "summary", "content"),
            Category = GetValue(element, "category", "section"),
            ImageUrl = GetImageValue(element),
            Url = GetValue(element, "link", "url"),
            Tags = GetTags(element),
            PublishedAt = GetDate(element)
        };
    }

    private static string? GetValue(XElement element, params string[] candidateNames)
    {
        foreach (var name in candidateNames)
        {
            var found = element.Elements().FirstOrDefault(e =>
                string.Equals(e.Name.LocalName, name, StringComparison.OrdinalIgnoreCase));
            if (found != null && !string.IsNullOrWhiteSpace(found.Value))
                return found.Value.Trim();
        }

        if (candidateNames.Contains("link", StringComparer.OrdinalIgnoreCase))
        {
            var linkEl = element.Elements().FirstOrDefault(e =>
                string.Equals(e.Name.LocalName, "link", StringComparison.OrdinalIgnoreCase));
            var href = linkEl?.Attribute("href")?.Value;
            if (!string.IsNullOrWhiteSpace(href)) return href;
        }

        return null;
    }

    private static string? GetImageValue(XElement element)
    {
        var enclosure = element.Elements().FirstOrDefault(e =>
            string.Equals(e.Name.LocalName, "enclosure", StringComparison.OrdinalIgnoreCase));
        var enclosureUrl = enclosure?.Attribute("url")?.Value;
        if (!string.IsNullOrWhiteSpace(enclosureUrl)) return enclosureUrl;

        var mediaContent = element.Elements().FirstOrDefault(e => e.Name.LocalName == "content");
        var mediaUrl = mediaContent?.Attribute("url")?.Value;
        if (!string.IsNullOrWhiteSpace(mediaUrl)) return mediaUrl;

        return GetValue(element, "image", "thumbnail");
    }

    private static List<string> GetTags(XElement element)
    {
        return element.Elements()
            .Where(e => string.Equals(e.Name.LocalName, "category", StringComparison.OrdinalIgnoreCase))
            .Select(e => e.Value.Trim())
            .Where(v => !string.IsNullOrWhiteSpace(v))
            .ToList();
    }

    private static DateTime? GetDate(XElement element)
    {
        var raw = GetValue(element, "pubDate", "published", "updated", "date");
        return raw != null && DateTime.TryParse(raw, out var parsed) ? parsed : null;
    }
}
