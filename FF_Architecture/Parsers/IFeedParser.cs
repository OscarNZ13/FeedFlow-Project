namespace FF.Architecture.Parsers;


/// <see cref="NewsItemDto"/>.

public interface IFeedParser
{
    FeedFormat Format { get; }

    /// <param name="rawContent">
    /// <param name="sourceName">
    IEnumerable<NewsItemDto> Parse(string rawContent, string sourceName);
}

/// <see cref="IFeedParser"/>
public interface IFeedParserFactory
{
    IFeedParser GetParser(FeedFormat format);
}

public class FeedParserFactory : IFeedParserFactory
{
    private readonly Dictionary<FeedFormat, IFeedParser> _parsers;

    public FeedParserFactory()
    {
        _parsers = new Dictionary<FeedFormat, IFeedParser>
        {
            [FeedFormat.Json] = new JsonFeedParser(),
            [FeedFormat.Xml] = new XmlFeedParser(),
            [FeedFormat.Text] = new TxtFeedParser()
        };
    }

    public IFeedParser GetParser(FeedFormat format)
    {
        if (_parsers.TryGetValue(format, out var parser))
            return parser;

        throw new NotSupportedException($"No hay parser registrado para el formato {format}");
    }
}
