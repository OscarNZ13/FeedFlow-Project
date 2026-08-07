namespace FF_ModelsDB.Models;

public class CollectionItem
{
    public int Id { get; set; }
    public int CollectionId { get; set; }
    public int SourceItemId { get; set; }

    public virtual Collection Collection { get; set; } = null!;
    public virtual SourceItem SourceItem { get; set; } = null!;
}

