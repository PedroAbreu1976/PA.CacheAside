namespace PA.CacheAside.Tests.TestData;

public interface IRepositoryNotForCaching
{
    Task<IEnumerable<string>> GetAllAsync(CancellationToken ct);
}
