namespace FF_ModelsDB;

public enum SecretLocation
{
    Header = 0,
    QueryString = 1
}

public partial class SourceSecret
{
    public int Id { get; set; }

    public int SourceId { get; set; }

    public string KeyName { get; set; } = string.Empty;

    public string KeyValue { get; set; } = string.Empty;

    public SecretLocation Location { get; set; } = SecretLocation.Header;

    public virtual Source? Source { get; set; }
}
