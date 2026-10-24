using PA.CacheAside.Core;

namespace Demo.Models;

public class Book : ICacheKeyProvider
{
    public Guid Id { get; set; }
    public Author Author { get; set; }
    public DateTime PublishDate { get; set; }
    public string Title { get; set; }

    public string GetCacheKey() => Id.ToString();
}
