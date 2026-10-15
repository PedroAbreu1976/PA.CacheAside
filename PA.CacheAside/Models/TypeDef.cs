using Microsoft.CodeAnalysis;
using System;
using System.Collections.Generic;
using System.Linq;

namespace PA.CacheAside.Models
{
    internal abstract record TypeDef
    {
        public TypeDef(INamedTypeSymbol symbol)
        {
            if (symbol == null) throw new ArgumentNullException(nameof(symbol));
            Name = symbol.Name;
            FullName = symbol.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat);
            Namespace = symbol.ContainingNamespace?.ToDisplayString();
            MethodDefs = symbol.GetMembers()
                    .OfType<IMethodSymbol>()
                    .Where(m => m.MethodKind == MethodKind.Ordinary)
                    .Select(m => new MethodDef(m));
            Properties = symbol.GetMembers()
                    .OfType<IPropertySymbol>()
                    .Select(p => new PropertyDef(p));
            Interfaces = symbol.AllInterfaces.Select(x => new InterfaceDef(x));
            Attributes = symbol.GetAttributes().Select(x => new AttributeDef(x));
        }

        public string Name { get; }
        public string FullName { get; }
        public string Namespace { get; }
        public IEnumerable<MethodDef> MethodDefs { get; }
        public IEnumerable<PropertyDef> Properties { get; }
        public IEnumerable<InterfaceDef> Interfaces { get; }

        public IEnumerable<AttributeDef> Attributes { get; }
    }
}
