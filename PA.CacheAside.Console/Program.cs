using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Demo = PA.CacheAside.Demo.Aot;

var builder = Host.CreateDefaultBuilder();

builder.ConfigureServices((hostContext, services) => {
    
    services.AddScoped<Demo.IRepository, Demo.Repository>();
    services.AddCacheAside()
        .WithHybridCache()
        .LoadCacheKeyGeneratorsFromAssemblies(new[] { typeof(Demo.ContainsRequest).Assembly });
    //.WithAotSerialization(() => Demo.CacheAsideJsonContext.Default)
    //.ConfigureCacheKeyFactory(s =>
    //{
    //    s.RegisterCacheKeyProvider<Demo.ContainsRequest>();
    //});
});

var app = builder.Build();

var repository = app.Services.GetRequiredService<Demo.IRepository>();
var repositoryCacheInvalidator = app.Services.GetRequiredService<Demo.IRepositoryInvalidator>();

await repository.Add("Item 1", CancellationToken.None);
await repository.Add("Item 2", CancellationToken.None);
await repository.Add("Item 3", CancellationToken.None);

await GetItemsAsync("Initial fetch of items (should hit repository):");

await GetItemsAsync("Second fetch of items (should not hit repository)");

await repositoryCacheInvalidator.InvalidateContainsAsync(new Demo.ContainsRequest { Text = "Item" }, CancellationToken.None);
await repositoryCacheInvalidator.InvalidateGetAllAsync();
await GetItemsAsync(
    "Third fetch after invalidation of GetAllAsync() and ContainsAsync(\"Item\")",
    "(should hit repository again just for fetching all items and items containing 'Item')");

await repositoryCacheInvalidator.InvalidateAsync(CancellationToken.None);
await GetItemsAsync("Fourth fetch after invalidation of all methods (should hit repository again for all fetches)");

await app.RunAsync();

async Task GetItemsAsync(params string[] descriptions)
{
    foreach (var description in descriptions)
        Console.WriteLine($"=> {description}");
    Console.WriteLine("===================================");

    Console.WriteLine("CACHE: Fetching all items: ");
    var allItems = await repository.GetAllAsync();
    Console.WriteLine($"Result: {string.Join(", ", allItems.Items)}");
    Console.WriteLine("----------------------------------");

    var request = new Demo.ContainsRequest { Text = "Item" };
    Console.WriteLine("CACHE: Fetching if any item contains 'Item': ");
    Console.WriteLine("Item 1 HasCode: " + request.GetCacheKey());
    var itemsContainingItem = await repository.ContainsAsync(request, CancellationToken.None);
    Console.WriteLine($"Result: {itemsContainingItem}");        

    Console.WriteLine("----------------------------------");

    request = new Demo.ContainsRequest { Text = "Hello" };
    Console.WriteLine("CACHE: Fetching if any item contains 'Hello': ");
    Console.WriteLine("Hello HasCode: " + request.GetCacheKey());
    var itemsContainingHello = await repository.ContainsAsync(request, CancellationToken.None);
    Console.WriteLine($"Result: {itemsContainingHello}");

    Console.WriteLine("----------------------------------");

    var itemsToCheck = new List<string> { "Item 1", "Item 4" };
    Console.WriteLine("CACHE: Fetching if any item contains \"Item 1\" or \"Item 4\": ");
    var itemsContaining = await repository.ContainsAny(itemsToCheck, CancellationToken.None);
    Console.WriteLine($"Result: {itemsContaining}");
    Console.WriteLine("===================================");
    Console.WriteLine();

}

