namespace FF_ModelsDB.Models;

/// <summary>Relación entre un usuario y una noticia marcada como favorita.</summary>
public class Favorite
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public int SourceItemId { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime LastFavoriteAt { get; set; } = DateTime.UtcNow;

    public virtual User User { get; set; } = null!;
    public virtual SourceItem SourceItem { get; set; } = null!;
}
