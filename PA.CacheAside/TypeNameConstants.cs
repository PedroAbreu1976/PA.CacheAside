namespace PA.CacheAside;

internal static class TypeNameConstants
{
    private const string GlobalPrefix = "global::";
    private const string CoreNamespace = $"{GlobalPrefix}PA.CacheAside.Core";
    internal const string CacheInterfaceAttribute = $"{CoreNamespace}.CacheInterfaceAttribute";
    internal const string CacheIgnoreAttribute = $"{CoreNamespace}.CacheIgnoreAttribute";
    internal const string CacheTagAttribute = $"{CoreNamespace}.CacheTagAttribute";
    internal const string CacheTimeoutAttribute = $"{CoreNamespace}.CacheTimeoutAttribute";
    internal const string CacheLocalTimeoutAttribute = $"{CoreNamespace}.CacheLocalTimeoutAttribute";
    internal const string CacheFlagsAttribute = $"{CoreNamespace}.CacheFlagsAttribute";
    internal const string CacheInvalidatorInterface = $"{CoreNamespace}.ICacheInvalidator";
    internal const string CancelationTokenType = $"{GlobalPrefix}System.Threading.CancellationToken";
    //internal const string NullableCancelationTokenType = $"{GlobalPrefix}System.Threading.CancellationToken?";
    internal const string CancelationTokenNoneValue = $"{GlobalPrefix}System.Threading.CancellationToken.None";
    internal const string ValueTaskType = $"{GlobalPrefix}System.Threading.Tasks.ValueTask";
    internal const string TaskType = $"{GlobalPrefix}System.Threading.Tasks.Task";
    internal const string HybridCacheType = $"{GlobalPrefix}Microsoft.Extensions.Caching.Hybrid.HybridCache";
    internal const string HybridCacheEntryOptionsType = $"{GlobalPrefix}Microsoft.Extensions.Caching.Hybrid.HybridCacheEntryOptions";
    internal const string HybridCacheEntryFlagsType = $"{GlobalPrefix}Microsoft.Extensions.Caching.Hybrid.HybridCacheEntryFlags";
    internal const string HybridCacheOptionsType = $"{GlobalPrefix}Microsoft.Extensions.Caching.Hybrid.HybridCacheOptions";
    internal const string IHybridCacheBuilderType = $"{GlobalPrefix}Microsoft.Extensions.Caching.Hybrid.IHybridCacheBuilder";
    internal const string VoidTypeFullname = $"{GlobalPrefix}System.Void";
    internal const string VoidTypeSimplename = "void";
    internal const string IServiceCollectionType = $"{GlobalPrefix}Microsoft.Extensions.DependencyInjection.IServiceCollection";
    internal const string CacheAsideServiceSetupType = $"{GlobalPrefix}Microsoft.Extensions.DependencyInjection.CacheAsideServiceSetup";
    internal const string ICacheKeyFactoryType = $"{CoreNamespace}.ICacheKeyFactory";
}
