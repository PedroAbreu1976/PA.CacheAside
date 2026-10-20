using System.Threading;
using System.Threading.Tasks;

namespace PA.CacheAside.Core;

public interface ICacheInvalidator
{
    Task InvalidateAsync(CancellationToken ct = default);
}
