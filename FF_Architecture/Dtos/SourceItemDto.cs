using System.Text.Json;

namespace FF.Architecture.Dtos
{
        public class SourceItemDto
        {
            public string Id { get; set; } = string.Empty;

            public string Title { get; set; } = string.Empty;

            public string? Description { get; set; }

            public string? ImageUrl { get; set; }

            public string? Url { get; set; }

            public string? Category { get; set; }

            public List<string> Tags { get; set; } = new();

            public DateTime? PublishedAt { get; set; }

            public string? Author { get; set; }
        }
    }