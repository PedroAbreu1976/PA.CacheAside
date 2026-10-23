using System;
using System.Collections.Generic;

namespace PA.CacheAside.Core;

public interface ICacheKeyFactorySetup
{
    void RegisterCacheKeyProvider<T>(Func<T, string> provider);
    void RegisterCacheKeyProvider<T>() where T : ICacheKeyProvider;
}

public interface ICacheKeyFactory : ICacheKeyFactorySetup
{
    ICacheKeyGenerator<T> GetCacheKeyGenerator<T>();
    string GetCacheKey<T>(T item);
    string GetCacheKey<T>(IEnumerable<T> items);
}

public class CacheKeyFactory : ICacheKeyFactorySetup, ICacheKeyFactory
{
    private readonly Dictionary<Type, object> _cacheKeyGenerators = new();
    public CacheKeyFactory()
    {
        RegisterCacheKeyGenerator(new BasicCacheKeyGenerator<string>());
        RegisterCacheKeyGenerator(new BasicCacheKeyGenerator<int>());
        RegisterCacheKeyGenerator(new BasicCacheKeyGenerator<int?>());
        RegisterCacheKeyGenerator(new BasicCacheKeyGenerator<bool>());
        RegisterCacheKeyGenerator(new BasicCacheKeyGenerator<bool?>());
        RegisterCacheKeyGenerator(new BasicCacheKeyGenerator<char>());
        RegisterCacheKeyGenerator(new BasicCacheKeyGenerator<char?>());
        RegisterCacheKeyGenerator(new BasicCacheKeyGenerator<DateTime>());
        RegisterCacheKeyGenerator(new BasicCacheKeyGenerator<DateTime?>());
        RegisterCacheKeyGenerator(new BasicCacheKeyGenerator<Guid>());
        RegisterCacheKeyGenerator(new BasicCacheKeyGenerator<Guid?>());
        RegisterCacheKeyGenerator(new BasicCacheKeyGenerator<sbyte>());
        RegisterCacheKeyGenerator(new BasicCacheKeyGenerator<sbyte?>());
        RegisterCacheKeyGenerator(new BasicCacheKeyGenerator<byte>());
        RegisterCacheKeyGenerator(new BasicCacheKeyGenerator<byte?>());
        RegisterCacheKeyGenerator(new BasicCacheKeyGenerator<short>());
        RegisterCacheKeyGenerator(new BasicCacheKeyGenerator<short?>());
        RegisterCacheKeyGenerator(new BasicCacheKeyGenerator<ushort>());
        RegisterCacheKeyGenerator(new BasicCacheKeyGenerator<ushort?>());
        RegisterCacheKeyGenerator(new BasicCacheKeyGenerator<uint>());
        RegisterCacheKeyGenerator(new BasicCacheKeyGenerator<uint?>());
        RegisterCacheKeyGenerator(new BasicCacheKeyGenerator<long>());
        RegisterCacheKeyGenerator(new BasicCacheKeyGenerator<long?>());
        RegisterCacheKeyGenerator(new BasicCacheKeyGenerator<ulong>());
        RegisterCacheKeyGenerator(new BasicCacheKeyGenerator<ulong?>());
        RegisterCacheKeyGenerator(new BasicCacheKeyGenerator<nint>());
        RegisterCacheKeyGenerator(new BasicCacheKeyGenerator<nint?>());
        RegisterCacheKeyGenerator(new BasicCacheKeyGenerator<nuint>());
        RegisterCacheKeyGenerator(new BasicCacheKeyGenerator<nuint?>());
        RegisterCacheKeyGenerator(new BasicCacheKeyGenerator<float>());
        RegisterCacheKeyGenerator(new BasicCacheKeyGenerator<float?>());
        RegisterCacheKeyGenerator(new BasicCacheKeyGenerator<double>());
        RegisterCacheKeyGenerator(new BasicCacheKeyGenerator<double?>());
        RegisterCacheKeyGenerator(new BasicCacheKeyGenerator<decimal>());
        RegisterCacheKeyGenerator(new BasicCacheKeyGenerator<decimal?>());
    }

    public void RegisterCacheKeyGenerator<T>(ICacheKeyGenerator<T> cacheKeyGenerator)
    {
        _cacheKeyGenerators.Add(cacheKeyGenerator.Type, cacheKeyGenerator);
    }

    public void RegisterCacheKeyProvider<T>(Func<T, string> provider)
    {
        _cacheKeyGenerators.Add(typeof(T), new DynamicCacheKeyGenerator<T>(provider));
    }

    public void RegisterCacheKeyProvider<T>()
        where T : ICacheKeyProvider
    {
        _cacheKeyGenerators.Add(typeof(T), new DynamicCacheKeyGenerator<T>(x => x.GetCacheKey()));
    }

    public ICacheKeyGenerator<T> GetCacheKeyGenerator<T>()
    {
        if (_cacheKeyGenerators.TryGetValue(typeof(T), out var generator))
        {
            return (ICacheKeyGenerator<T>)generator;
        }
        throw new InvalidOperationException($"No cache key generator registered for type {typeof(T).FullName}");
    }

    public string GetCacheKey<T>(T item)
    {
        var generator = GetCacheKeyGenerator<T>();
        return generator.GenerateCacheKey(item);
    }

    public string GetCacheKey<T>(IEnumerable<T> items)
    {
        var generator = GetCacheKeyGenerator<T>();
        return generator.GenerateCacheKey(items);
    }
}

