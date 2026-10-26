using Microsoft.CodeAnalysis;
using PA.CacheAside.Extensions;
using PA.CacheAside.Models;
using PA.CacheAside.Writers;
using System.Collections.Generic;
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
            .SelectMany(m => m.CreateClassInvalidatorMethodWriter());
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
            await cache.RemoveByTagAsync(""{interfaceDef.FullName}"".GenerateCompactCacheKey(), ct);
            "
        };

    private static ClassMethodWriter[] CreateClassInvalidatorMethodWriter(this MethodDef methodDef)
    {
        var cacheKey = methodDef.CreateCacheKey();
        var cacheTag = methodDef.CreateCacheTag();
        var arguments = methodDef.Parameters
            .Where(p => !p.IsCancelationToken())
            .Select(p => p.Definition);

        ClassMethodWriter[] result = 
        [
            new ClassMethodWriter
            {
                Name = $"Invalidate{methodDef.Name}",
                ReturnType = TypeNameConstants.TaskType,
                Arguments = [.. arguments, $"{TypeNameConstants.CancelationTokenType} ct"],
                IsAsync = true,
                Body = $@"
            var cacheKey = $""{cacheKey}"";
            System.Diagnostics.Debug.WriteLine($""Invalidating Cache Key:{{cacheKey}}"");
            await cache.RemoveAsync(cacheKey.GenerateCompactCacheKey(), ct);
            "
            }
        ];

        if (arguments.Any())
        {
            result =
            [
                .. result,
                new ClassMethodWriter
                {
                    Name = $"Invalidate{methodDef.Name}",
                    ReturnType = TypeNameConstants.TaskType,
                    Arguments = [$"{TypeNameConstants.CancelationTokenType} ct"],
                    IsAsync = true,
                    Body = $@"
                var cacheTag = $""{cacheTag}"";
                System.Diagnostics.Debug.WriteLine($""Invalidating Cache Tag:{{cacheTag}}"");
                await cache.RemoveByTagAsync(cacheTag.GenerateCompactCacheKey(), ct);
                "
                }
            ];
        }

        return result;
    }

    private static InterfaceWriter CreateInvalidatorInterfaceWriter(this InterfaceDef cacheEnabledImplementedInterface) =>
    new InterfaceWriter
    {
        Name = $"{cacheEnabledImplementedInterface.Name}Invalidator",
        Interfaces = [TypeNameConstants.CacheInvalidatorInterface],
        MethodWriters = cacheEnabledImplementedInterface.MethodDefs
            .Where(m => m.HasResult())
            .Where(m => !m.IgnoreMethod())
            .SelectMany(m => m.CreateInvalidatorInterfaceMethodWriter())
    };

    private static IEnumerable<InterfaceMethodWriter> CreateInvalidatorInterfaceMethodWriter(this MethodDef methodDef)
    {
        var arguments = methodDef.Parameters
            .Where(p => !p.IsCancelationToken())
            .Select(p => p.Definition);
        InterfaceMethodWriter[] result =
        [
            new InterfaceMethodWriter
            {
                Name = $"Invalidate{methodDef.Name}",
                ReturnType = TypeNameConstants.TaskType,
                Arguments = 
                [
                    .. arguments, 
                    $"{TypeNameConstants.CancelationTokenType} ct = default"
                ]
            }
        ];
        if (arguments.Any())
        {
            result =
            [
                .. result,
                new InterfaceMethodWriter
                {
                    Name = $"Invalidate{methodDef.Name}",
                    ReturnType = TypeNameConstants.TaskType,
                    Arguments = [$"{TypeNameConstants.CancelationTokenType} ct = default"]
                }
            ];
        }
        return result;
    }
}
