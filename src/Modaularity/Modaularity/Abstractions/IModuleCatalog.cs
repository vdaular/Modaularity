namespace Modaularity.Abstractions;

public interface IModuleCatalog
{
    Task Initialize();
    bool IsInitialized { get; }
    List<Module> GetModules();
    Module? Get(string name, Version version);
}
