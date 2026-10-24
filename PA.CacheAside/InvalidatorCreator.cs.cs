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
            TypeWriters =
            [
                CreateInvalidatorInterfaceWriter(interfaceDef),
                CreateClassWriter(interfaceDef),
            ]
        };

    private static ClassWriter CreateClassWriter(InterfaceDef interfaceDef)
    {
        var invalidatorMethodWriters = interfaceDef.MethodDefs
            .Where(m => m.HasResult())
            .Where(m => !CacheCreatorCommon.IgnoreMethod(m))
            .Select(m => CreateClassInvalidatorMethodWriter(m, interfaceDef))
            .ToArray();
        var generalInvalidatorMethodWriter = CreateClassGeneralInvalidatorMethodWriter(interfaceDef.FullName);
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

    private static ClassMethodWriter CreateClassInvalidatorMethodWriter(MethodDef methodDef, InterfaceDef interfaceDef)
    {
        var cacheKey = methodDef.CreateCacheKey(interfaceDef.FullName);
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

    private static InterfaceWriter CreateInvalidatorInterfaceWriter(InterfaceDef cacheEnabledImplementedInterface) =>
    new InterfaceWriter
    {
        Name = $"{cacheEnabledImplementedInterface.Name}Invalidator",
        Interfaces = [TypeNameConstants.CacheInvalidatorInterface],
        MethodWriters = cacheEnabledImplementedInterface.MethodDefs
            .Where(m => m.HasResult())
            .Where(m => !m.IgnoreMethod())
            .Select(m => CreateInvalidatorInterfaceMethodWriter(m))
    };

    private static InterfaceMethodWriter CreateInvalidatorInterfaceMethodWriter(MethodDef methodDef)
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
