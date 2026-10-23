using System;
using System.Collections.Generic;
using System.Linq;

namespace PA.CacheAside.Core;

public interface ICacheKeyGenerator<T>
{
    Type Type { get; }
    string GenerateCacheKey(T item);
    string GenerateCacheKey(IEnumerable<T> items) =>
        $"[{string.Join(",", items.Select(x => GenerateCacheKey(x)))}]";
}

internal class BasicCacheKeyGenerator<T> : ICacheKeyGenerator<T>
{
    public Type Type => typeof(T);

    public string GenerateCacheKey(T item) => item as string ?? item?.ToString() ?? "-";
}

internal class DynamicCacheKeyGenerator<T>(Func<T,string> factory) : ICacheKeyGenerator<T>
{
    public Type Type => typeof(T);
    public string GenerateCacheKey(T item) => factory(item);
}

