using Microsoft.CodeAnalysis;
using PA.CacheAside.Extensions;
using PA.CacheAside.Models;
using PA.CacheAside.Writers;
using System.Collections.Generic;
using System.Linq;

namespace PA.CacheAside;

internal static class CacheAsideExtensionsCreator
{
    internal static void CreateCacheAsideExtensions(this SourceProductionContext context, IEnumerable<InterfaceDef> interfaceDefs)
    {
        var fileWriter = CreateFileWriter(interfaceDefs);
        context.AddSource("CacheAsideExtensions.g.cs", fileWriter.ToString());
    }  

    private static FileWriter CreateFileWriter(IEnumerable<InterfaceDef> interfaceDefs) =>
        new FileWriter
        {
            Namespace = "Microsoft.Extensions.DependencyInjection",
            TypeWriters =
            [
                new ClassWriter {
                    Name = "CacheAsideExtensions",
                    IsStatic = true,
                    MethodWriters = [
                        interfaceDefs.CreateAddCacheAsideMethod(),
                        CreateAddCacheAsideAndHybridCacheWithOptionsMethod(),
                        CreateAddCacheAsideAndHybridCacheWithoutOptionsMethod()
                    ]
            }]
        };

    private static ClassMethodWriter CreateAddCacheAsideMethod(this IEnumerable<InterfaceDef> interfaceDefs) =>
        new ClassMethodWriter
        {
            Name = "AddCacheAside",
            ReturnType = TypeNameConstants.IServiceCollectionType,
            IsStatic = true,
            Arguments = ["this IServiceCollection services"],
            Body = $@"
{interfaceDefs.GetAddCacheAsideMethodBody()}
        return services;"
        };

    private static string GetAddCacheAsideMethodBody(this IEnumerable<InterfaceDef> interfaceDefs)=>
        interfaceDefs
            .Select(interfaceDef => $@"
        services.Decorate<{interfaceDef.FullName}, {interfaceDef.FullName}Cache>();
        services.AddScoped<{interfaceDef.FullName}Invalidator>(sp => ({interfaceDef.FullName}Invalidator)sp.GetRequiredService<{interfaceDef.FullName}> ());")
            .Join("\n");

    private static ClassMethodWriter CreateAddCacheAsideAndHybridCacheWithOptionsMethod() =>
         new ClassMethodWriter
         {
             Name = "AddCacheAsideAndHybridCache",
             ReturnType = TypeNameConstants.IHybridCacheBuilderType,
             IsStatic = true,
             Arguments = ["this IServiceCollection services", $"Action<{TypeNameConstants.HybridCacheOptionsType}> setupAction"],
             Body = $@"
        services.AddCacheAside();
        return services.AddHybridCache(setupAction);"
         };

    private static ClassMethodWriter CreateAddCacheAsideAndHybridCacheWithoutOptionsMethod() =>
         new ClassMethodWriter
         {
             Name = "AddCacheAsideAndHybridCache",
             ReturnType = TypeNameConstants.IHybridCacheBuilderType,
             IsStatic = true,
             Arguments = ["this IServiceCollection services"],
             Body = $@"
        services.AddCacheAside();
        return services.AddHybridCache();"
         };
}
