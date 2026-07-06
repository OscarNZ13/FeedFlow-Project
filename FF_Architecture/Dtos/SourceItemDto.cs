using System.Text.Json;

namespace FF.Architecture.Dtos
{
    public class SourceItemDto
    {
        public string Title { get; set; }
        public string Description { get; set; }
        public string Url { get; set; }
        public string? Author { get; set; }
        public DateTime? PublishedAt { get; set; }
    }
}