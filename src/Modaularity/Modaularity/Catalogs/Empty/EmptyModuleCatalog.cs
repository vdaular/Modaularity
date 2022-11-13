using Modaularity.Abstractions;

namespace Modaularity.Catalogs.Empty;

public class EmptyModuleCatalog : IModuleCatalog
{
    public bool IsInitialized { get; } = true;

    public Module? Get(string name, Version version) => null;

    public List<Module> GetModules() => new();

    public Task Initialize() => Task.CompletedTask;
}
