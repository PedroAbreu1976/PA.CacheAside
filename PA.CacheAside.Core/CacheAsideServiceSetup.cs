using Microsoft.Extensions.Caching.Hybrid;
using PA.CacheAside.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Microsoft.Extensions.DependencyInjection;

public class CacheAsideServiceSetup : ICacheAsideServiceSetup
{
    private readonly CacheKeyFactory _cacheKeyFactory = new();
    private readonly IServiceCollection _services;

    public CacheAsideServiceSetup(IServiceCollection services)
    {
        this._services = services;
        _services.AddSingleton<ICacheKeyFactory>(_cacheKeyFactory);
    }

    public CacheAsideServiceSetup WithHybridCache(Action<HybridCacheOptions> setupAction = null)
    {
        if (setupAction is not null)
        {
            _services.AddHybridCache(setupAction);
        }
        else
        {
            _services.AddHybridCache();
        }
        return this;
    }

    public CacheAsideServiceSetup WithAotSerialization<TContext>(
        Func<TContext> contextFactory)
        where TContext : AotCacheAsideJsonContext
    {
        _services.AddSingleton<AotCacheAsideJsonContext>(_ => contextFactory());
        _services.AddSingleton<IHybridCacheSerializerFactory, AotCacheAsideSerializerFactory>();
        return this;
    }

    public CacheAsideServiceSetup ConfigureCacheKeyFactory(Action<ICacheKeyFactorySetup> setup)
    {
        setup(_cacheKeyFactory);
        return this;
    }

    public CacheAsideServiceSetup LoadCacheKeyGeneratorsFromAssemblies(IEnumerable<Assembly> assemblies)
    {
        var cacheKeyProviderTypes = assemblies
            .SelectMany(a => a.GetTypes())
            .Where(t => typeof(ICacheKeyProvider).IsAssignableFrom(t))
            .Where(t => !t.IsInterface)
            .Where(t => !t.IsAbstract);
        foreach (var type in cacheKeyProviderTypes)
        {
            var method = typeof(CacheKeyFactory)
                .GetMethod(nameof(CacheKeyFactory.RegisterCacheKeyProvider), [])?
                .MakeGenericMethod(type);
            method?.Invoke(_cacheKeyFactory, null);
        }
        return this;
    }

    public IServiceCollection GetServices()
    {
        return _services;
    }
}
