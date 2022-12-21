using Modaularity.Catalogs.Roslyn;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace Modaularity.Tests.Catalogs.Roslyn;

public static class TestHelpers
{
    public static async Task<List<Type>> CompileScript(string code, RoslynModuleCatalogOptions options = null)
    {
        var catalog = new ScriptCodeInitializer(code, options);
        var assembly = await catalog.CreateAssembly();
        var result = assembly.GetTypes().Where(x => x.GetCustomAttribute(typeof(CompilerGeneratedAttribute), true) == null).ToList();

        return result;
    }

    public static async Task<List<Type>> CompileRegular(string code, RoslynModuleCatalogOptions options = null)
    {
        var catalog = new RegularCodeInitializer(code, options);
        var assembly = await catalog.CreateAssembly();
        var result = assembly.GetTypes().Where(x => x.GetCustomAttribute(typeof(CompilerGeneratedAttribute), true) == null).ToList();

        return result;
    }

    public static async Task<List<Type>> CreateCatalog(string code, RoslynModuleCatalogOptions options = null)
    {
        var catalog = new RoslynModuleCatalog(code, options);

        await catalog.Initialize();

        return catalog.GetModules().Select(x => x.Type).ToList();
    }
}