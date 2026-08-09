using System.Security.Cryptography;
using System.Text;

namespace FF.Architecture.Parsers;

/// <summary>
/// Genera un identificador determinístico para noticias que no traen un id/guid propio
/// en la fuente original. Antes se usaba Guid.NewGuid(), lo que generaba un id distinto
/// en cada refresh y hacía que la misma noticia se insertara una y otra vez.
/// Con un hash estable basado en el contenido (url/título/descripción), la misma noticia
/// produce siempre el mismo id entre refrescos, permitiendo detectar duplicados.
/// </summary>
public static class StableIdGenerator
{
    public static string Generate(string sourceName, string? url, string? title, string? description)
    {
        // Preferimos la URL como base porque suele ser el dato más estable y único.
        // Si no hay URL, caemos a título + descripción.
        var key = !string.IsNullOrWhiteSpace(url)
            ? $"{sourceName}|{url}"
            : $"{sourceName}|{title}|{description}";

        var bytes = Encoding.UTF8.GetBytes(key);
        var hash = SHA256.HashData(bytes);
        return Convert.ToHexString(hash)[..32].ToLowerInvariant();
    }
}