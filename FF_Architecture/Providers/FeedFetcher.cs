namespace FF.Architecture.Providers;

public record FeedFetchResult(string Content, string? ContentType);

/// <see cref="RestProvider"/> 
public interface IFeedFetcher
{
    Task<FeedFetchResult> FetchAsync(
        string url,
        IDictionary<string, string>? headers = null,
        IDictionary<string, string>? queryParams = null,
        CancellationToken cancellationToken = default);
}

public class FeedFetcher : IFeedFetcher
{
    private readonly HttpClient _httpClient;

    public FeedFetcher(HttpClient? httpClient = null)
    {
        _httpClient = httpClient ?? new HttpClient();
    }

    public async Task<FeedFetchResult> FetchAsync(
        string url,
        IDictionary<string, string>? headers = null,
        IDictionary<string, string>? queryParams = null,
        CancellationToken cancellationToken = default)
    {
        var finalUrl = BuildUrl(url, queryParams);

        using var request = new HttpRequestMessage(HttpMethod.Get, finalUrl);
        request.Headers.Accept.ParseAdd("*/*");
        request.Headers.TryAddWithoutValidation("User-Agent", "FeedFlow/1.0 (proyecto)");

        if (headers != null)
        {
            foreach (var (key, value) in headers)
                request.Headers.TryAddWithoutValidation(key, value);
        }

        try
        {
            using var response = await _httpClient.SendAsync(request, cancellationToken);
            var content = await response.Content.ReadAsStringAsync(cancellationToken);
            var contentType = response.Content.Headers.ContentType?.MediaType;

            if (!response.IsSuccessStatusCode)
            {
                var snippet = content.Length > 300 ? content[..300] : content;
                throw new ApplicationException(
                    $"La fuente {url} respondió {(int)response.StatusCode} {response.ReasonPhrase}. Detalle: {snippet}");
            }

            return new FeedFetchResult(content, contentType);
        }
        catch (ApplicationException)
        {
            throw; // ya trae el mensaje bueno, no lo envolvemos de nuevo
        }
        catch (Exception ex)
        {
            throw new ApplicationException($"Error obteniendo datos de la fuente {url}: {ex.Message}", ex);
        }
    }

    private static string BuildUrl(string url, IDictionary<string, string>? queryParams)
    {
        if (queryParams is null || queryParams.Count == 0)
            return url;

        var separator = url.Contains('?') ? "&" : "?";
        var query = string.Join("&", queryParams.Select(kv =>
            $"{Uri.EscapeDataString(kv.Key)}={Uri.EscapeDataString(kv.Value)}"));

        return $"{url}{separator}{query}";
    }
}