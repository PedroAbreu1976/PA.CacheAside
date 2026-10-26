using Microsoft.CodeAnalysis;
using PA.CacheAside.Extensions;
using PA.CacheAside.Models;
using PA.CacheAside.Writers;
using System.Linq;

namespace PA.CacheAside;

internal static class CacheAsideImplementationCreator
{
    internal static void CreateCacheAsideImplementation(this SourceProductionContext context, InterfaceDef interfaceDef)
    {
        var fileWriter = interfaceDef.CreateFileWriter();
        context.AddSource($"{interfaceDef.Name}Cache.g.cs", fileWriter.ToString());
    }

    private static FileWriter CreateFileWriter(this InterfaceDef interfaceDef) =>
        new FileWriter
        {
            Namespace = interfaceDef.Namespace,
            Usings = ["PA.CacheAside.Core"],
            TypeWriters = [interfaceDef.CreateClassWriter()]
        };

    private static ClassWriter CreateClassWriter(this InterfaceDef interfaceDef)
    {
        var classMethodWriters = interfaceDef.MethodDefs
            .Select(m => m.CreateClassMethodWriter())
            .ToArray();

        var propertyWriters = interfaceDef.Properties
            .Select(p => p.CreatePropertyWriter());

        return new ClassWriter
        {
            Name = $"{interfaceDef.Name}Cache",
            Arguments =
            [
                $"{interfaceDef.FullName} source",
                $"{TypeNameConstants.HybridCacheType} cache",
                $"{TypeNameConstants.ICacheKeyFactoryType} cacheKeyFactory"
            ],
            Interfaces = [interfaceDef.FullName],
            MethodWriters = classMethodWriters,
            PropertyWriters = propertyWriters
        };
    }

    private static PropertyWriter CreatePropertyWriter(this PropertyDef propertyDef)
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

    private static ClassMethodWriter CreateClassMethodWriter(this MethodDef methodDef) =>
        new ClassMethodWriter
        {
            Name = methodDef.Name,
            ReturnType = methodDef.ReturnTypeFullName,
            Arguments = methodDef.Parameters.Select(x => x.Definition),
            IsAsync = methodDef.ResultIsAsyncOrTaskOrValueTask(),
            Body = methodDef.GetBody()
        };

    private static string GetBody(this MethodDef methodDef)
    {
        var interfacesFullname = methodDef.ParentTypeDef.FullName;
        var isAsync = methodDef.ResultIsAsyncOrTaskOrValueTask();
        var ignore = CacheCreatorCommon.IgnoreMethod(methodDef);
        var timeoutInMinutes = GetTimeOutInMinutes();
        var localTimeoutInMinutes = GetLocalTimeOutInMinutes();
        var flags = GetFlags();
        var hasResult = methodDef.HasResult();
        var methodName = methodDef.Name;
        var parametersCall = methodDef.GetParametersCall();
        var cacheKey = methodDef.CreateCacheKey();

        var cancellationTokenParameterName = methodDef.Parameters
            .FirstOrDefault(p => p.IsNonNullableCancelationToken())?.Name
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
        var cacheKey = $""{cacheKey}"";
        System.Diagnostics.Debug.WriteLine($""Fetching Cache Key:{{cacheKey}}"");
        return await cache.GetOrCreateAsync(
            key: cacheKey.GenerateCompactCacheKey(),
            factory: async _ => await source.{methodName}({parametersCall}),
            cancellationToken: {cancellationTokenParameterName},
            options: {GetEntryOptions()},
            tags: [{GetTagsString()}]);";

        string GetCacheSyncBody() => $@"
        var cacheKey = $""{cacheKey}"";
        System.Diagnostics.Debug.WriteLine($""Fetching Cache Key:{{cacheKey}}"");
        return cache.GetOrCreateAsync(
            key: cacheKey.GenerateCompactCacheKey(),
            factory: _ => ValueTask.FromResult(source.{methodName}({parametersCall})),
            cancellationToken: {TypeNameConstants.CancelationTokenNoneValue},
            options: {GetEntryOptions()},
            tags: [{GetTagsString()}]).Result;";

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
            var tagArray = methodDef
                .GetAttributeFirstValueArray<string>(TypeNameConstants.CacheTagAttribute)
                .Select(x=> $"\"{x}\"");
            tagArray = 
            [
                .. tagArray, 
                $"@\"{interfacesFullname}\".GenerateCompactCacheKey()",
                $"@\"{methodDef.CreateCacheTag()}\".GenerateCompactCacheKey()"
            ];
            return $"{tagArray.Join(", ")}";
        }

        int? GetTimeOutInMinutes() => methodDef
            .GetAttributeFirstValue<int?>(TypeNameConstants.CacheTimeoutAttribute);
        int? GetLocalTimeOutInMinutes() => methodDef
            .GetAttributeFirstValue<int?>(TypeNameConstants.CacheLocalTimeoutAttribute);
        int? GetFlags() => methodDef
            .GetAttributeFirstValue<int?>(TypeNameConstants.CacheFlagsAttribute);
    }
}
