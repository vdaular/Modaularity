using Modaularity.Abstractions;
using Modaularity.TypeFinding;

namespace Modaularity.Catalogs.Types;

public class TypeModuleCatalogOptions
{
    public ModuleNameOptions ModuleNameOptions { get; set; } = Defaults.ModuleNameOptions;
    public Dictionary<string, TypeFinderCriteria> TypeFinderCriterias = new();
    public TypeFinderOptions TypeFinderOptions { get; set; } = new();
    public ITypeFindingContext? TypeFindingContext { get; set; } = null;

    public static class Defaults
    {
        public static ModuleNameOptions ModuleNameOptions { get; set; } = new();
    }
}