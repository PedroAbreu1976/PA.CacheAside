using System.Collections.Generic;

namespace PA.CacheAside.Extensions;

internal static class StringExtensions
{
    public static string Join(this IEnumerable<string> source, string separator) => 
        string.Join(separator, source);
}
