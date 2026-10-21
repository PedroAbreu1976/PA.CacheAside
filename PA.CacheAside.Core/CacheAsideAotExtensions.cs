using Microsoft.Extensions.Caching.Hybrid;
using PA.CacheAside.Core;
using System;
using System.Text.Json.Serialization;

namespace Microsoft.Extensions.DependencyInjection;

public static class CacheAsideAotExtensions
{
    public static IServiceCollection AddAotCacheAsideSerialization<TContext>(
        this IServiceCollection services,
        Func<TContext> contextFactory)
        where TContext : AotCacheAsideJsonContext
    {
        services.AddSingleton<AotCacheAsideJsonContext>(_ => contextFactory());
        services.AddSingleton<IHybridCacheSerializerFactory, AotCacheAsideSerializerFactory>();
        return services;
    }
}
