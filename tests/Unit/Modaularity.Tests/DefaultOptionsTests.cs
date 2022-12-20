using Modaularity.Abstractions;
using Modaularity.Catalogs.Assemblies;
using Modaularity.Catalogs.Folders;
using Modaularity.Context;
using Modaularity.TypeFinding;
using Newtonsoft.Json;

namespace Modaularity.Tests;

public class DefaultOptionsTests
{
    private char separator = Path.DirectorySeparatorChar;

    [Fact]
    public async Task CanConfigureDefaultOptions()
    {
        var json = JsonConvert.SerializeObject(1);
        ModuleLoadContextOptions.Defaults.UseHostApplicationAssemblies = UseHostApplicationAssembliesEnum.Always;

        Action<TypeFinderCriteriaBuilder> configureFinder = configure =>
        {
            configure.HasName("*JsonResolver");
        };

        var assemblyModuleCatalog = new AssemblyModuleCatalog($"..{separator}..{separator}..{separator}..{separator}..{separator}Assemblies{separator}output{separator}JsonNew{separator}net7.0{separator}JsonNet2.dll");
        var folderModuleCatalog = new FolderModuleCatalog($"..{separator}..{separator}..{separator}..{separator}..{separator}Assemblies{separator}output{separator}JsonOld{separator}net7.0");

        await assemblyModuleCatalog.Initialize();
        await folderModuleCatalog.Initialize();

        var newModule = assemblyModuleCatalog.Single();
        var oldModule = folderModuleCatalog.Single();

        dynamic newModuleJsonResolver = Activator.CreateInstance(newModule);
        var newModuleVersion = newModuleJsonResolver.GetVersion();

        dynamic oldModuleJsonResolver = Activator.CreateInstance(oldModule);
        var oldModuleVersion = oldModuleJsonResolver.GetVersion();

        Assert.Equal("13.0.0.0", newModuleVersion);
        Assert.Equal("13.0.0.0", oldModuleVersion);
    }
}
