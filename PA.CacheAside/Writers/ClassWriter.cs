using System;
using System.Collections.Generic;
using System.Reflection.Metadata;
using System.Linq;
using PA.CacheAside.Extensions;

namespace PA.CacheAside.Writers
{
    internal class ClassWriter : TypeWriter
    {
        public ClassWriter()
        : base()
        {
        }

        public IEnumerable<string> Arguments { get; set; } = new List<string>();

        public IEnumerable<ClassMethodWriter> MethodWriters { get; set; } = new List<ClassMethodWriter>();

        public IEnumerable<PropertyWriter> PropertyWriters { get; set; } = new List<PropertyWriter>();

        public override string ToString()
        {
            return $@"
public class {Name}({Arguments.Join(", ")}) : {Interfaces.Join(", ")}
{{
    {PropertyWriters.Select(x => x.ToString()).Join("\n")}
    {MethodWriters.Select(x=>x.ToString()).Join("\n")}
}}";
        }
    }
}
