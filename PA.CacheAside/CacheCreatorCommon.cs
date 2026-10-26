using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using PA.CacheAside.Extensions;
using PA.CacheAside.Models;
using PA.CacheAside.Writers;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

namespace PA.CacheAside;

internal static class CacheCreatorCommon
{
    internal static IEnumerable<InterfaceDef> GetCacheEnabledInterfaceDefs(this (Compilation Left, ImmutableArray<InterfaceDeclarationSyntax> Right) tuple)
    {
        return tuple.Right
            .Select(x => tuple.Left.GetInterface(x))
            .Where(x => x.HasAttribute(TypeNameConstants.CacheInterfaceAttribute));
    }

    internal static bool IgnoreMethod(this MethodDef methodDef) =>
        methodDef.GetAttributeFirstValue<bool?>(TypeNameConstants.CacheIgnoreAttribute) ?? false;

    internal static string CreateCacheKey(this MethodDef methodDef)
    {
        var cacheParameters = methodDef.Parameters
            .Where(p => !p.IsCancelationToken())
            .Select(x => $"{{cacheKeyFactory.GetCacheKey({x.Name})}}")
            .Join(", ");
        return $"{methodDef.ParentTypeDef.FullName}.{methodDef.Name}({cacheParameters})";
    }

    internal static string CreateCacheTag(this MethodDef methodDef) =>
        $"{methodDef.ParentTypeDef.FullName}.{methodDef.Name}";
}
