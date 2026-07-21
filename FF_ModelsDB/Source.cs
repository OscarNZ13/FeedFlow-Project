using System.Collections.Generic;

namespace FF_ModelsDB;

public partial class Source
{
    public int Id { get; set; }

    public string Url { get; set; } = string.Empty;

    public string Name { get; set; } = string.Empty;

    public string? Description { get; set; }

    public string ComponentType { get; set; } = "feed";

    public bool RequiresSecret { get; set; }

    public bool IsActive { get; set; } = true;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime? LastFetchedAt { get; set; }

    public virtual ICollection<SourceItem> SourceItems { get; set; } = new List<SourceItem>();

    public virtual ICollection<SourceSecret> Secrets { get; set; } = new List<SourceSecret>();
}
