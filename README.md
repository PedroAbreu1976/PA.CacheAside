# PA.CacheAside

PA.CacheAside is a .NET source generator and cache wrapper library that turns a service interface into a cached implementation with automatic invalidation support.

It is designed for interfaces decorated with `CacheInterfaceAttribute`, and it generates a concrete cache-aware wrapper that delegates to your original implementation while caching method results and exposing invalidation methods for specific calls or the entire interface.

## What it does

- Generates a cache wrapper for interfaces annotated with `[CacheInterface]`
- Caches method results automatically based on the method and argument values
- Supports method-level invalidation via generated invalidator methods
- Supports interface-wide invalidation
- Lets specific methods opt out of caching with `[CacheIgnore]`
- Lets you tag cached methods with `[CacheTag]` and set custom timeout values with `[CacheTimeout]`
- Uses `Microsoft.Extensions.Caching.Hybrid` for the underlying cache storage

## Typical usage

```csharp
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Caching.Hybrid;
using PA.CacheAside.Core;

[CacheInterface]
public interface IRepository
{
    Task<IEnumerable<string>> GetAllAsync(CancellationToken ct);

    Task<IEnumerable<string>> GetContainingAsync(string text, CancellationToken ct);

    Task Add(string item, CancellationToken ct);

    [CacheIgnore]
    Task<string> Remove(string item, CancellationToken ct);
}

public class Repository : IRepository
{
    private readonly List<string> _items = new();

    public Task Add(string item, CancellationToken ct)
    {
        _items.Add(item);
        return Task.CompletedTask;
    }

    public Task<IEnumerable<string>> GetAllAsync(CancellationToken ct)
        => Task.FromResult(_items.AsEnumerable());

    public Task<IEnumerable<string>> GetContainingAsync(string text, CancellationToken ct)
        => Task.FromResult(_items.Where(x => x.Contains(text)).AsEnumerable());

    public Task<string> Remove(string item, CancellationToken ct)
    {
        _items.Remove(item);
        return Task.FromResult(item);
    }
}
```

Register the services in DI:

```csharp
var builder = Host.CreateDefaultBuilder();

builder.ConfigureServices((_, services) =>
{
    services.AddHybridCache();
    services.AddScoped<IRepository, Repository>();
    services.AddCacheAside();
});
```

Then resolve the generated cache wrapper and invalidator:

```csharp
var app = builder.Build();

var repository = app.Services.GetRequiredService<IRepository>();
var invalidator = app.Services.GetRequiredService<IRepositoryInvalidator>();

var all = await repository.GetAllAsync(CancellationToken.None);

await invalidator.InvalidateGetAllAsync(CancellationToken.None);
// or invalidate everything:
await invalidator.InvalidateAsync(CancellationToken.None);
```

## Attributes

### `CacheInterfaceAttribute`
Used on an interface to enable generation of the cache wrapper and invalidator support.

```csharp
[CacheInterface]
public interface IRepository
{
    // ...
}
```

### `CacheIgnoreAttribute`
Prevents a method from being cached even if it belongs to a cache-enabled interface.

```csharp
[CacheIgnore]
Task<string> Remove(string item, CancellationToken ct);
```

### `CacheTagAttribute`
Adds cache tags used when invalidating entries by tag or group.

```csharp
[CacheTag("TEST")]
Task<IEnumerable<string>> GetAllAsync(CancellationToken ct);
```

### `CacheTimeoutAttribute`
Sets a custom timeout for a cached method.

```csharp
[CacheTimeout(5)]
Task<IEnumerable<string>> GetDataAsync(CancellationToken ct);
```

## Notes

- Generated wrappers follow the naming pattern `<InterfaceName>Cache`.
- Generated invalidation methods are emitted for cacheable result-returning methods.
- The wrapper preserves interface contracts and forwards non-cached members directly to the underlying implementation.
- The library is designed to work with `HybridCache` from `Microsoft.Extensions.Caching.Hybrid`.

## Example project

The repository includes a sample console app under `PA.CacheAside.Console` that demonstrates the generated cache behavior and invalidation flow.
