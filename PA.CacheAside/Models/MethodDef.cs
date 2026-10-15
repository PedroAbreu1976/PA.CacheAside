using Microsoft.CodeAnalysis;
using System;
using System.Collections.Generic;
using System.Linq;

namespace PA.CacheAside.Models
{
    internal record MethodDef
    {
        public MethodDef(IMethodSymbol symbol)
        {
            if (symbol == null) throw new ArgumentNullException(nameof(symbol));
            Name = symbol.Name;
            ReturnTypeFullName = symbol.ReturnType.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat);
            ReturnType = Type.GetType(ReturnTypeFullName);
            Parameters = symbol.Parameters.Select(x=> new ParameterDef(x));
            IsAsync = symbol.IsAsync;
            Attributes = symbol.GetAttributes().Select(x => new AttributeDef(x));
        }

        public string Name { get; }
        public Type ReturnType { get; }
        public string ReturnTypeFullName { get; }
        public bool IsAsync { get; }
        public IEnumerable<ParameterDef> Parameters { get; }
        public IEnumerable<AttributeDef> Attributes { get; }
    }
}
