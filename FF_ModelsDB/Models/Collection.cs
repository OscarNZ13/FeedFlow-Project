namespace FF_ModelsDB.Models;

public class Collection
{
    public int Id { get; set; }
    public int UserId { get; set; }   
    public string Name { get; set; } = string.Empty;

    public virtual ICollection<CollectionItem> Items { get; set; } = new List<CollectionItem>();
}

