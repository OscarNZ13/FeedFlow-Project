using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FF.Architecture.Dtos
{
        public class ExportSourceItemDto
        {
            public string Title { get; set; }

            public string? Description { get; set; }

            public string? ImageUrl { get; set; }

            public string? Url { get; set; }

            public string? PublishedAt { get; set; }

            public string SourceName { get; set; }

            public string SourceUrl { get; set; }

            public string? SourceDescription { get; set; }

            public string SourceComponentType { get; set; }

            public bool SourceRequiresSecret { get; set; }
        }
    }