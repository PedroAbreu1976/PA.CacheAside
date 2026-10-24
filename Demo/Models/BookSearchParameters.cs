using PA.CacheAside.Core;

namespace Demo.Models;

public class BookSearchParameters : ICacheKeyProvider
{
    public string? Title { get; set; }
    public string? AuthorName { get; set; }
    public DateTime? PublishedAfter { get; set; }
    public DateTime? PublishedBefore { get; set; }

    public string GetCacheKey() =>
        $"[{Title ?? "null"}, {AuthorName ?? "null"}, {PublishedAfter?.ToString() ?? "null"}, {PublishedBefore?.ToString() ?? "null"}]";
}
