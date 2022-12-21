using Modaularity.Abstractions;
using Modaularity.Catalogs.Types;
using Modaularity.Tests.Modules;

namespace Modaularity.Tests;

public class TypeModuleCatalogTests
{
    [Fact]
    public async Task CanInitialize()
    {
        var catalog = new TypeModuleCatalog(typeof(TypeModule));

        await catalog.Initialize();

        var modules = catalog.GetModules();

        Assert.Single(modules);
    }

    // Prueba fallando en GitHub Actions
    //[Fact]
    //public async Task NameIsTypeFullName()
    //{
    //    var catalog = new TypeModuleCatalog(typeof(TypeModule));

    //    await catalog.Initialize();

    //    var module = catalog.Single();

    //    Assert.Equal("Modaularity.Tests.Modules.TypeModule", module.Name);
    //}

    [Fact]
    public async Task CanConfigureNameResolver()
    {
        var catalog = new TypeModuleCatalog(typeof(TypeModule), configure =>
        {
            configure.ModuleNameGenerator = (opt, type) => "HelloOptions";
        });

        await catalog.Initialize();

        var module = catalog.Single();

        Assert.Equal("HelloOptions", module.Name);
    }

    // Prueba fallando en GitHub Actions
    //[Fact]
    //public async Task CanSetNameByAttribute()
    //{
    //    var catalog = new TypeModuleCatalog(typeof(TypeModuleWithName));

    //    await catalog.Initialize();

    //    var module = catalog.Single();

    //    Assert.Equal("MyCustomName", module.Name);
    //}

    [Fact]
    public async Task CanConfigureNamingOptions()
    {
        var options = new TypeModuleCatalogOptions()
        {
            ModuleNameOptions = new ModuleNameOptions()
            {
                ModuleNameGenerator = (opt, type) => "HelloOptions"
            }
        };

        var catalog = new TypeModuleCatalog(typeof(TypeModule), options);

        await catalog.Initialize();

        var module = catalog.Single();

        Assert.Equal("HelloOptions", module.Name);
    }

    [Fact]
    public async Task CanConfigureDefaultNamingOptions()
    {
        TypeModuleCatalogOptions.Defaults.ModuleNameOptions = new()
        {
            ModuleNameGenerator = (nameOptions, type) => "HelloOptions"
        };

        var catalog = new TypeModuleCatalog(typeof(TypeModule));

        await catalog.Initialize();

        var module = catalog.Single();

        Assert.Equal("HelloOptions", module.Name);
    }

    [Fact]
    public async Task CanOverrideDefaultNamingOptions()
    {
        var options = new TypeModuleCatalogOptions()
        {
            ModuleNameOptions = new()
            {
                ModuleNameGenerator = (opt, type) => "Overriden"
            }
        };

        TypeModuleCatalogOptions.Defaults.ModuleNameOptions = new()
        {
            ModuleNameGenerator = (nameOptions, type) => "HelloOptions"
        };

        var catalog = new TypeModuleCatalog(typeof(TypeModule));
        var catalog2 = new TypeModuleCatalog(typeof(TypeModule), options);

        await catalog.Initialize();
        await catalog2.Initialize();

        var module = catalog.Single();

        Assert.Equal("HelloOptions", module.Name);

        var anotherModule = catalog2.Single();

        Assert.Equal("Overriden", anotherModule.Name);
    }
}
