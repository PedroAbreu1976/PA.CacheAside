using PA.CacheAside.Models;
using System.Collections.Generic;
using System.Linq;

namespace PA.CacheAside.Extensions;

internal static class TypeDefExtensions
{
    public static bool ImplementsInterface(this TypeDef typeDef, InterfaceDef interfaceDef) =>
        typeDef.Interfaces.Any(x=>x == interfaceDef);

    public static IEnumerable<InterfaceDef> GetInterfacesImplementing(this TypeDef typeDef, InterfaceDef parentInterface) =>
        typeDef.Interfaces.Where(i =>
            i.Interfaces.Any(ai => ai == parentInterface));

    public static bool HasAttribute(this TypeDef typeDef, string attributeFullName) =>
        typeDef.Attributes.Any(x => x.FullName == attributeFullName);
}
