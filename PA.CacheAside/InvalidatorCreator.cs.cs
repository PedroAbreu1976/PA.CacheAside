using Microsoft.CodeAnalysis;
using PA.CacheAside.Extensions;
using PA.CacheAside.Models;
using PA.CacheAside.Writers;
using System.Linq;

namespace PA.CacheAside;

internal static class InvalidatorCreator
{
    internal static void CreateInvalidators(this SourceProductionContext context, InterfaceDef interfaceDef)
    {
        var fileWriter = CreateFileWriter(interfaceDef);
        context.AddSource($"{interfaceDef.Name}Invalidator.g.cs", fileWriter.ToString());
    }

    private static FileWriter CreateFileWriter(InterfaceDef interfaceDef) =>
        new FileWriter
        {
            Namespace = interfaceDef.Namespace,
            Usings = ["PA.CacheAside.Core"],
            TypeWriters =
            [
                interfaceDef.CreateInvalidatorInterfaceWriter(),
                interfaceDef.CreateClassWriter(),
            ]
        };

    private static ClassWriter CreateClassWriter(this InterfaceDef interfaceDef)
    {
        var invalidatorMethodWriters = interfaceDef.MethodDefs
            .Where(m => m.HasResult())
            .Where(m => !CacheCreatorCommon.IgnoreMethod(m))
            .Select(m => m.CreateClassInvalidatorMethodWriter())
            .ToArray();
        var generalInvalidatorMethodWriter = interfaceDef.CreateClassGeneralInvalidatorMethodWriter();
        ClassMethodWriter[] methodWriters =
        [
            .. invalidatorMethodWriters,
            generalInvalidatorMethodWriter
        ];

        return new ClassWriter
        {
            Name = $"{interfaceDef.Name}InvalidatorImplementation",
            Arguments =
            [
                $"{TypeNameConstants.HybridCacheType} cache",
                $"{TypeNameConstants.ICacheKeyFactoryType} cacheKeyFactory"
            ],
            Interfaces = [ $"{interfaceDef.Name}Invalidator" ],
            MethodWriters = methodWriters,
        };
    }

    private static ClassMethodWriter CreateClassGeneralInvalidatorMethodWriter(this InterfaceDef interfaceDef) =>
        new ClassMethodWriter
        {
            Name = "InvalidateAsync",
            ReturnType = TypeNameConstants.TaskType,
            Arguments = [$"{TypeNameConstants.CancelationTokenType} ct"],
            IsAsync = true,
            Body = $@"
            await cache.RemoveByTagAsync(""{interfaceDef.FullName}"", ct);
            "
        };

    private static ClassMethodWriter CreateClassInvalidatorMethodWriter(this MethodDef methodDef)
    {
        var cacheKey = methodDef.CreateCacheKey();
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
        var cacheKey = $""{cacheKey}"";
        System.Diagnostics.Debug.WriteLine($""Invalidating Cache Key:{{cacheKey}}"");
        await cache.RemoveAsync(cacheKey.GenerateCompactCacheKey(), ct);
        "
        };
    }

    private static InterfaceWriter CreateInvalidatorInterfaceWriter(this InterfaceDef cacheEnabledImplementedInterface) =>
    new InterfaceWriter
    {
        Name = $"{cacheEnabledImplementedInterface.Name}Invalidator",
        Interfaces = [TypeNameConstants.CacheInvalidatorInterface],
        MethodWriters = cacheEnabledImplementedInterface.MethodDefs
            .Where(m => m.HasResult())
            .Where(m => !m.IgnoreMethod())
            .Select(m => m.CreateInvalidatorInterfaceMethodWriter())
    };

    private static InterfaceMethodWriter CreateInvalidatorInterfaceMethodWriter(this MethodDef methodDef)
    {
        var arguments = methodDef.Parameters
            .Where(p => !p.IsCancelationToken())
            .Select(p => p.Definition)
            .ToArray();
        arguments = [.. arguments, $"{TypeNameConstants.CancelationTokenType} ct = default"];
        return new InterfaceMethodWriter
        {
            Name = $"Invalidate{methodDef.Name}",
            ReturnType = TypeNameConstants.TaskType,
            Arguments = arguments
        };
    }
}
