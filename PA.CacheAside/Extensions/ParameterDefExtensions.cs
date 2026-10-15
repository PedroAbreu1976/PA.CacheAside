using PA.CacheAside.Models;

namespace PA.CacheAside.Extensions;

internal static class ParameterDefExtensions
{
    public static bool IsCancelationToken(this ParameterDef parameterDef)
    {
        return parameterDef.TypeFullName == TypeNameConstants.CancelationTokenType;
    }
}
