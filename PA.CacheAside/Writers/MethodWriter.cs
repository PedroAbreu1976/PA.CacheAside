using PA.CacheAside.Extensions;
using System.Collections.Generic;

namespace PA.CacheAside.Writers;

internal abstract class MethodWriter
{
    public string Name { get; set; }

    public string ReturnType { get; set; } = "void";
    public IEnumerable<string> Arguments { get; set; } = new List<string>();
}

internal class InterfaceMethodWriter : MethodWriter
{
    public override string ToString()
    {
        return $@"
    {ReturnType} {Name}({Arguments.Join(", ")});";
    }
}

internal class ClassMethodWriter : MethodWriter
{
    public bool IsAsync { get; set; } = false;
    public bool IsStatic { get; set; }

    public string Body { get; set; } = "";

    public override string ToString()
    {
        return $@"
    public {(IsAsync ? "async" : "")}{(IsStatic ? "static" : "")} {ReturnType} {Name}({Arguments.Join(", ")})
    {{
{Body}
    }}";
    }
}