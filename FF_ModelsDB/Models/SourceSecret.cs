using System;

namespace FF_ModelsDB.Models;

/// <summary>
/// Where the secret must be sent when calling the external source.
/// </summary>
public enum SecretLocation
{
    Header = 0,
    QueryString = 1
}

/// <summary>
/// Implementa la tabla "Secrets/Settings" que quedaba "To be defined" en el
/// enunciado. Un Source puede necesitar cero, una o varias keys (por eso es
/// una tabla 1-a-muchos, ligada a Sources.RequiresSecret).
/// </summary>
public partial class SourceSecret
{
    public int Id { get; set; }

    public int SourceId { get; set; }

    public string KeyName { get; set; } = string.Empty;

    public string KeyValue { get; set; } = string.Empty;

    public SecretLocation Location { get; set; } = SecretLocation.Header;

    public virtual Source? Source { get; set; }
}
