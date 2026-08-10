using System.Security.Cryptography;
using System.Text;

namespace FF.Architecture.Parsers;

public static class StableIdGenerator
{
    public static string Generate(string sourceName, string? url, string? title, string? description)
    {
        var key = !string.IsNullOrWhiteSpace(url)
            ? $"{sourceName}|{url}"
            : $"{sourceName}|{title}|{description}";

        var bytes = Encoding.UTF8.GetBytes(key);
        var hash = SHA256.HashData(bytes);
        return Convert.ToHexString(hash)[..32].ToLowerInvariant();
    }
}
