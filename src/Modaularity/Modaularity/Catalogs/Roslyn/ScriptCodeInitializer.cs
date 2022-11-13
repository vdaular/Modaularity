using DotNetTypeGenerator;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Scripting;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Scripting;
using System.Reflection;
using System.Text;

namespace Modaularity.Catalogs.Roslyn;

public class ScriptCodeInitializer
{
    private readonly string _code;
    private readonly RoslynModuleCatalogOptions? _options;

    public ScriptCodeInitializer(string code, RoslynModuleCatalogOptions? options)
    {
        if (string.IsNullOrWhiteSpace(code))
            throw new ArgumentOutOfRangeException(nameof(code), code, "El script no puede ser nulo o vacío");

        _code = code;
        _options = options ?? new();

        if (_options.TypeNameGenerator is null)
            throw new ArgumentNullException(nameof(_options.TypeNameGenerator));

        if (_options.NamespaceNameGenerator is null)
            throw new ArgumentNullException(nameof(_options.NamespaceNameGenerator));

        if (_options.MethodNameGenerator is null)
            throw new ArgumentNullException(nameof(_options.MethodNameGenerator));
    }

    public async Task<Assembly> CreateAssembly()
    {
        try
        {
            var returnType = await GetReturnType();
            var parameters = await GetParameters();
            var updatedScript = await RemoveProperties(_code);

            var generator = new AssemblyGenerator();
            generator.ReferenceAssemblyContainingType<Action>();

            var code = new StringBuilder();
            code.AppendLine("using System;");
            code.AppendLine("using System.Diagnostics;");
            code.AppendLine("using System.Threading.Tasks;");
            code.AppendLine("using System.Text;");
            code.AppendLine("using System.Collections;");
            code.AppendLine("using System.Collections.Generic;");
            code.AppendLine("using System.Reflection;");
            code.AppendLine("[assembly: AssemblyFileVersion(\"1.0.0.0\")]");

            if (_options.AdditionalNamespaces?.Any() == true)
                foreach (var ns in _options.AdditionalNamespaces)
                    code.AppendLine($"using {ns};");

            code.AppendLine($"namespace {_options.NamespaceNameGenerator(_options)};");
            code.AppendLine($"public class {_options.TypeNameGenerator(_options)}");
            code.StartBlock();

            if (returnType == null)
            {
                if (_options.ReturnsTasks)
                    code.AppendLine($"public async Task {_options.MethodNameGenerator(_options)}({GetParametersString(parameters)})");
                else
                    code.AppendLine($"public void {_options.MethodNameGenerator(_options)}({GetParametersString(parameters)})");
            }
            else
            {
                if (_options.ReturnsTasks)
                    code.AppendLine($"public async Task<{returnType.FullName}> {_options.MethodNameGenerator(_options)}({GetParametersString(parameters)})");
                else
                    code.AppendLine($"public {returnType.FullName} {_options.MethodNameGenerator(_options)}({GetParametersString(parameters)})");
            }

            code.StartBlock();
            code.AppendLine(updatedScript);
            code.FinishBlock();

            code.FinishBlock();

            var assemblySourceCode = code.ToString();
            var result = generator.GenerateAssembly(assemblySourceCode);

            return result;
        }
        catch (Exception ex)
        {
            throw new InvalidCodeException($"No fue posible crear el ensamblado desde el script. Código: {_code}", ex);
        }
    }

    private async Task<string> RemoveProperties(string currentScript)
    {
        var tree = CSharpSyntaxTree.ParseText(currentScript);
        var root = await tree.GetRootAsync();

        var descendants = root.DescendantNodes().ToList();
        var declarations = descendants.OfType<PropertyDeclarationSyntax>().ToList();

        if (declarations?.Any() != true)
            return currentScript;

        var firstProperty = declarations.First();
        root = root.RemoveNode(firstProperty, SyntaxRemoveOptions.KeepEndOfLine);

        var updatedScript = root.GetText();
        var result = updatedScript.ToString();

        return await RemoveProperties(result);
    }

    private async Task<List<(string, Type)>> GetParameters()
    {
        var csharpScript = CSharpScript.Create(_code, ScriptOptions.Default);
        var compilation = csharpScript.GetCompilation();
        var syntaxTree = compilation.SyntaxTrees.Single();
        var semanticModel = compilation.GetSemanticModel(syntaxTree);
        var descendants = (await syntaxTree.GetRootAsync()).DescendantNodes().ToList();
        var declarations = descendants.OfType<PropertyDeclarationSyntax>().ToList();
        var result = new List<(string, Type)>();

        if (declarations?.Any() != true)
            return result;

        var symbolDisplayFormat = new SymbolDisplayFormat(typeQualificationStyle: SymbolDisplayTypeQualificationStyle.NameAndContainingTypesAndNamespaces);

        foreach (var propertyDeclaration in declarations)
        {
            var name = propertyDeclaration.Identifier.Text;
            var typeInfo = semanticModel.GetTypeInfo(propertyDeclaration.Type);
            var typeName = typeInfo.Type.ToDisplayString(symbolDisplayFormat);
            Type? type = Type.GetType(typeName, true);

            result.Add((name, type));
        }

        return result;
    }

    private async Task<Type?> GetReturnType()
    {
        var csharpScript = CSharpScript.Create(_code, ScriptOptions.Default);
        var compilation = csharpScript.GetCompilation();
        var syntaxTree = compilation.SyntaxTrees.Single();
        var semanticModel = compilation.GetSemanticModel(syntaxTree);
        var descendants = (await syntaxTree.GetRootAsync()).DescendantNodes().ToList();
        var returnSyntax = descendants.OfType<ReturnStatementSyntax>().FirstOrDefault();

        if (returnSyntax == null)
            return null;

        ExpressionSyntax? expr = returnSyntax.Expression;
        var typeInfo = semanticModel.GetTypeInfo(expr);
        var symbolDisplayFormat = new SymbolDisplayFormat(typeQualificationStyle: SymbolDisplayTypeQualificationStyle.NameAndContainingTypesAndNamespaces);
        string fullyQualifiedName;

        if (typeInfo.Type is INamedTypeSymbol mySymbol)
            fullyQualifiedName = mySymbol.ToDisplayString(symbolDisplayFormat);
        else
            fullyQualifiedName = typeInfo.Type.ToDisplayString(symbolDisplayFormat);

        try
        {
            var result = Type.GetType(fullyQualifiedName, true);

            return result;
        }
        catch (Exception ex)
        {
            throw new NotSupportedException($"{fullyQualifiedName} no es un tipo de respuesta soportado para un script", ex);
        }
    }

    private string GetParametersString(List<(string, Type)> parameters)
    {
        if (parameters?.Any() != true)
            return "";

        var result = string.Join(", ", parameters.Select(x => $"{x.Item2.FullName} {x.Item1}"));

        return result;
    }
}