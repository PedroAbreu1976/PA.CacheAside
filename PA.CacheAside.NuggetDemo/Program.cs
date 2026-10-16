using PA.CacheAside.Core;
using Microsoft.Extensions.Caching.Hybrid;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Demo = PA.CacheAside.Demo;

var builder = Host.CreateDefaultBuilder();


builder.ConfigureServices((hostContext, services) => {
    services.AddHybridCache();
    services.AddScoped<Demo.IRepository, Demo.Repository>();
    services.AddCacheAside();
});

var app = builder.Build();

var cache = app.Services.GetRequiredService<HybridCache>();
var repository = app.Services.GetRequiredService<Demo.IRepository>();
var invalidator = app.Services.GetRequiredService<Demo.IRepositoryInvalidator>();

await repository.Add("Item 1", CancellationToken.None);
await repository.Add("Item 2", CancellationToken.None);
await repository.Add("Hello", CancellationToken.None);

await GetItemsAsync("Initial fetch of items (should hit repository):");

await GetItemsAsync("Second fetch of items (should not hit repository)");

await invalidator.InvalidateGetContainingAsync("Item", CancellationToken.None);
await invalidator.InvalidateGetAllAsync(CancellationToken.None);
await GetItemsAsync(
    "Third fetch after invalidation of GetAllAsync() and GetContainingAsync(\"Item\")",
    "(should hit repository again just for fetching all items and items containing 'Item')");

await invalidator.InvalidateAsync(CancellationToken.None);
await GetItemsAsync("Fourth fetch after invalidation of all methods (should hit repository again for all fetches)");

await app.RunAsync();

async Task GetItemsAsync(params string[] descriptions)
{
    foreach (var description in descriptions)
        Console.WriteLine($"=> {description}");
    Console.WriteLine("===================================");

    Console.WriteLine("CACHE: Fetching all items: ");
    var allItems = await repository.GetAllAsync(CancellationToken.None);
    Console.WriteLine($"Result: {string.Join(", ", allItems)}");
    Console.WriteLine("----------------------------------");

    Console.WriteLine("CACHE: Fetching items containing 'Item': ");
    var itemsContainingItem = await repository.GetContainingAsync("Item", CancellationToken.None);
    Console.WriteLine($"Result: {string.Join(", ", itemsContainingItem)}");
    Console.WriteLine("----------------------------------");

    Console.WriteLine("CACHE: Fetching items containing 'Hello': ");
    var itemsContainingHello = await repository.GetContainingAsync("Hello", CancellationToken.None);
    Console.WriteLine($"Result: {string.Join(", ", itemsContainingHello)}");
    Console.WriteLine("===================================");
    Console.WriteLine();
}

namespace PA.CacheAside.Demo
{
    [CacheInterface]
    public interface IRepository
    {
        [CacheTag("TEST")]
        Task<IEnumerable<string>> GetAllAsync(CancellationToken ct);
        Task<IEnumerable<string>> GetContainingAsync(string text, CancellationToken token);

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

        public Task<IEnumerable<string>> GetAllAsync(CancellationToken ct)
        {
            Console.WriteLine("REPOSITORY: Fetching all items: ");
            return Task.FromResult(_items.AsEnumerable());
        }

        public Task<IEnumerable<string>> GetContainingAsync(string text, CancellationToken token)
        {
            Console.WriteLine($"REPOSITORY: Fetching items containing '{text}': ");
            var items = _items.Where(x => x.Contains(text));
            return Task.FromResult(items);
        }

        public Task<string> Remove(string item, CancellationToken token)
        {
            _items.Remove(item);
            return Task.FromResult(item);
        }
    }
}