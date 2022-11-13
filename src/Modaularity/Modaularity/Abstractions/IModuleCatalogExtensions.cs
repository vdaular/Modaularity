namespace Modaularity.Abstractions;

public static class IModuleCatalogExtensions
{
    public static Module Single(this IModuleCatalog catalog)
    {
        var modules = catalog.GetModules();

        return modules.Single();
    }

    public static Module Get(this IModuleCatalog catalog) => catalog.Single();

    public static List<Module> GetByTag(this IModuleCatalog catalog, string tag)
        => catalog.GetModules().Where(x => x.Tags.Contains(tag)).ToList();
}
