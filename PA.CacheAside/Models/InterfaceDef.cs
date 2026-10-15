using Microsoft.CodeAnalysis;
using System.Dynamic;

namespace PA.CacheAside.Models
{
    internal record InterfaceDef : TypeDef
    {
        public InterfaceDef(INamedTypeSymbol symbol)
            :base(symbol)
        {
        }
    }
}
