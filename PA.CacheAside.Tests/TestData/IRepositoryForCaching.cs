using PA.CacheAside.Core;

namespace PA.CacheAside.Tests.TestData;

[CacheInterface]
public interface IRepositoryForCaching
{
    [CacheTag("TEST")]
    Task<IEnumerable<string>> GetAllAsync(CancellationToken ct);
    Task<string> GetByIdAsync(int id, CancellationToken token);

    [CacheTimeout(2)]
    IEnumerable<string> GetAll();

    Task Create(string item, CancellationToken ct);

    [CacheIgnore]
    Task<string> GetFirstAsync(CancellationToken token);

    public bool IsValid { get; set; }

    public bool IsValidGetter { get; }
    public bool IsValidSetter { set; }
}
