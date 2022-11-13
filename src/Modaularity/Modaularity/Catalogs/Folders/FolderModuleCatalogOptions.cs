using Modaularity.Abstractions;
using Modaularity.Context;
using Modaularity.TypeFinding;

namespace Modaularity.Catalogs.Folders;

public class FolderModuleCatalogOptions
{
    public bool IncludeSubfolders { get; set; } = true;
    public List<string> SearchPatterns { get; set; } = new() { "*.dll" };
    public ModuleLoadContextOptions ModuleLoadContextOptions { get; set; } = new();
    public TypeFinderCriteria? TypeFinderCriteria { get; set; }
    public Dictionary<string, TypeFinderCriteria> TypeFinderCriterias { get; set; } = new();
    public TypeFinderOptions TypeFinderOptions { get; set; } = new();
    public ModuleNameOptions ModuleNameOptions { get; set; } = Defaults.ModuleNameOptions;

    public static class Defaults
    {
        public static ModuleNameOptions ModuleNameOptions { get; set; } = new();
    }
}
