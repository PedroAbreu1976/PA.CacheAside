using Demo.Models;
using PA.CacheAside.Core;

namespace Demo.Services;

[CacheInterface]
public interface IBookService
{
    Task<IEnumerable<Book>> GetAllAsync(CancellationToken ct = default);
    
    Task<Book> GetByIdAsync(Guid id, CancellationToken ct = default);

    [CacheTimeout(1)]
    [CacheFlags(Microsoft.Extensions.Caching.Hybrid.HybridCacheEntryFlags.DisableDistributedCache)]
    Task<IEnumerable<Book>> SearchAsync(BookSearchParameters parameters, CancellationToken ct = default);
    
    [CacheIgnore]
    Task<int> CountAsync(CancellationToken ct = default);

    Task<IEnumerable<Book>> GetByAuthorAsync(Author author, CancellationToken ct = default);
    
    Task AddAsync(Book book, CancellationToken ct = default);
    
    Task RemoveAsync(Guid id, CancellationToken ct = default);
    Task UpdateAsync(Book book, CancellationToken ct = default);
}
