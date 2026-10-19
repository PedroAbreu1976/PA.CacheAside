using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using PA.CacheAside.Extensions;
using PA.CacheAside.Models;
using PA.CacheAside.Writers;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

namespace PA.CacheAside;

internal static class CacheAsideImplementationCreator
{
    internal static IEnumerable<InterfaceDef> GetCacheEnabledInterfaceDefs(this (Compilation Left, ImmutableArray<InterfaceDeclarationSyntax> Right) tuple)
    {
        return tuple.Right
            .Select(x => tuple.Left.GetInterface(x))
            .Where(x => x.HasAttribute(TypeNameConstants.CacheInterfaceAttribute)); 
    }

    internal static void CreateCacheAsideImplementation(this SourceProductionContext context, InterfaceDef interfaceDef)
    {
        var fileWriter = CreateFileWriter(interfaceDef);
        context.AddSource($"{interfaceDef.Name}Cache.g.cs", fileWriter.ToString());
    }

    private static FileWriter CreateFileWriter(InterfaceDef interfaceDef) =>
        new FileWriter
        {
            Namespace = interfaceDef.Namespace,
            TypeWriters =
            [
                CreateClassWriter(interfaceDef),
                CreateInvalidatorInterfaceWriter(interfaceDef)
            ]
        };

    private static ClassWriter CreateClassWriter(InterfaceDef interfaceDef)
    {
        var classMethodWriters = interfaceDef.MethodDefs
            .Select(m => CreateClassMethodWriter(m, interfaceDef))
            .ToArray();
        var invalidatorMethodWriters = interfaceDef.MethodDefs
            .Where(m => m.HasResult())
            .Where(m => !IgnoreMethod(m))
            .Select(m => CreateClassInvalidatorMethodWriter(m, interfaceDef))
            .ToArray();
        var generalInvalidatorMethodWriter = CreateClassGeneralInvalidatorMethodWriter(interfaceDef.FullName);
        ClassMethodWriter[] methodWriters =
            [
                .. classMethodWriters,
            .. invalidatorMethodWriters,
            generalInvalidatorMethodWriter
            ];

        var propertyWriters = interfaceDef.Properties
            .Select(p => CreatePropertyWriter(p));

        return new ClassWriter
        {
            Name = $"{interfaceDef.Name}Cache",
            Arguments =
            [
                $"{interfaceDef.FullName} source",
            $"{TypeNameConstants.HybridCacheType} cache"
            ],
            Interfaces =
            [
                interfaceDef.FullName,
            $"{interfaceDef.Name}Invalidator"
            ],
            MethodWriters = methodWriters,
            PropertyWriters = propertyWriters
        };
    }

    private static PropertyWriter CreatePropertyWriter(PropertyDef propertyDef)
    {
        return new PropertyWriter
        {
            Name = propertyDef.Name,
            TypeName = propertyDef.TypeName,
            HasGetter = propertyDef.HasGetter,
            HasSetter = propertyDef.HasSetter,
            GetterBody = propertyDef.HasGetter ? $"return source.{propertyDef.Name};" : null,
            SetterBody = propertyDef.HasSetter ? $"source.{propertyDef.Name} = value;" : null
        };
    }

    private static ClassMethodWriter CreateClassMethodWriter(MethodDef methodDef, InterfaceDef interfaceDef) =>
        new ClassMethodWriter
        {
            Name = methodDef.Name,
            ReturnType = methodDef.ReturnTypeFullName,
            Arguments = methodDef.Parameters.Select(x => x.Definition),
            IsAsync = methodDef.ResultIsAsyncOrTaskOrValueTask(),
            Body = GetBody(methodDef, interfaceDef)
        };

