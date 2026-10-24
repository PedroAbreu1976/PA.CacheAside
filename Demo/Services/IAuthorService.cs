using Demo.Models;
using PA.CacheAside.Core;

namespace Demo.Services;

[CacheInterface]
public interface IAuthorService
{
    Task<IEnumerable<Author>> GetAllAsync(CancellationToken ct = default);

    Task<Author> GetByIdAsync(int id, CancellationToken ct = default);

    [CacheTimeout(1)]
    [CacheFlags(Microsoft.Extensions.Caching.Hybrid.HybridCacheEntryFlags.DisableDistributedCache)]
    Task<IEnumerable<Author>> SearchAsync(AuthorSearchParameters parameters, CancellationToken ct = default);

    Task AddAsync(Author author, CancellationToken ct = default);

    Task RemoveAsync(int id, CancellationToken ct = default);
    Task UpdateAsync(Author author, CancellationToken ct = default);
}
