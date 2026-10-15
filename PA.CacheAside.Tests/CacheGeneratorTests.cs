using Microsoft.Extensions.Caching.Hybrid;
using Microsoft.Extensions.DependencyInjection;
using PA.CacheAside.Core;
using PA.CacheAside.Tests.TestData;
using System.Reflection;

namespace PA.CacheAside.Tests;

public class CacheGeneratorTests
{
    [Fact]
    public void Should_GenerateCacheClass_OnlyForInterfacesWithCacheInterfaceAttribute()
    {
        // Verify that source generation occurs only when an interface is decorated with CacheInterfaceAttribute.
        var generatedType = GetGeneratedType(nameof(IRepositoryForCaching));

        Assert.NotNull(generatedType);
    }

    [Fact]
    public void Should_NotGenerateCacheClass_ForInterfacesWithoutCacheInterfaceAttribute()
    {
        // Verify that no cache wrapper source is generated when CacheInterfaceAttribute is missing.
        var generatedType = GetGeneratedType(nameof(IRepositoryNotForCaching));

        Assert.Null(generatedType);
    }

    [Fact]
    public void Should_GenerateCacheClassWithExpectedName_AndImplementedInterfaces()
    {
        // Verify that the generated class name follows the "<InterfaceName>Cache" convention and implements both the source interface and invalidator interface.
        var generatedType = GetGeneratedType(nameof(IRepositoryForCaching));

        Assert.NotNull(generatedType);
        Assert.Equal("IRepositoryForCachingCache", generatedType.Name);
        Assert.Contains(typeof(IRepositoryForCaching), generatedType.GetInterfaces());
        Assert.Contains(typeof(ICacheInvalidator), generatedType.GetInterfaces());
    }

    [Fact]
    public void Should_GeneratePropertyForwarders_ForInterfaceProperties()
    {
        // Verify that interface properties are generated as pass-through members that delegate get/set operations to the source instance.
        var source = new RepositoryForCaching();
        var sut = CreateGeneratedWrapper(source);

        sut.IsValid = true;
        _ = sut.IsValid;
        _ = sut.IsValidGetter;
        sut.IsValidSetter = false;

        Assert.Collection(
            source.Actions,
            x => Assert.Equal("Set: IsValid(True)", x.ToString()),
            x => Assert.Equal("Get: IsValid()", x.ToString()),
            x => Assert.Equal("Get: IsValidGetter()", x.ToString()),
            x => Assert.Equal("Set: IsValidSetter(False)", x.ToString()));
    }

    [Fact]
    public async Task Should_GenerateMethodWithoutCaching_WhenMethodHasNoResult()
    {
        // Verify that methods with no return value bypass caching and directly invoke the source method.
        var source = new RepositoryForCaching();
        var sut = CreateGeneratedWrapper(source);

        await sut.Create("X", CancellationToken.None);
        await sut.Create("X", CancellationToken.None);

        Assert.Equal(2, source.Actions.Count(x => x.Method == nameof(RepositoryForCaching.Create)));
    }

    [Fact]
    public async Task Should_GenerateMethodWithoutCaching_WhenMethodIsMarkedToBeIgnored()
    {
        // Verify that methods marked to skip caching call the source directly and do not use cache APIs.
        var source = new RepositoryForCaching();
        var sut = CreateGeneratedWrapper(source);

        await sut.GetFirstAsync(CancellationToken.None);
        await sut.GetFirstAsync(CancellationToken.None);

        Assert.Equal(2, source.Actions.Count(x => x.Method == nameof(RepositoryForCaching.GetFirstAsync)));
    }

    [Fact]
    public async Task Should_GenerateAsyncCachingBody_ForAsyncMethodsWithResult()
    {
        // Verify that async methods with results are generated with cache.GetOrCreateAsync and await source invocation.
        var source = new RepositoryForCaching();
        var sut = CreateGeneratedWrapper(source);

        _ = await sut.GetByIdAsync(1, CancellationToken.None);
        _ = await sut.GetByIdAsync(1, CancellationToken.None);

        Assert.Equal(1, source.Actions.Count(x => x.Method == nameof(RepositoryForCaching.GetByIdAsync)));
    }

    [Fact]
    public void Should_GenerateSyncCachingBody_ForSyncMethodsWithResult()
    {
        // Verify that sync methods with results are generated with cache.GetOrCreateAsync using ValueTask.FromResult and synchronous result access.
        var source = new RepositoryForCaching();
        var sut = CreateGeneratedWrapper(source);

        _ = sut.GetAll().ToArray();
        _ = sut.GetAll().ToArray();

        Assert.Equal(1, source.Actions.Count(x => x.Method == nameof(RepositoryForCaching.GetAll)));
    }

