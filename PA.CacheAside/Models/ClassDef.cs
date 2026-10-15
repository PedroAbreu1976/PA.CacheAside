using Microsoft.CodeAnalysis;
using System;
using System.Collections.Generic;
using System.Linq;

namespace PA.CacheAside.Models
{
    internal record ClassDef : TypeDef
    {
        public ClassDef(INamedTypeSymbol symbol)
            :base(symbol)
        {
        }
    }
}
