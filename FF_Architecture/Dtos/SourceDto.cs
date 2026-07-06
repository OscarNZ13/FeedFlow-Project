using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FF.Architecture.Dtos
{
    public class SourceDto
    {
        public string Url { get; set; }
        public string Name { get; set; }
        public string? Description { get; set; }
        public string ComponentType { get; set; }
        public bool RequiresSecret { get; set; }
    }
}
