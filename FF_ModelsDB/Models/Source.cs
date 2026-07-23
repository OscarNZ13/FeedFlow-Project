using System;
using System.Collections.Generic;

namespace FF_ModelsDB.Models;

public partial class Source
{
    public int Id { get; set; }

    public string Url { get; set; } = null!;

    public string Name { get; set; } = null!;

    public string? Description { get; set; }

    public string ComponentType { get; set; } = null!;

    public bool RequiresSecret { get; set; }

    // --- Agregado para el módulo de Feed (Mel) ---
    public bool IsActive { get; set; } = true;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime? LastFetchedAt { get; set; }

    public virtual ICollection<SourceSecret> Secrets { get; set; } = new List<SourceSecret>();
    // --- fin de lo agregado ---

    public virtual ICollection<SourceItem> SourceItems { get; set; } = new List<SourceItem>();
}
