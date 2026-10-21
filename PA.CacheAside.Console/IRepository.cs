using Microsoft.Extensions.Caching.Hybrid;
using PA.CacheAside.Core;

namespace PA.CacheAside.Demo.Aot
{
    [CacheInterface]
    public interface IRepository
    {
        [CacheTag("TEST")]
        [CacheTimeout(120)]
        [CacheLocalTimeout(10)]
        [CacheFlags(HybridCacheEntryFlags.DisableDistributedCache)]
        Task<GetAllResponse> GetAllAsync(CancellationToken? ct = default);

        GetAllResponse GetAll();

        Task<bool> ContainsAsync(ContainsRequest request, CancellationToken token = default);

        Task Add(string item, CancellationToken ct);

        [CacheIgnore]
        Task<string> Remove(string item, CancellationToken token);
    }

    public class Repository : IRepository
    {
        private readonly List<string> _items = new();

        public Task Add(string item, CancellationToken ct)
        {

            _items.Add(item);
            return Task.CompletedTask;
        }

        public Task<GetAllResponse> GetAllAsync(CancellationToken? ct = default)
        {
            System.Console.WriteLine("REPOSITORY: Fetching all items: ");
            var response = new GetAllResponse
            {
                Items = _items.AsEnumerable()
            };
            return Task.FromResult(response);
        }

        public GetAllResponse GetAll()
        {
            var response = new GetAllResponse
            {
                Items = _items.AsEnumerable()
            };
            return response;
        }

        public Task<bool> ContainsAsync(ContainsRequest request, CancellationToken token)
        {
            System.Console.WriteLine($"REPOSITORY: Fetching if any item contains '{request.Text}': ");
            var items = _items.Where(x => x.Contains(request.Text));
            return Task.FromResult(items.Any());
        }

        public Task<string> Remove(string item, CancellationToken token)
        {
            _items.Remove(item);
            return Task.FromResult(item);
        }
    }
}
