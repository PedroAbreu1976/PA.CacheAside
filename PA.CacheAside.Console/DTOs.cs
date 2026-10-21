
namespace PA.CacheAside.Demo.Aot;

public class GetAllResponse
{
    public IEnumerable<string> Items { get; set; } = Enumerable.Empty<string>();
}

public class ContainsRequest 
{
    public string Text { get; set; } = string.Empty;

    public override string ToString() => Text;
}
