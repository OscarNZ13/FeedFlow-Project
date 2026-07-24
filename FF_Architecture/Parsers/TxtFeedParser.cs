namespace FF.Architecture.Parsers;

public class TxtFeedParser : IFeedParser
{
    public FeedFormat Format => FeedFormat.Text;

    public IEnumerable<NewsItemDto> Parse(string rawContent, string sourceName)
    {
        var blocks = rawContent
            .Replace("\r\n", "\n")
            .Split("\n\n", StringSplitOptions.RemoveEmptyEntries)
            .Select(b => b.Trim())
            .Where(b => !string.IsNullOrWhiteSpace(b))
            .ToList();

        if (blocks.Count == 0)
            return Enumerable.Empty<NewsItemDto>();

        return blocks.Select(block => MapBlock(block, sourceName));
    }

    private static NewsItemDto MapBlock(string block, string sourceName)
    {
        var lines = block.Split('\n', StringSplitOptions.RemoveEmptyEntries);
        var title = lines.Length > 0 ? lines[0].Trim() : "(sin título)";
        var description = lines.Length > 1
            ? string.Join(" ", lines.Skip(1)).Trim()
            : null;

        return new NewsItemDto
        {
            SourceName = sourceName,
            Id = Guid.NewGuid().ToString("N"),
            Title = title,
            Description = description
        };
    }
}