    private static string GetBody(MethodDef methodDef, InterfaceDef interfaceDef)
    {
        var interfacesFullname = interfaceDef.FullName;
        var isAsync = methodDef.ResultIsAsyncOrTaskOrValueTask();
        var ignore = IgnoreMethod(methodDef);
        var tagsString = GetTagsString();
        var timeoutInMinutes = GetTimeOutInMinutes();
        var localTimeoutInMinutes = GetLocalTimeOutInMinutes();
        var flags = GetFlags();
        var hasResult = methodDef.HasResult();
        var methodName = methodDef.Name;
        var parametersCall = methodDef.GetParametersCall();
        var cacheKey = CreateCacheKey(interfacesFullname, methodDef);

        var cancellationTokenParameterName = methodDef.Parameters
            .FirstOrDefault(p => p.IsCancelationToken())?.Name
            ?? TypeNameConstants.CancelationTokenNoneValue;

        if (!hasResult)
        {
            return GetNoResultBody();
        }
        else if (ignore)
        {
            return GetNoCacheBody();
        }
        else if (isAsync)
        {
            return GetCacheAsyncBody();
        }
        else
        {
            return GetCacheSyncBody();
        }

        string GetNoResultBody() => $@"
        {(isAsync ? "await " : "")}source.{methodName}({parametersCall});";

        string GetNoCacheBody() => $@"
        return {(isAsync ? "await " : "")}source.{methodName}({parametersCall});";

        string GetCacheAsyncBody() => $@"
        System.Diagnostics.Debug.WriteLine($""Fetching Cache Key:{cacheKey}"");
        var cacheKey = $""{cacheKey}"";
        return await cache.GetOrCreateAsync(
            key: cacheKey,
            factory: async _ => await source.{methodName}({parametersCall}),
            cancellationToken: {cancellationTokenParameterName},
            options: {GetEntryOptions()},
            tags: [{tagsString}]);";

        string GetCacheSyncBody() => $@"
        System.Diagnostics.Debug.WriteLine($""Fetching Cache Key:{cacheKey}"");
        var cacheKey = $""{cacheKey}"";
        return cache.GetOrCreateAsync(
            key: cacheKey,
            factory: _ => ValueTask.FromResult(source.{methodName}({parametersCall})),
            cancellationToken: {TypeNameConstants.CancelationTokenNoneValue},
            options: {GetEntryOptions()},
            tags: [{tagsString}]).Result;";

        string GetEntryOptions() =>
            $@"new {TypeNameConstants.HybridCacheEntryOptionsType}
            {{
                Expiration = {GetTimeSpanString(timeoutInMinutes)},
                LocalCacheExpiration = {GetTimeSpanString(localTimeoutInMinutes)},
                Flags = {(flags.HasValue ? $"({TypeNameConstants.HybridCacheEntryFlagsType}){flags}" : "null")}
            }}";

        string GetTimeSpanString(int? minutes) => minutes.HasValue ? $"TimeSpan.FromMinutes({minutes.Value})" : "null";

        string GetTagsString()
        {
            var tagArray = methodDef.GetAttributeFirstValueArray<string>(TypeNameConstants.CacheTagAttribute);
            tagArray = [.. tagArray, interfacesFullname];
            return $"\"{tagArray.Join("\", \"")}\"";
        }

        int? GetTimeOutInMinutes() => methodDef
            .GetAttributeFirstValue<int?>(TypeNameConstants.CacheTimeoutAttribute);
        int? GetLocalTimeOutInMinutes() => methodDef
            .GetAttributeFirstValue<int?>(TypeNameConstants.CacheLocalTimeoutAttribute);
        int? GetFlags() => methodDef
            .GetAttributeFirstValue<int?>(TypeNameConstants.CacheFlagsAttribute);
    }

    private static ClassMethodWriter CreateClassInvalidatorMethodWriter(MethodDef methodDef, InterfaceDef interfaceDef)
    {
        var cacheKey = CreateCacheKey(interfaceDef.FullName, methodDef);
        var arguments = methodDef.Parameters
            .Where(p => !p.IsCancelationToken())
            .Select(p => p.Definition)
            .ToArray();
        arguments = [.. arguments, $"{TypeNameConstants.CancelationTokenType} ct"];

        return new ClassMethodWriter
        {
            Name = $"Invalidate{methodDef.Name}",
            ReturnType = TypeNameConstants.TaskType,
            Arguments = arguments,
            IsAsync = true,
            Body = $@"
        System.Diagnostics.Debug.WriteLine($""Invalidating Cache Key:{cacheKey}"");
        var cacheKey = $""{cacheKey}"";
        await cache.RemoveAsync(cacheKey, ct);
        "
        };
    }

    private static ClassMethodWriter CreateClassGeneralInvalidatorMethodWriter(string interfaceFullName) =>
        new ClassMethodWriter
        {
            Name = "InvalidateAsync",
            ReturnType = TypeNameConstants.TaskType,
            Arguments = [$"{TypeNameConstants.CancelationTokenType} ct"],
            IsAsync = true,
            Body = $@"
        await cache.RemoveByTagAsync(""{interfaceFullName}"", ct);
        "
        };

    private static InterfaceWriter CreateInvalidatorInterfaceWriter(InterfaceDef cacheEnabledImplementedInterface) =>
        new InterfaceWriter
        {
            Name = $"{cacheEnabledImplementedInterface.Name}Invalidator",
            Interfaces = [TypeNameConstants.CacheInvalidatorInterface],
            MethodWriters = cacheEnabledImplementedInterface.MethodDefs
                .Where(m => m.HasResult())
                .Where(m => !IgnoreMethod(m))
                .Select(m => CreateInvalidatorInterfaceMethodWriter(m))
        };

    private static InterfaceMethodWriter CreateInvalidatorInterfaceMethodWriter(MethodDef methodDef)
    {
        var arguments = methodDef.Parameters
            .Where(p => !p.IsCancelationToken())
            .Select(p => p.Definition)
            .ToArray();
        arguments = [.. arguments, $"{TypeNameConstants.CancelationTokenType} ct"];
        return new InterfaceMethodWriter
        {
            Name = $"Invalidate{methodDef.Name}",
            ReturnType = TypeNameConstants.TaskType,
            Arguments = arguments
        };
    }

    private static bool IgnoreMethod(MethodDef methodDef) =>
        methodDef.GetAttributeFirstValue<bool?>(TypeNameConstants.CacheIgnoreAttribute) ?? false;

    private static string CreateCacheKey(string interfaceFullName, MethodDef methodDef)
    {
        var cacheParameters = methodDef.Parameters
            .Where(p => !p.IsCancelationToken())
            .Select(x => x.ValueInterpolation)
            .Join(", ");
        return $"{interfaceFullName}.{methodDef.Name}({cacheParameters})";
    }
}
