namespace Demo.Services;

public interface ILoggerService
{
    void LogRepositoryQuery(string repositoryName, string methodName, params object?[] parameters);
    void LogRepositoryCommand(string repositoryName, string methodName, params object?[] parameters);
}

public class LoggerService : ILoggerService
{
    public void LogRepositoryCommand(string repositoryName, string methodName, params object?[] parameters)
    {
        parameters = parameters.Select(p => p?.ToString() ?? "null").ToArray();
        var parameterString = string.Join(", ", parameters);
        Console.WriteLine($"Command: {repositoryName}.{methodName}({string.Join(", ", parameterString)})");
    }

    public void LogRepositoryQuery(string repositoryName, string methodName, params object?[] parameters)
    {
        parameters = parameters.Select(p => p?.ToString() ?? "null").ToArray();
        var parameterString = string.Join(", ", parameters);
        Console.WriteLine($"Query: {repositoryName}.{methodName}({string.Join(", ", parameterString)})");
    }
}