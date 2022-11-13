using Modaularity.Abstractions;

namespace Modaularity.Catalogs.Delegates;

public class DelegateModuleCatalogOptions
{
    public ModuleNameOptions NameOptions { get; set; } = new ModuleNameOptions();
    public List<DelegateConversionRule> ConversionRules { get; set; } = new();
    public string MethodName { get; set; } = "Run";
    public string TypeName { get; set; } = "GeneratedType";
    public string NamespaceName { get; set; } = "GeneratedNamespace";
    public Func<DelegateModuleCatalogOptions, string?> MethodNameGenerator { get; set; } = options => options.MethodName;
    public Func<DelegateModuleCatalogOptions, string?> TypeNameGenerator { get; set; } = options => options.TypeName;
    public Func<DelegateModuleCatalogOptions, string?> NamespaceNameGenerator { get; set; } = options => options.NamespaceName;
    public List<string> Tags { get; set; } = new();
}
