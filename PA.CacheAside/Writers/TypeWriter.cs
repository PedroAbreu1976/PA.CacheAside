using System;
using System.Collections.Generic;
using System.Linq;

namespace PA.CacheAside.Writers
{
    internal abstract class TypeWriter
    {
        public TypeWriter() { }

        
        public string Name { get; set; }

        public List<string> Interfaces { get; set; } = new List<string>();

        
    }
}
