using Microsoft.CodeAnalysis;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace PA.CacheAside.Models;

internal class AttributeArgumentDef
{
    public AttributeArgumentDef(string name, TypedConstant value)
    {
        Name = name;
        if (value.Kind == TypedConstantKind.Array)
        {
            Values = value.Values
                .Where(x => !x.IsNull)
                .Select(v => v.Value)
                .ToArray()
                ?? [];
        }
        else
        {
            Value = value.Value;
        }
            
    
        
    }
    public string Name { get; }
    public object Value { get; }
    public object[] Values { get; }
}
