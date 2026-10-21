using System.Text.Json;
using System.Text.Json.Serialization;

namespace PA.CacheAside.Core;

public abstract class AotCacheAsideJsonContext(JsonSerializerOptions o) : JsonSerializerContext(o)
{

}
