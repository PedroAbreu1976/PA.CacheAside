using PA.CacheAside.Models;
using System;

namespace PA.CacheAside.Extensions;

internal static class ParameterDefExtensions
{
    public static bool IsCancelationToken(this ParameterDef parameterDef)
    {
        return 
            parameterDef.TypeFullName == TypeNameConstants.CancelationTokenType
            || parameterDef.TypeFullName == $"{TypeNameConstants.CancelationTokenType}?";
    }

    public static bool IsNonNullableCancelationToken(this ParameterDef parameterDef)
    {
        return
            parameterDef.TypeFullName == TypeNameConstants.CancelationTokenType;
    }
}
