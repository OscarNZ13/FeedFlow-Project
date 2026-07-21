namespace FF_ModelsDB;

public partial class SourceItem
{
    public int Id { get; set; }

    public int SourceId { get; set; }

    public string Json { get; set; } = string.Empty;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public virtual Source? Source { get; set; }
}
