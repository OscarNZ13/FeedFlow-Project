namespace FF.Architecture.Parsers;

public enum FeedFormat
{
    Json,
    Xml,
    Text
}

public static class FeedFormatDetector
{
    public static FeedFormat Detect(string? contentType, string content)
    {
        if (!string.IsNullOrWhiteSpace(contentType))
        {
            var ct = contentType.ToLowerInvariant();
            if (ct.Contains("json"))
                return FeedFormat.Json;
            if (ct.Contains("xml") || ct.Contains("rss") || ct.Contains("atom"))
                return FeedFormat.Xml;
            if (ct.Contains("text/plain"))
                return FeedFormat.Text;
        }

        return SniffContent(content);
    }

    private static FeedFormat SniffContent(string content)
    {
        var trimmed = content.TrimStart();

        if (trimmed.Length == 0)
            return FeedFormat.Text;

        return trimmed[0] switch
        {
            '{' or '[' => FeedFormat.Json,
            '<' => FeedFormat.Xml,
            _ => FeedFormat.Text
        };
    }
}
