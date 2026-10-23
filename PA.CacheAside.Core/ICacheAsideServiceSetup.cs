using Microsoft.Extensions.Caching.Hybrid;
using PA.CacheAside.Core;
using System;
using System.Collections.Generic;
using System.Reflection;

namespace Microsoft.Extensions.DependencyInjection
{
    /// <summary>
    /// Defines configuration for registering and customizing cache-aside services, including cache key generation,
    /// assembly-based generator discovery, AOT serialization, and hybrid cache integration.
    /// </summary>
    /// <remarks>Supports setup of cache-aside infrastructure during application composition and access to the
    /// underlying service collection for dependency injection registration.</remarks>
    public interface ICacheAsideServiceSetup
    {
        /// <summary>
        /// Configures cache key factory options for the cache-aside service.
        /// </summary>
        /// <param name="setup">Applies cache key factory configuration.</param>
        /// <returns>The current <c>CacheAsideServiceSetup</c> instance for chaining additional setup.</returns>
        CacheAsideServiceSetup ConfigureCacheKeyFactory(Action<ICacheKeyFactorySetup> setup);

        /// <summary>
        /// Gets the service collection used for dependency injection.
        /// </summary>
        /// <returns>An <see cref="IServiceCollection"/> containing the registered services.</returns>
        IServiceCollection GetServices();

        /// <summary>
        /// Loads cache key generator implementations <see cref="ICacheKeyProvider"/> from the specified assemblies.
        /// </summary>
        /// <remarks>Supports fluent configuration by returning the same setup instance.</remarks>
        /// <param name="assemblies">Assemblies to scan for cache key generator implementations.</param>
        /// <returns>The current setup instance.</returns>
        CacheAsideServiceSetup LoadCacheKeyGeneratorsFromAssemblies(IEnumerable<Assembly> assemblies);

        /// <summary>
        /// Configures the cache-aside setup to use an AOT-generated JSON serialization context.
        /// </summary>
        /// <typeparam name="TContext">Type of the AOT JSON serialization context.</typeparam>
        /// <param name="contextFactory">Factory that creates the serialization context instance.</param>
        /// <returns>The current service setup instance for method chaining.</returns>
        CacheAsideServiceSetup WithAotSerialization<TContext>(Func<TContext> contextFactory) where TContext : AotCacheAsideJsonContext;
        
        /// <summary>
        /// Configures hybrid caching for the cache-aside service setup.
        /// </summary>
        /// <param name="setupAction">Delegate used to configure <see cref="HybridCacheOptions"/>. If <see langword="null"/>, default options are
        /// used.</param>
        /// <returns>The current <see cref="CacheAsideServiceSetup"/> instance for method chaining.</returns>
        CacheAsideServiceSetup WithHybridCache(Action<HybridCacheOptions> setupAction = null);
    }
}