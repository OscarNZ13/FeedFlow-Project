using System;
using System.Collections.Generic;

namespace FF_ModelsDB.Models;

public partial class SourceItem
{
    public int Id { get; set; }

    public int? SourceId { get; set; }

    public string? Json { get; set; }

    public DateTime? CreatedAt { get; set; }

    public virtual Source? Source { get; set; }

    public virtual ICollection<Favorite> Favorites { get; set; } = new List<Favorite>();
}
