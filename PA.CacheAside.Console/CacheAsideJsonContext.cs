using PA.CacheAside.Core;
using System.Text.Json.Serialization;

namespace PA.CacheAside.Demo.Aot;

[JsonSerializable(typeof(GetAllResponse))]
[JsonSerializable(typeof(bool))]
public partial class CacheAsideJsonContext : AotCacheAsideJsonContext
{

}

