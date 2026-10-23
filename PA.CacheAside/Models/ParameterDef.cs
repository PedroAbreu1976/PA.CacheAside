using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Linq;

namespace PA.CacheAside.Models
{
    internal record ParameterDef
    {
        public ParameterDef(IParameterSymbol symbol)
        {
            var modifier = symbol.RefKind switch
            {
                RefKind.Ref => "ref ",
                RefKind.Out => "out ",
                RefKind.In => "in ",
                _ => string.Empty
            };
            Name = symbol.Name;
            
            Call = $"{modifier}{symbol.Name}";
            ValueInterpolation = $"{{{symbol.Name}}}";
            Type = Type.GetType(symbol.Type.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat));
            TypeFullName = symbol.Type.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat);
            Definition = $"{modifier}{TypeFullName} {symbol.Name}";
            
        }

        public string Name { get; }
        public Type Type { get; } 
        public string TypeFullName { get; }
        public string Definition { get; } 
        public string Call { get; } 
        public string ValueInterpolation { get; } 
    }
}
