using Microsoft.Extensions.Caching.Hybrid;
using System;

namespace PA.CacheAside.Core;

[AttributeUsage(AttributeTargets.Interface, Inherited = false, AllowMultiple = false)]
public class CacheInterfaceAttribute: Attribute
{
}

[AttributeUsage(AttributeTargets.Method, Inherited = false, AllowMultiple = false)]
public class CacheIgnoreAttribute(bool ignore = true) : Attribute
{
    public bool Ignore { get; } = ignore;
}

[AttributeUsage(AttributeTargets.Method, Inherited = false, AllowMultiple = false)]
public class CacheTimeoutAttribute(int minutes) : Attribute
{
    public TimeSpan Timeout { get; } = TimeSpan.FromMinutes(minutes);
}

[AttributeUsage(AttributeTargets.Method, Inherited = false, AllowMultiple = false)]
public class CacheLocalTimeoutAttribute(int minutes) : Attribute
{
    public TimeSpan Timeout { get; } = TimeSpan.FromMinutes(minutes);
}

[AttributeUsage(AttributeTargets.Method, Inherited = false, AllowMultiple = false)]
public class CacheFlagsAttribute(HybridCacheEntryFlags flags) : Attribute
{
    public HybridCacheEntryFlags Flags { get; } = flags;
}

[AttributeUsage(AttributeTargets.Method, Inherited = false, AllowMultiple = true)]
public class CacheTagAttribute(params string[] tags) : Attribute
{
    public string[] Tags { get; } = tags;
}
