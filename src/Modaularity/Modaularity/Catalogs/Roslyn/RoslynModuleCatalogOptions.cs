using Modaularity.Abstractions;
using System.Reflection;

namespace Modaularity.Catalogs.Roslyn;

public class RoslynModuleCatalogOptions
{
    private string _moduleName = "RoslynCode";
    private Version _moduleVersion = new(1, 0, 0);

    public string TypeName { get; set; } = "GeneratedType";
    public string NamespaceName { get; set; } = "GeneratedNamespace";
    public string MethodName { get; set; } = "Run";
    public bool ReturnsTasks { get; set; } = true;
    public Func<RoslynModuleCatalogOptions, string> TypeNameGenerator { get; set; } = options
        => options.TypeName;
    public Func<RoslynModuleCatalogOptions, string> NamespaceNameGenerator { get; set; } = options
        => options.NamespaceName;

    public Func<RoslynModuleCatalogOptions, string> MethodNameGenerator { get; set; } = options
        => options.MethodName;

    public List<Assembly> AdditionalReferences { get; set; } = new();
    public List<string> AdditionalNamespaces { get; set; } = new();
    public ModuleNameOptions ModuleNameOptions { get; set; } = new();

    public string ModuleName
    {
        get => _moduleName;
        set
        {
            _moduleName = value;

            ModuleNameOptions.ModuleNameGenerator = (options, type) => _moduleName;
        }
    }

    public Version ModuleVersion 
    { 
        get => _moduleVersion;
        set 
        { 
            _moduleVersion = value;

            ModuleNameOptions.ModuleVersionGenerator = (options, type) => _moduleVersion;
        }
    }

    public List<string> Tags { get; set; } = new();
}
