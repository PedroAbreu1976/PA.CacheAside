using Microsoft.CodeAnalysis;
using System;

namespace PA.CacheAside.Models
{
    internal class PropertyDef
    {
        public PropertyDef(IPropertySymbol propertySymbol)
        {
            if (propertySymbol == null) throw new ArgumentNullException(nameof(propertySymbol));
            Name = propertySymbol.Name;
            TypeName = propertySymbol.Type?.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat);
            HasGetter = propertySymbol.GetMethod != null;
            HasSetter = propertySymbol.SetMethod != null;
        } 
        
        public string Name { get; }   
        public string TypeName { get; }
        public bool HasGetter { get; }
        public bool HasSetter { get; }
    }
}
