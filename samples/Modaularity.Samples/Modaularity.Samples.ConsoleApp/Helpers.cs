using Modaularity.Abstractions;
using Modaularity.Catalogs.Assemblies;
using Modaularity.Catalogs.Composites;
using Modaularity.Catalogs.Types;
using Modaularity.Samples.Shared;

namespace Modaularity.Samples.ConsoleApp;

public class Helpers
{
    public static async Task AssemblyCatalogSample()
    {
        Console.WriteLine("Assembly Catalog Sample");

        // 1. Create a new module catalog from current assembly
        var assemblyModuleCatalog = new AssemblyModuleCatalog(typeof(Program).Assembly, type
            => typeof(IModule).IsAssignableFrom(type));

        // 2. Initialize catalog
        await assemblyModuleCatalog.Initialize();

        // 3. Get the modules from the catalog
        var assemblyModules = assemblyModuleCatalog.GetModules();

        foreach (var module in assemblyModules)
        {
            var inst = (IModule)Activator.CreateInstance(module);
            inst.Run();
        }

        Console.WriteLine();
    }

    public static async Task TypeCatalogSample()
    {
        Console.WriteLine("Type Catalog Sample");

        var typeModuleCatalog = new TypeModuleCatalog(typeof(FirstModule));
        await typeModuleCatalog.Initialize();

        var typeModule = typeModuleCatalog.Get();
        var moduleInstance = (IModule)Activator.CreateInstance(typeModule);
        moduleInstance.Run();

        Console.WriteLine();
    }

    public static async Task CompositeCatalogSample()
    {
        Console.WriteLine("Composite Catalog Sample");

        // 1. Create a new module catalog from current assembly
        var assemblyModuleCatalog = new AssemblyModuleCatalog(typeof(Program).Assembly, type
            => typeof(IModule).IsAssignableFrom(type));

        // 2. Also create a new module catalog from a type
        var typeModuleCatalog = new TypeModuleCatalog(typeof(MyModule));

        // 3. Then combine the catalogs into a composite catalog
        var compositeCatalog = new CompositeModuleCatalog(assemblyModuleCatalog, typeModuleCatalog);

        // 4. Initialize the composite catalog
        await compositeCatalog.Initialize();

        // 5. Get the modules from the catalog
        var assemblyModules = compositeCatalog.GetModules();

        foreach (var module in assemblyModules)
        {
            if (module.Type.Name == "MyModule")
            {
                var inst = (IMyModule)Activator.CreateInstance(module);
                inst.Run();
            }
            else
            {
                var inst = (IModule)Activator.CreateInstance(module);
                inst.Run();
            }
        }

        Console.WriteLine();
    }
}
