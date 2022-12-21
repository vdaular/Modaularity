using Modaularity.Catalogs.Assemblies;
using Modaularity.Tests.Modules;

namespace Modaularity.Tests;

public class TypeFinderTests
{
    [Fact]
    public async Task CanGetModulesByAttribute()
    {
        var catalog = new AssemblyModuleCatalog(typeof(TypeFinderTests).Assembly, configure => 
        {
            configure.HasAttribute(typeof(MyModuleAttribute));
        });

        await catalog.Initialize();

        Assert.Equal(2, catalog.GetModules().Count);
    }

    [Fact]
    public async Task CanGetModulesByMultipleCriteria()
    {
        var catalog = new AssemblyModuleCatalog(typeof(TypeFinderTests).Assembly, configure => 
        {
            configure.HasAttribute(typeof(MyModuleAttribute)).IsAbstract(true).HasName(nameof(AbstractModuleWithAttribute));
        });

        await catalog.Initialize();

        Assert.Single(catalog.GetModules());
    }
}
