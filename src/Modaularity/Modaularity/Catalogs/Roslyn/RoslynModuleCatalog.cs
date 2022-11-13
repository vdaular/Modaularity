using Microsoft.CodeAnalysis.CSharp.Scripting;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Scripting;
using Modaularity.Abstractions;
using Modaularity.Catalogs.Assemblies;
using Modaularity.TypeFinding;
using System.Reflection;
using Module = Modaularity.Abstractions.Module;

namespace Modaularity.Catalogs.Roslyn;

public class RoslynModuleCatalog : IModuleCatalog
{
    private readonly RoslynModuleCatalogOptions? _options;
    private readonly string? _code;

    private Assembly? _assembly;
    private AssemblyModuleCatalog? _catalog;

    public RoslynModuleCatalog(string? code, RoslynModuleCatalogOptions? options = null, string? description = null, string? productVersion = null)
    {
        if (string.IsNullOrWhiteSpace(code))
            throw new ArgumentOutOfRangeException(nameof(code), code, "El código no puede ser nulo o vacío");

        _code = code;
        _options = options ?? new();

        _options.ModuleNameOptions.ModuleDescriptionGenerator = (nameOptions, type) => description;
        _options.ModuleNameOptions.ModuleProductVersionGenerator = (nameOptions, type) => productVersion;
    }

    public bool IsInitialized { get; private set; }

    public Module? Get(string name, Version version) => _catalog.Get(name, version);

    private async Task<bool> IsScript()
    {
        try
        {
            var csharpScript = CSharpScript.Create(_code, ScriptOptions.Default);
            var compilation = csharpScript.GetCompilation();
            var syntaxTree = compilation.SyntaxTrees.Single();
            var descendants = (await syntaxTree.GetRootAsync()).DescendantNodes().ToList();
            var classDeclarations = descendants.OfType<ClassDeclarationSyntax>().FirstOrDefault();

            return classDeclarations == null;
        }
        catch (Exception ex)
        {
            throw new InvalidCodeException($"No es posible determinar si el código es script o regular. Código: {_code}", ex);
        }
    }

    public List<Abstractions.Module> GetModules() => _catalog.GetModules();

    public async Task Initialize()
    {
        try
        {
            var isScript = await IsScript();

            if (isScript)
            {
                var scriptInitializer = new ScriptCodeInitializer(_code, _options);
                _assembly = await scriptInitializer.CreateAssembly();
            }
            else
            {
                var regularInitializer = new RegularCodeInitializer(_code, _options);
                _assembly = await regularInitializer.CreateAssembly();
            }

            var assemblyCatalogOptions = new AssemblyModuleCatalogOptions { ModuleNameOptions = _options.ModuleNameOptions };

            if (_options.Tags?.Any() == true)
                assemblyCatalogOptions.TypeFinderOptions = new TypeFinderOptions()
                {
                    TypeFinderCriterias = new()
                    {
                        new TypeFinderCriteria()
                        {
                            Query = (context, type) => true,
                            Tags = _options.Tags
                        }
                    }
                };

            _catalog = new AssemblyModuleCatalog(_assembly, assemblyCatalogOptions);
            await _catalog.Initialize();

            IsInitialized = true;
        }
        catch (Exception ex)
        {
            throw new InvalidCodeException($"No fue posible inicializar el catálogo con código: {Environment.NewLine}{_code}", ex);
        }
    }
}
