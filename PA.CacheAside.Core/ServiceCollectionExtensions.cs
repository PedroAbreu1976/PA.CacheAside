using Microsoft.Extensions.Caching.Hybrid;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Linq;
using System.Reflection;

namespace PA.CacheAside.Core;

public static class ServiceCollectionExtensions
{
    public static IHybridCacheBuilder AddCacheAsideAndHybridCache(
        this IServiceCollection service,
        Assembly[] assemblies = null)
    {
        assemblies = assemblies ?? [Assembly.GetCallingAssembly()];
        service.AddCacheAside(assemblies);
        return service.AddHybridCache();
    }

    public static IHybridCacheBuilder AddCacheAsideAndHybridCache(
        this IServiceCollection service, 
        Action<HybridCacheOptions> setupAction,
        Assembly[] assemblies = null)
    {
        assemblies = assemblies ?? [Assembly.GetCallingAssembly()];
        service.AddCacheAside(assemblies);
        return service.AddHybridCache(setupAction);
    }

    public static IServiceCollection AddCacheAside(this IServiceCollection services, Assembly[] assemblies = null)
    {
        assemblies = assemblies ?? new[] { Assembly.GetCallingAssembly() };
        foreach (var assembly in assemblies)
        {
            var intefaceTypesWithCacheInterfaceAttribute = assembly.GetTypes()
                .Where(t => t.IsInterface)
                .Where(t => t.GetCustomAttributes(typeof(CacheInterfaceAttribute), false).Any())
                .ToList();

            assembly.GetTypes()
                .Where(t => t.IsClass)
                .Where(t => !t.IsAbstract)
                .Where(t => t.GetInterfaces().Any(i => typeof(ICacheInvalidator).IsAssignableFrom(i)))
                .ToList()
                .ForEach(type =>
                {
                    var serviceType = type.GetInterfaces().FirstOrDefault(i => !typeof(ICacheInvalidator).IsAssignableFrom(i));
                    var invalidatorType = type.GetInterfaces().FirstOrDefault(i => typeof(ICacheInvalidator).IsAssignableFrom(i));
                    if (serviceType != null && invalidatorType != null)
                    {
                        services.Decorate(serviceType, type);
                        services.AddScoped(invalidatorType, sp => (ICacheInvalidator)sp.GetRequiredService(serviceType));
                    }
                });
        }
        return services;
    }
}
