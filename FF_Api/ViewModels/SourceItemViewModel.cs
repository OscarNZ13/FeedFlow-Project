using FF_ModelsDB.Models;

namespace FF_Api.ViewModels;

public class SourceItemViewModel
{
    public int Id { get; set; }

    public int? SourceId { get; set; }

    public string? Json { get; set; }

    public DateTime? CreatedAt { get; set; }

    public virtual Source? Source { get; set; }
}