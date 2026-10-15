namespace PA.CacheAside.Tests.TestData;

internal class RepositoryNotForCaching : IRepositoryNotForCaching
{
    private readonly Dictionary<int, string> _items = new Dictionary<int, string>
    {
        {1, "A"},
        {2, "B"},
        {3, "C"}
    };

    public Task<IEnumerable<string>> GetAllAsync(CancellationToken ct)
    {
        return Task.FromResult((IEnumerable<string>)_items.Values);
    }
}
