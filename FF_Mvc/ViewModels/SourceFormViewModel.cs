using System.ComponentModel.DataAnnotations;

namespace FF_Mvc.ViewModels;

public class SourceFormViewModel
{
    [Required(ErrorMessage = "La URL es obligatoria")]
    [Url(ErrorMessage = "Debe ser una URL válida")]
    public string Url { get; set; } = string.Empty;

    [Required(ErrorMessage = "El nombre es obligatorio")]
    public string Name { get; set; } = string.Empty;

    public string? Description { get; set; }

    public string ComponentType { get; set; } = "feed";

    public bool RequiresSecret { get; set; }

    public string? SecretKeyName { get; set; }

    public string? SecretKeyValue { get; set; }

    /// <summary>0 = Header, 1 = QueryString (igual que FF_ModelsDB.Models.SecretLocation)</summary>
    public int SecretLocation { get; set; } = 0;
}

public class SourceListItemViewModel
{
    public int Id { get; set; }
    public string Url { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string ComponentType { get; set; } = string.Empty;
    public bool RequiresSecret { get; set; }
    public bool IsActive { get; set; }
    public DateTime? LastFetchedAt { get; set; }
}