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
                    MethodWriters = [interfaceDefs.CreateAddCacheAsideMethod()]
            }]
        };

    private static ClassMethodWriter CreateAddCacheAsideMethod(this IEnumerable<InterfaceDef> interfaceDefs) =>
    new ClassMethodWriter
    {
        Name = "AddCacheAside",
        ReturnType = TypeNameConstants.CacheAsideServiceSetupType,
        IsStatic = true,
        Arguments = ["this IServiceCollection services"],
        Body = $@"
{interfaceDefs.GetAddCacheAsideMethodBody()}
        return new {TypeNameConstants.CacheAsideServiceSetupType}(services);"
    };

    private static string GetAddCacheAsideMethodBody(this IEnumerable<InterfaceDef> interfaceDefs)=>
        interfaceDefs
            .Select(interfaceDef => $@"
        services.Decorate<{interfaceDef.FullName}, {interfaceDef.FullName}Cache>();
        services.AddScoped<{interfaceDef.FullName}Invalidator, {interfaceDef.FullName}InvalidatorImplementation>();")
            .Join("\n");
}
