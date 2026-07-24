using FF.Architecture.Parsers;
using System.Text.Json;

namespace FF.Architecture.Dtos
{
    public class SourcePackageDto
    {
        public SourceDto Source { get; set; }
        public List<NewsItemDto> Items { get; set; } = new();
    }
}