    [Fact]
    public void Should_PassMethodCancellationToken_ToCacheGetOrCreate_WhenPresent()
    {
        // Verify that a method CancellationToken parameter is propagated to cache.GetOrCreateAsync when available.
        var generatedType = GetGeneratedType(nameof(IRepositoryForCaching));
        var method = generatedType!.GetMethod(nameof(IRepositoryForCaching.GetByIdAsync));

        Assert.NotNull(method);
        Assert.Contains(method!.GetParameters(), p => p.ParameterType == typeof(CancellationToken));
    }

    [Fact]
    public void Should_UseCancellationTokenNone_WhenMethodHasNoCancellationTokenParameter()
    {
        // Verify that CancellationToken.None is used when the method signature does not define a CancellationToken.
        var generatedType = GetGeneratedType(nameof(IRepositoryForCaching));
        var method = generatedType!.GetMethod(nameof(IRepositoryForCaching.GetAll));

        Assert.NotNull(method);
        Assert.DoesNotContain(method!.GetParameters(), p => p.ParameterType == typeof(CancellationToken));
    }

    [Fact]
    public async Task Should_AppendInterfaceNameToCacheTags_WhenGeneratingCachingCalls()
    {
        // Verify that generated cache tags include both method-declared tags and the interface full name.
        var source = new RepositoryForCaching();
        var sut = CreateGeneratedWrapper(source);

        _ = await sut.GetAllAsync(CancellationToken.None);
        var beforeInvalidate = source.Actions.Count(x => x.Method == nameof(RepositoryForCaching.GetAllAsync));

        await ((ICacheInvalidator)sut).InvalidateAsync(CancellationToken.None);
        _ = await sut.GetAllAsync(CancellationToken.None);
        var afterInvalidate = source.Actions.Count(x => x.Method == nameof(RepositoryForCaching.GetAllAsync));

        Assert.Equal(1, beforeInvalidate);
        Assert.Equal(2, afterInvalidate);
    }

    [Fact]
    public void Should_GenerateMethodSpecificInvalidator_ForEachCacheableMethod()
    {
        // Verify that an invalidation method is generated per cacheable method, excluding non-result and ignored methods.
        var generatedType = GetGeneratedType(nameof(IRepositoryForCaching));

        Assert.NotNull(generatedType);
        Assert.NotNull(generatedType!.GetMethod("InvalidateGetAllAsync", [typeof(CancellationToken)]));
        Assert.NotNull(generatedType.GetMethod("InvalidateGetByIdAsync", [typeof(int), typeof(CancellationToken)]));
        Assert.NotNull(generatedType.GetMethod("InvalidateGetAll", [typeof(CancellationToken)]));
        Assert.Null(generatedType.GetMethod("InvalidateCreate", BindingFlags.Public | BindingFlags.Instance));
        Assert.Null(generatedType.GetMethod("InvalidateGetFirstAsync", BindingFlags.Public | BindingFlags.Instance));
    }

    [Fact]
    public void Should_GenerateGeneralInvalidator_ForInterfaceCacheEntries()
    {
        // Verify that a general invalidator method is generated to invalidate cache entries for the interface scope.
        var generatedType = GetGeneratedType(nameof(IRepositoryForCaching));
        var invalidateMethod = generatedType!.GetMethod(nameof(ICacheInvalidator.InvalidateAsync), [typeof(CancellationToken)]);

        Assert.NotNull(invalidateMethod);
        Assert.Equal(typeof(Task), invalidateMethod!.ReturnType);
    }

    [Fact]
    public void Should_AddGeneratedSource_WithExpectedFileNamePattern()
    {
        // Verify that generated source is added with the expected "<InterfaceName>Cache.g.cs" file naming pattern.
        var generatedType = GetGeneratedType(nameof(IRepositoryForCaching));

        Assert.NotNull(generatedType);
        Assert.Equal("IRepositoryForCachingCache", generatedType!.Name);
    }

    private static IRepositoryForCaching CreateGeneratedWrapper(RepositoryForCaching source)
    {
        var generatedType = GetGeneratedType(nameof(IRepositoryForCaching));
        Assert.NotNull(generatedType);

        var cache = CreateHybridCache();
        var instance = Activator.CreateInstance(generatedType!, source, cache);

        Assert.NotNull(instance);
        return (IRepositoryForCaching)instance!;
    }

    private static Type? GetGeneratedType(string interfaceName)
    {
        var interfaceNamespace = typeof(IRepositoryForCaching).Namespace;
        var generatedTypeName = $"{interfaceNamespace}.{interfaceName}Cache";
        return typeof(IRepositoryForCaching).Assembly.GetType(generatedTypeName);
    }

    private static HybridCache CreateHybridCache()
    {
        var services = new ServiceCollection();
        services.AddMemoryCache();
        services.AddHybridCache();

        var provider = services.BuildServiceProvider();
        return provider.GetRequiredService<HybridCache>();
    }
}
