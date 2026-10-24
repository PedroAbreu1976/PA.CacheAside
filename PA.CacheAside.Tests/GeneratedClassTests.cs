using Microsoft.Extensions.Caching.Hybrid;
using Microsoft.Extensions.DependencyInjection;
using PA.CacheAside.Core;
using PA.CacheAside.Tests.TestData;

namespace PA.CacheAside.Tests;

public class GeneratedClassTests
{
    [Fact]
    public void Should_ForwardPropertyAccess_ToSourceInstance()
    {
        var context = CreateGeneratedWrapper();

        context.Service.IsValid = true;
        _ = context.Service.IsValid;
        _ = context.Service.IsValidGetter;
        context.Service.IsValidSetter = false;

        Assert.Collection(
            context.Source.Actions,
            x => Assert.Equal("Set: IsValid(True)", x.ToString()),
            x => Assert.Equal("Get: IsValid()", x.ToString()),
            x => Assert.Equal("Get: IsValidGetter()", x.ToString()),
            x => Assert.Equal("Set: IsValidSetter(False)", x.ToString()));
    }

    [Fact]
    public async Task Should_CacheAsyncResults_PerArgumentValues()
    {
        var context = CreateGeneratedWrapper();

        _ = await context.Service.GetByIdAsync(1, CancellationToken.None);
        _ = await context.Service.GetByIdAsync(1, CancellationToken.None);
        _ = await context.Service.GetByIdAsync(2, CancellationToken.None);
        _ = await context.Service.GetByIdAsync(2, CancellationToken.None);

        Assert.Equal(1, CountActions(context.Source, nameof(RepositoryForCaching.GetByIdAsync), 1));
        Assert.Equal(1, CountActions(context.Source, nameof(RepositoryForCaching.GetByIdAsync), 2));
    }

    [Fact]
    public void Should_CacheSyncResults_UntilMethodSpecificInvalidation()
    {
        var context = CreateGeneratedWrapper();

        _ = context.Service.GetAll().ToArray();
        _ = context.Service.GetAll().ToArray();
        InvokeInvalidatorAsync(context.Instance, "InvalidateGetAll", CancellationToken.None).GetAwaiter().GetResult();
        _ = context.Service.GetAll().ToArray();

        Assert.Equal(2, CountActions(context.Source, nameof(RepositoryForCaching.GetAll)));
    }

    [Fact]
    public async Task Should_InvalidateOnlyTheMatchingAsyncCacheEntry()
    {
        var context = CreateGeneratedWrapper();

        _ = await context.Service.GetByIdAsync(1, CancellationToken.None);
        _ = await context.Service.GetByIdAsync(2, CancellationToken.None);
        await InvokeInvalidatorAsync(context.Instance, "InvalidateGetByIdAsync", 1, CancellationToken.None);
        _ = await context.Service.GetByIdAsync(1, CancellationToken.None);
        _ = await context.Service.GetByIdAsync(2, CancellationToken.None);

        Assert.Equal(2, CountActions(context.Source, nameof(RepositoryForCaching.GetByIdAsync), 1));
        Assert.Equal(1, CountActions(context.Source, nameof(RepositoryForCaching.GetByIdAsync), 2));
    }

    [Fact]
    public async Task Should_InvalidateAllInterfaceCacheEntries_WhenUsingGeneralInvalidator()
    {
        var context = CreateGeneratedWrapper();

        _ = await context.Service.GetAllAsync(CancellationToken.None);
        _ = await context.Service.GetByIdAsync(1, CancellationToken.None);
        await ((ICacheInvalidator)context.Instance).InvalidateAsync(CancellationToken.None);
        _ = await context.Service.GetAllAsync(CancellationToken.None);
        _ = await context.Service.GetByIdAsync(1, CancellationToken.None);

        Assert.Equal(2, CountActions(context.Source, nameof(RepositoryForCaching.GetAllAsync)));
        Assert.Equal(2, CountActions(context.Source, nameof(RepositoryForCaching.GetByIdAsync), 1));
    }

    [Fact]
    public async Task Should_NotCacheIgnoredOrVoidReturningMethods()
    {
        var context = CreateGeneratedWrapper();

        await context.Service.Create("X", CancellationToken.None);
        await context.Service.Create("X", CancellationToken.None);
        _ = await context.Service.GetFirstAsync(CancellationToken.None);
        _ = await context.Service.GetFirstAsync(CancellationToken.None);

        Assert.Equal(2, CountActions(context.Source, nameof(RepositoryForCaching.Create), "X"));
        Assert.Equal(2, CountActions(context.Source, nameof(RepositoryForCaching.GetFirstAsync)));
    }

    private static int CountActions(RepositoryForCaching source, string method, params object[] args)
    {
        return source.Actions.Count(x =>
            x.Method == method &&
            x.Args.SequenceEqual(args));
    }

    private static async Task InvokeInvalidatorAsync(IRepositoryForCachingCache instance, string methodName, params object[] args)
    {
        var method = instance.GetType().GetMethod(methodName);

        Assert.NotNull(method);

        var result = method!.Invoke(instance, args);
        var task = Assert.IsAssignableFrom<Task>(result);

        await task;
    }

    private static GeneratedWrapperContext CreateGeneratedWrapper()
    {
        var source = new RepositoryForCaching();
        var cache = CreateHybridCache();
        var cacheKeyFactory = new CacheKeyFactory();
        var instance = new IRepositoryForCachingCache(source, cache, cacheKeyFactory);

        Assert.NotNull(instance);

        return new GeneratedWrapperContext(source, (IRepositoryForCaching)instance!, instance!);
    }

    private static HybridCache CreateHybridCache()
    {
        var services = new ServiceCollection();
        services.AddMemoryCache();
        services.AddHybridCache();

        var provider = services.BuildServiceProvider();
        return provider.GetRequiredService<HybridCache>();
    }

    private sealed record GeneratedWrapperContext(
        RepositoryForCaching Source,
        IRepositoryForCaching Service,
        IRepositoryForCachingCache Instance);
}
