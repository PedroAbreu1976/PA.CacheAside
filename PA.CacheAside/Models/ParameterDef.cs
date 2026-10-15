using Microsoft.CodeAnalysis;
using System;

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
            Definition = $"{modifier}{symbol.Type.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat)} {symbol.Name}";
            Call = $"{modifier}{symbol.Name}";
            ValueInterpolation = $"{{{symbol.Name}}}";
            Type = Type.GetType(symbol.Type.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat));
            TypeFullName = symbol.Type.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat);
        }

        public string Name { get; }
        public Type Type { get; } 
        public string TypeFullName { get; }
        public string Definition { get; } 
        public string Call { get; } 
        public string ValueInterpolation { get; } 
    }
}
