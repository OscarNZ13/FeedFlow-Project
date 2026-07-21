using FF_ModelsDB;

namespace FF_Api.ViewModels;

public class SourceViewModel
{
    public int Id { get; set; }
    public string Url { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string ComponentType { get; set; } = "feed";
    public bool RequiresSecret { get; set; }
    public bool IsActive { get; set; } = true;
    public DateTime? LastFetchedAt { get; set; }
}

/// <summary>Payload used by the Admin "agregar fuente" form (URL, Name, Key).</summary>
public class CreateSourceRequest
{
    public string Url { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string ComponentType { get; set; } = "feed";

    /// <summary>Optional: if the source needs an API key/token to be called.</summary>
    public string? SecretKeyName { get; set; }
    public string? SecretKeyValue { get; set; }
    public SecretLocation SecretLocation { get; set; } = SecretLocation.Header;
}
