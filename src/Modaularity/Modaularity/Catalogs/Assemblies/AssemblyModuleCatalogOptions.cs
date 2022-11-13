using Modaularity.Abstractions;
using Modaularity.Context;
using Modaularity.TypeFinding;

namespace Modaularity.Catalogs.Assemblies;

public class AssemblyModuleCatalogOptions
{
    public ModuleLoadContextOptions ModuleLoadContextOptions = new();

    public Dictionary<string, TypeFinderCriteria> TypeFinderCriterias = new();

    public ModuleNameOptions ModuleNameOptions { get; set; } = Defaults.ModuleNameOptions;

    public TypeFinderOptions TypeFinderOptions { get; set; } = new();

    public static class Defaults
    {
        public static ModuleNameOptions ModuleNameOptions { get; set; } = new();
    }
}
