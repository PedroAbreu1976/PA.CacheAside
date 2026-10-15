using PA.CacheAside.Models;
using System.Linq;

namespace PA.CacheAside.Extensions;

internal static class MethodDefExtensions
{
    public static string GetParametersDefinition(this MethodDef methodDef)
    {
        return methodDef.Parameters
            .Select(x => x.Definition)
            .Join(", ");
    }

    public static string GetParametersCall(this MethodDef methodDef)
    {
        return methodDef.Parameters
            .Select(x => x.Call)
            .Join(", ");
    }

    public static string GetParametersValueInterpolation(this MethodDef methodDef)
    {
        return methodDef.Parameters
            .Select(x => x.ValueInterpolation)
            .Join(", ");
    }

    public static bool IsDefinedInInterface(this MethodDef methodDef, InterfaceDef interfaceDef)
    {
        return interfaceDef.MethodDefs
            .Where(x => x.Name == methodDef.Name)
            .Where(x => x.GetParametersDefinition() == methodDef.GetParametersDefinition())
            .Where(x=>x.ReturnTypeFullName == methodDef.ReturnTypeFullName)
            .Any();
    }

    public static bool ResultIsTaskOrValueTask(this MethodDef methodDef)
    {
        return _AsyncTypes.Any(IsType);

        bool IsType(string typeName)=>
            methodDef.ReturnTypeFullName ==
                typeName ||
                methodDef.ReturnTypeFullName.StartsWith($"{typeName}<");
    }

    public static bool ResultIsAsyncOrTaskOrValueTask(this MethodDef methodDef)
    {
        return methodDef.IsAsync || methodDef.ResultIsTaskOrValueTask();
    }

    public static bool HasResult(this MethodDef methodDef)
    {
        return !_VoidTypes.Any(x=>x == methodDef.ReturnTypeFullName);
    }

    public static T GetAttributeFirstValue<T>(this MethodDef methodDef, string attributeFullName)
    {
        var attribute = methodDef.Attributes.FirstOrDefault(x => x.FullName == attributeFullName);
        if (attribute == null)
        {
            return default;

        }
        var firstArgument = attribute.Arguments.FirstOrDefault();
        if (firstArgument == null)
        {
            return default;
        }
        if (firstArgument.Value != null)
        {
            return (T)firstArgument.Value;
        }
        return default;
    }

    public static T[] GetAttributeFirstValueArray<T>(this MethodDef methodDef, string attributeFullName)
    {
        var attribute = methodDef.Attributes.FirstOrDefault(x => x.FullName == attributeFullName);
        if (attribute == null)
        {
            return [];

        }
        var firstArgument = attribute.Arguments.FirstOrDefault();
        if (firstArgument == null)
        {
            return [];
        }
        if (firstArgument.Values != null && firstArgument.Values.Any())
        {
            return firstArgument.Values.Select(x => (T)x).ToArray();
        }
        return default;
    }

    private static string[] _VoidTypes = 
        [
            TypeNameConstants.VoidTypeSimplename,
            TypeNameConstants.VoidTypeFullname,
            TypeNameConstants.TaskType,
            TypeNameConstants.ValueTaskType
        ];

    private static string[] _AsyncTypes = 
        [
            TypeNameConstants.TaskType,
            TypeNameConstants.ValueTaskType
        ];
}
