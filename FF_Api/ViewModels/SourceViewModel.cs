using FF_ModelsDB.Models;

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

public class CreateSourceRequest
{
    public string Url { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string ComponentType { get; set; } = "feed";

    public string? SecretKeyName { get; set; }
    public string? SecretKeyValue { get; set; }
    public SecretLocation SecretLocation { get; set; } = SecretLocation.Header;
}
