using System.Text.Json;

namespace FF.Architecture.Dtos
{
    public class SourcePackageDto
    {
        public SourceDto Source { get; set; }
        public List<SourceItemDto> Items { get; set; }
    }
}