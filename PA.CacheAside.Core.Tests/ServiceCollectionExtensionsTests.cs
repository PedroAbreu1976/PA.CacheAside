using Microsoft.Extensions.DependencyInjection;
using PA.CacheAside.Core;
using System.Threading;
using System.Threading.Tasks;

namespace PA.CacheAside.Core.Tests;

public class ServiceCollectionExtensionsTests
{
    [Fact]
    public void AddCacheAside_RegistersDecoratorAndInvalidator_FromCallingAssembly()
    {
        var services = new ServiceCollection();
        services.AddSingleton<IWidgetService, WidgetService>();
        services.AddSingleton<IPlainWidgetService, PlainWidgetService>();

        services.AddCacheAside();

        using var provider = services.BuildServiceProvider();

        var widgetService = provider.GetRequiredService<IWidgetService>();
        var invalidator = provider.GetRequiredService<ICacheInvalidator>();
        var plainWidgetService = provider.GetRequiredService<IPlainWidgetService>();

        Assert.IsType<WidgetServiceCache>(widgetService);
        Assert.Same(widgetService, invalidator);
        Assert.Equal("inner", widgetService.GetValue());
        Assert.Same(plainWidgetService, provider.GetRequiredService<IPlainWidgetService>());
        Assert.Equal("plain", plainWidgetService.GetValue());
    }

    [Fact]
    public void AddCacheAside_RegistersDecoratorAndInvalidator_FromExplicitAssemblies()
    {
        var services = new ServiceCollection();
        services.AddSingleton<IWidgetService, WidgetService>();

        var result = services.AddCacheAside([typeof(WidgetServiceCache).Assembly]);

        using var provider = services.BuildServiceProvider();

        var widgetService = provider.GetRequiredService<IWidgetService>();
        var invalidator = provider.GetRequiredService<ICacheInvalidator>();

        Assert.Same(services, result);
        Assert.IsType<WidgetServiceCache>(widgetService);
        Assert.Same(widgetService, invalidator);
        Assert.Equal("inner", widgetService.GetValue());
    }

    private interface IWidgetService
    {
        string GetValue();
    }

    private sealed class WidgetService : IWidgetService
    {
        public string GetValue() => "inner";
    }

    private sealed class WidgetServiceCache(IWidgetService inner) : IWidgetService, ICacheInvalidator
    {
        public string GetValue() => inner.GetValue();

        public Task InvalidateAsync(CancellationToken ct) => Task.CompletedTask;
    }

    private interface IPlainWidgetService
    {
        string GetValue();
    }

    private sealed class PlainWidgetService : IPlainWidgetService
    {
        public string GetValue() => "plain";
    }
}
