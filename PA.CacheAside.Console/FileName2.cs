//using Microsoft.Extensions.Caching.Hybrid;

//namespace PA.CacheAside.Demo;


//public class IRepositoryCache2(global::PA.CacheAside.Demo.IRepository source, global::Microsoft.Extensions.Caching.Hybrid.HybridCache cache)
//{


//    public async global::System.Threading.Tasks.Task<global::System.Collections.Generic.IEnumerable<string>> GetAllAsync(global::System.Threading.CancellationToken? ct)
//    {

//        System.Diagnostics.Debug.WriteLine($"Fetching Cache Key:global::PA.CacheAside.Demo.IRepository.GetAllAsync({ct})");
//        var cacheKey = $"global::PA.CacheAside.Demo.IRepository.GetAllAsync({ct})";
//        return await cache.GetOrCreateAsideCacheAsync<IEnumerable<string>>(
//            cacheKey: cacheKey,
//            factory: async _ => await source.GetAllAsync(ct),
//            expirationMinutes: 120,
//            localExpirationMinutes: 10,
//            flags: (int)HybridCacheEntryFlags.DisableDistributedCache,
//            tags: ["TEST", "global::PA.CacheAside.Demo.IRepository"],
//            token: ct ?? CancellationToken.None);
//    }

//    public global::System.Collections.Generic.IEnumerable<string> GetAll()
//    {

//        System.Diagnostics.Debug.WriteLine($"Fetching Cache Key:global::PA.CacheAside.Demo.IRepository.GetAll()");
//        var cacheKey = $"global::PA.CacheAside.Demo.IRepository.GetAll()";
//        return cache.GetOrCreateAsideCache<IEnumerable<string>>(
//            cacheKey: cacheKey,
//            factory: () => source.GetAll(),
//            expirationMinutes: null,
//            localExpirationMinutes: null,
//            flags: null,
//            tags: ["global::PA.CacheAside.Demo.IRepository"]);
//    }

//    public async global::System.Threading.Tasks.Task<global::System.Collections.Generic.IEnumerable<string>> GetContainingAsync(string text, global::System.Threading.CancellationToken token)
//    {

//        System.Diagnostics.Debug.WriteLine($"Fetching Cache Key:global::PA.CacheAside.Demo.IRepository.GetContainingAsync({text})");
//        var cacheKey = $"global::PA.CacheAside.Demo.IRepository.GetContainingAsync({text})";
//        return await cache.GetOrCreateAsync(
//            key: cacheKey,
//            factory: async _ => await source.GetContainingAsync(text, token),
//            cancellationToken: token,
//            options: new global::Microsoft.Extensions.Caching.Hybrid.HybridCacheEntryOptions
//            {
//                Expiration = null,
//                LocalCacheExpiration = null,
//                Flags = null
//            },
//            tags: ["global::PA.CacheAside.Demo.IRepository"]);
//    }

//    public async global::System.Threading.Tasks.Task Add(string item, global::System.Threading.CancellationToken ct)
//    {

//        await source.Add(item, ct);
//    }

//    public async global::System.Threading.Tasks.Task<string> Remove(string item, global::System.Threading.CancellationToken token)
//    {

//        return await source.Remove(item, token);
//    }

//    public async global::System.Threading.Tasks.Task InvalidateGetAllAsync(global::System.Threading.CancellationToken ct)
//    {

//        System.Diagnostics.Debug.WriteLine($"Invalidating Cache Key:global::PA.CacheAside.Demo.IRepository.GetAllAsync({ct})");
//        var cacheKey = $"global::PA.CacheAside.Demo.IRepository.GetAllAsync({ct})";
//        await cache.RemoveAsync(cacheKey, ct);

//    }

//    public async global::System.Threading.Tasks.Task InvalidateGetAll(global::System.Threading.CancellationToken ct)
//    {

//        System.Diagnostics.Debug.WriteLine($"Invalidating Cache Key:global::PA.CacheAside.Demo.IRepository.GetAll()");
//        var cacheKey = $"global::PA.CacheAside.Demo.IRepository.GetAll()";
//        await cache.RemoveAsync(cacheKey, ct);

//    }

//    public async global::System.Threading.Tasks.Task InvalidateGetContainingAsync(string text, global::System.Threading.CancellationToken ct)
//    {

//        System.Diagnostics.Debug.WriteLine($"Invalidating Cache Key:global::PA.CacheAside.Demo.IRepository.GetContainingAsync({text})");
//        var cacheKey = $"global::PA.CacheAside.Demo.IRepository.GetContainingAsync({text})";
//        await cache.RemoveAsync(cacheKey, ct);

//    }

//    public async global::System.Threading.Tasks.Task InvalidateAsync(global::System.Threading.CancellationToken ct)
//    {

//        await cache.RemoveByTagAsync("global::PA.CacheAside.Demo.IRepository", ct);

//    }
//}

//public static class  HybridCacheExtensions
//{
//    public static ValueTask<T> GetOrCreateAsideCacheAsync<T>(
//        this HybridCache cache,
//        string cacheKey,
//        Func<CancellationToken, ValueTask<T>> factory,
//        int? expirationMinutes,
//        int? localExpirationMinutes,
//        int? flags,
//        string[] tags,
//        CancellationToken token) =>
//            cache.GetOrCreateAsync(
//                key: cacheKey,
//                factory: async _ => await factory(token),
//                cancellationToken: token,
//                options: GetOptions(expirationMinutes, localExpirationMinutes, flags),
//                tags: tags);
    

//    public static T GetOrCreateAsideCache<T>(
//        this HybridCache cache,
//        string cacheKey,
//        Func<T> factory,
//        int? expirationMinutes,
//        int? localExpirationMinutes,
//        int? flags,
//        string[] tags) =>
//            cache.GetOrCreateAsync(
//                key: cacheKey,
//                factory: _ => ValueTask.FromResult(factory()),
//                cancellationToken: CancellationToken.None,
//                options: GetOptions(expirationMinutes, localExpirationMinutes, flags),
//                tags: tags).Result;

//    private static HybridCacheEntryOptions GetOptions(int? expirationMinutes, int? localExpirationMinutes, int? flags) =>
//        new HybridCacheEntryOptions
//        {
//            Expiration = expirationMinutes.HasValue ? TimeSpan.FromMinutes(expirationMinutes.Value) : null,
//            LocalCacheExpiration = localExpirationMinutes.HasValue ? TimeSpan.FromMinutes(localExpirationMinutes.Value) : null,
//            Flags = flags.HasValue ? (HybridCacheEntryFlags?)flags.Value : null
//        };
//}
