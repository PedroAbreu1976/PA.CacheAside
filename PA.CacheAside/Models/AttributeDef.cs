using Microsoft.CodeAnalysis;
using System;
using System.Collections.Generic;
using System.Linq;

namespace PA.CacheAside.Models;

internal class AttributeDef
{
    public AttributeDef(AttributeData attributeData)
    {
        if (attributeData == null) throw new ArgumentNullException(nameof(attributeData));
        Name = attributeData.AttributeClass?.Name;
        FullName = attributeData.AttributeClass?.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat);
        Namespace = attributeData.AttributeClass?.ContainingNamespace?.ToDisplayString();
        //Arguments = attributeData.NamedArguments
        //    .Where(x => x.Value.Kind != TypedConstantKind.Error)
        //    .Select(x => new AttributeArgumentDef(x.Key, x.Value));

        Arguments = (attributeData?.ConstructorArguments ?? Enumerable.Empty<TypedConstant>())
            .Where(x => !x.IsNull)
            .Where(x => x.Kind != TypedConstantKind.Error)
            //.Select(x => )
            .Select((x, i) => new AttributeArgumentDef(i.ToString(), x))
            ?? Enumerable.Empty<AttributeArgumentDef>();
    } 
    
    public string Name { get; }   
    public string FullName { get; }
    public string Namespace { get; }

    public IEnumerable<AttributeArgumentDef> Arguments { get; }
}
