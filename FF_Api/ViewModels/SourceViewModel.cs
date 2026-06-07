using FF_ModelsDB.Models;

namespace FF_Api.ViewModels;

public class SourceViewModel
{
    public int Id { get; set; }

    public string Url { get; set; } = null!;

    public string Name { get; set; } = null!;

    public string? Description { get; set; }

    public string ComponentType { get; set; } = null!;

    public bool RequiresSecret { get; set; }

    public virtual ICollection<SourceItem> SourceItems { get; set; } = new List<SourceItem>();
}