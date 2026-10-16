# PA.CacheAside

[NuGet package](https://www.nuget.org/packages/PA.CacheAside/1.0.0)

PA.CacheAside is a small cache-aside helper for .NET that uses a Roslyn source generator to create cached decorators for your interfaces.

![NuGet Version](https://img.shields.io/nuget/v/PA.CacheAside)


Instead of hand-writing wrapper classes for every query service or repository, you mark an interface with an attribute and let the generator create:

- a cached implementation of the interface
- a matching invalidator interface for cache eviction
- DI registration support to decorate your existing implementation

## What problem does it solve?

Many applications use the cache-aside pattern for read-heavy services:

1. check the cache
2. if the value is missing, call the real implementation
3. store the result in the cache
4. invalidate cached values when data changes

The pattern is useful, but the boilerplate is repetitive:

- building cache keys
- forwarding every method and property
- wiring decorators into DI
- adding targeted invalidation methods
- keeping cache code consistent across services

PA.CacheAside removes most of that manual work by generating the wrapper for you.

## How it works

You define an interface and decorate it with `[CacheInterface]`.

The source generator creates a class named `<YourInterfaceName>Cache` that:

- implements your original interface
- calls the original service when a cache entry does not exist
- stores results in `HybridCache`
- forwards property access to the original implementation
- generates invalidation methods for cached methods

It also creates an invalidator interface named `<YourInterfaceName>Invalidator`.

## Project structure

- `PA.CacheAside.Core` - attributes, invalidation contract, and DI extensions
- `PA.CacheAside` - Roslyn source generator
- `PA.CacheAside.Console` - sample app showing usage
- `PA.CacheAside.Core.Tests` / `PA.CacheAside.Tests` - tests for registration and generated code

## Features

- source-generated cache decorators
- integration with `Microsoft.Extensions.Caching.Hybrid`
- automatic DI decoration with `AddCacheAside()`
- per-method cache invalidation
- full interface invalidation through `ICacheInvalidator`
- support for async and sync methods with return values
- support for parameter-based cache keys
- optional method exclusion with `[CacheIgnore]`
- optional cache tags with `[CacheTag]`
- optional cache expiration with `[CacheTimeout]`
- optional local cache expiration with `[CacheLocalTimeout]`
- optional HybridCache entry flags with `[CacheFlags]`

## When caching is applied

Caching is generated for methods that:

- belong to an interface marked with `[CacheInterface]`
- return a value
- are not marked with `[CacheIgnore]`

Methods without a return value are forwarded directly and are not cached.

## Quick start

### 1. Reference the projects

If you are using this repository directly, reference the runtime project and the generator project from your application:

```xml
<ItemGroup>
  <ProjectReference Include="..\PA.CacheAside.Core\PA.CacheAside.Core.csproj" />
  <ProjectReference Include="..\PA.CacheAside\PA.CacheAside.csproj"
                    OutputItemType="Analyzer"
                    ReferenceOutputAssembly="false" />
</ItemGroup>
```

You also need `HybridCache` in your application:

```xml
<ItemGroup>
  <PackageReference Include="Microsoft.Extensions.Caching.Hybrid" Version="10.10.0" />
</ItemGroup>
```

### 2. Mark an interface for caching

```csharp
using PA.CacheAside.Core;

[CacheInterface]
public interface IProductRepository
{
    Task<IReadOnlyList<Product>> GetAllAsync(CancellationToken ct);
    Task<Product?> GetByIdAsync(int id, CancellationToken ct);

    [CacheIgnore]
    Task SaveAsync(Product product, CancellationToken ct);
}
```

### 3. Implement the real service

```csharp
public sealed class ProductRepository : IProductRepository
{
    public Task<IReadOnlyList<Product>> GetAllAsync(CancellationToken ct)
        => Task.FromResult<IReadOnlyList<Product>>(new List<Product>());

    public Task<Product?> GetByIdAsync(int id, CancellationToken ct)
        => Task.FromResult<Product?>(null);

    public Task SaveAsync(Product product, CancellationToken ct)
        => Task.CompletedTask;
}
```

### 4. Register services

```csharp
using Microsoft.Extensions.DependencyInjection;
using PA.CacheAside.Core;

services.AddHybridCache();
services.AddScoped<IProductRepository, ProductRepository>();
services.AddCacheAside();
```

`AddCacheAside()` decorates matching services and also registers the generated invalidator interface.

### 5. Use the interface normally

```csharp
var repository = serviceProvider.GetRequiredService<IProductRepository>();

var products1 = await repository.GetAllAsync(CancellationToken.None); // hits source
var products2 = await repository.GetAllAsync(CancellationToken.None); // hits cache

var product1 = await repository.GetByIdAsync(10, CancellationToken.None); // hits source
var product2 = await repository.GetByIdAsync(10, CancellationToken.None); // hits cache
```

## Usage examples

### Example 1: Basic cached reads

```csharp
[CacheInterface]
public interface IWeatherService
{
    Task<Forecast> GetForecastAsync(string city, CancellationToken ct);
}
```

Generated behavior:

- `GetForecastAsync("London", ...)` caches one entry
- `GetForecastAsync("Paris", ...)` caches a different entry
- repeated calls with the same arguments reuse the cached value

### Example 2: Ignore write operations

```csharp
[CacheInterface]
public interface ICustomerService
{
    Task<Customer?> GetByIdAsync(Guid id, CancellationToken ct);

    [CacheIgnore]
    Task UpdateAsync(Customer customer, CancellationToken ct);
}
```

`UpdateAsync` is not cached and is forwarded directly to the underlying implementation.

### Example 3: Invalidate one cache entry

For cached methods, the generator creates method-specific invalidators.

```csharp
var invalidator = serviceProvider.GetRequiredService<IProductRepositoryInvalidator>();
await invalidator.InvalidateGetByIdAsync(10, CancellationToken.None);
```

That removes only the cache entry for `GetByIdAsync(10, ...)`.

### Example 4: Invalidate all cache entries for an interface

All generated decorators also implement `ICacheInvalidator`:

```csharp
var invalidator = serviceProvider.GetRequiredService<IProductRepositoryInvalidator>();
await invalidator.InvalidateAsync(CancellationToken.None);
```

That clears all cached entries associated with the interface.

### Example 5: Add cache tags

```csharp
[CacheInterface]
public interface IOrderQueries
{
    [CacheTag("orders", "dashboard")]
    Task<OrderSummary> GetSummaryAsync(CancellationToken ct);
}
```

Tags are attached to generated cache entries, and the interface name is also added as a tag to support broad invalidation.

### Example 6: Configure cache expiration and flags

```csharp
using Microsoft.Extensions.Caching.Hybrid;

[CacheInterface]
public interface IProductQueries
{
    [CacheTimeout(120)]
    [CacheLocalTimeout(10)]
    [CacheFlags(HybridCacheEntryFlags.DisableDistributedCache)]
    Task<IReadOnlyList<Product>> GetFeaturedAsync(CancellationToken ct);
}
```

This configures the generated cache entry with:

- a 120-minute overall expiration
- a 10-minute local cache expiration
- `HybridCache` entry flags for the cached method

## Generated members

For this interface:

```csharp
[CacheInterface]
public interface IRepository
{
    Task<string> GetByIdAsync(int id, CancellationToken ct);
}
```

The generator produces members conceptually like:

- `IRepositoryCache` - generated cached decorator class
- `IRepositoryInvalidator` - generated invalidator interface
- `InvalidateGetByIdAsync(int id, CancellationToken ct)` - targeted eviction
- `InvalidateAsync(CancellationToken ct)` - interface-wide eviction

## Running the sample

The repository includes a small console demo.

```powershell
dotnet run --project PA.CacheAside.Console
```

The sample shows:

- first call loading from the real repository
- second call loading from cache
- selective invalidation of specific entries
- full invalidation of all entries for the interface

## Notes and current behavior

- The current implementation uses `HybridCache` for storage.
- Cache keys are built from the full interface name, method name, and argument values.
- Property members are forwarded to the wrapped implementation.
- Cancellation tokens are passed through when present.
- Per-method cache options are supported through `[CacheTimeout]`, `[CacheLocalTimeout]`, and `[CacheFlags]`.

## Why use PA.CacheAside?

Use it when you want to:

- add caching without hand-writing decorators
- keep repositories and query services focused on business logic
- standardize cache invalidation patterns
- reduce copy-paste caching code
- adopt cache-aside with minimal ceremony

## License

See the repository license for details.
