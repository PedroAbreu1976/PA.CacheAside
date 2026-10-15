using PA.CacheAside.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PA.CacheAside.Writers
{
    internal class InterfaceWriter : TypeWriter
    {
        public InterfaceWriter():base() { }

        public IEnumerable<InterfaceMethodWriter> MethodWriters { get; set; } = new List<InterfaceMethodWriter>();

        public override string ToString()
        {
            return $@"
public interface {Name} : {Interfaces.Join(", ")}
{{
    { MethodWriters.Select(x=>x.ToString()).Join("\n")}
}}";
        }
    }
}
