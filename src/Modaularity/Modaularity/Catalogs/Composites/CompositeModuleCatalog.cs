using Modaularity.Abstractions;

namespace Modaularity.Catalogs.Composites;

public class CompositeModuleCatalog : IModuleCatalog
{
    private readonly List<IModuleCatalog> _catalogs;

    public CompositeModuleCatalog(params IModuleCatalog[] catalogs)
    {
        _catalogs = catalogs.ToList();
    }

    public void AddCatalog(IModuleCatalog catalog)
    {
        _catalogs.Add(catalog);
    }

    public bool IsInitialized { get; private set; }

    public Module? Get(string name, Version version)
    {
        foreach (var moduleCatalog in _catalogs)
        {
            var module = moduleCatalog.Get(name, version);

            if (module == null)
                continue;

            return module;
        }

        return null;
    }

    public List<Module> GetModules()
    {
        var result = new List<Module>();

        foreach (var moduleCatalog in _catalogs)
        {
            var modulesInCatalog = moduleCatalog.GetModules();
            result.AddRange(modulesInCatalog);
        }

        return result;
    }

    public async Task Initialize()
    {
        if (_catalogs?.Any() != true)
        {
            IsInitialized = true;

            return;
        }

        foreach (var moduleCatalog in _catalogs)
            await moduleCatalog.Initialize();

        IsInitialized = true;
    }
}
