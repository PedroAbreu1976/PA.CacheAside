using Microsoft.Extensions.DependencyInjection;
using System;
using System.Linq;
using System.Reflection;

namespace PA.CacheAside.Core;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddCacheAside(this IServiceCollection services)
    {
        return services.AddCacheAside([Assembly.GetCallingAssembly()]);
    }

    public static IServiceCollection AddCacheAside(this IServiceCollection services, Assembly[] assemblies)
    {
         

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
