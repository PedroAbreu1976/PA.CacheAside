using PA.CacheAside.Core;

namespace Demo.Models;

public class AuthorSearchParameters : ICacheKeyProvider
{
    public string? NameContains { get; set; }
    public int? MinimumAge { get; set; }
    public int? MaximumAge { get; set; }

    public string GetCacheKey() => 
        $"[{NameContains ?? "null"}, {MinimumAge?.ToString() ?? "null"}, {MaximumAge?.ToString() ?? "null"}]]";
}
