using PA.CacheAside.Models;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace PA.CacheAside.Extensions;

internal static class CompilationExtensions
{
    internal static InterfaceDef GetInterface(this Compilation compilation, string fullName) =>
        compilation
            .GetTypeByMetadataName(fullName) is INamedTypeSymbol interfaceSymbol
            ? new InterfaceDef(interfaceSymbol)
            : null;

    internal static ClassDef GetClass(this Compilation compilation, ClassDeclarationSyntax syntax) =>
        compilation
            .GetSemanticModel(syntax.SyntaxTree)
            .GetDeclaredSymbol(syntax) as INamedTypeSymbol is INamedTypeSymbol symbol
            ? new ClassDef(symbol)
            : null;

    internal static InterfaceDef GetInterface(this Compilation compilation, InterfaceDeclarationSyntax syntax) =>
        compilation
            .GetSemanticModel(syntax.SyntaxTree)
            .GetDeclaredSymbol(syntax) as INamedTypeSymbol is INamedTypeSymbol symbol
            ? new InterfaceDef(symbol)
            : null;
}
