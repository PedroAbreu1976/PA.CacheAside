using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Collections.Immutable;
using System.Linq;

namespace PA.CacheAside;

[Generator]
public class CacheGenerator : IIncrementalGenerator
{
    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        var provider = context.SyntaxProvider.CreateSyntaxProvider(
            predicate: static (s, _) => s is InterfaceDeclarationSyntax,
            transform: static (ctx, _) => (InterfaceDeclarationSyntax)ctx.Node)
            .Where(x => x is not null);

        var compilation = context.CompilationProvider.Combine(provider.Collect());

        context.RegisterSourceOutput(compilation, Execute);
    }

    private static void Execute(SourceProductionContext context, (Compilation Left, ImmutableArray<InterfaceDeclarationSyntax> Right) tuple)
    {
        
        var interfaceDefs = tuple.GetCacheEnabledInterfaceDefs();
        foreach (var interfaceDef in interfaceDefs)
        {
            context.CreateCacheAsideImplementation(interfaceDef);
        }

        context.CreateCacheAsideExtensions(interfaceDefs);
    }
}
