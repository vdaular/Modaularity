using Microsoft.Extensions.Logging;
using Modaularity.Abstractions;
using Modaularity.Catalogs.Assemblies;
using Modaularity.Catalogs.Folders;
using Modaularity.Context;
using Modaularity.TypeFinding;
using Newtonsoft.Json;
using System.Reflection;

namespace Modaularity.Tests;

public class FolderCatalogTests
{
    private readonly string _moduleFolder =
        $"..{Path.DirectorySeparatorChar}..{Path.DirectorySeparatorChar}..{Path.DirectorySeparatorChar}..{Path.DirectorySeparatorChar}..{Path.DirectorySeparatorChar}Assemblies{Path.DirectorySeparatorChar}output{Path.DirectorySeparatorChar}net7.0";

    [Fact]
    public async Task CanInitialize()
    {
        var catalog = new FolderModuleCatalog(_moduleFolder);

        await catalog.Initialize();

        var modules = catalog.GetModules();

        Assert.NotEmpty(modules);
    }

    [Fact]
    public async Task CanInitializeWithCriteria()
    {
        var catalog = new FolderModuleCatalog(_moduleFolder, configure =>
        {
            configure.HasName("*Module*");
        });

        await catalog.Initialize();

        var moduleCount = catalog.GetModules().Count();

        Assert.Equal(2, moduleCount);
    }

    [Fact]
    public async Task CanUseFolderOptions()
    {
        var options = new FolderModuleCatalogOptions
        {
            TypeFinderOptions = new()
            {
                TypeFinderCriterias = new()
                {
                    TypeFinderCriteriaBuilder.Create().HasName("SecondModule").Tag("MyModule")
                }
            }
        };

        var catalog = new FolderModuleCatalog(_moduleFolder, options);

        await catalog.Initialize();

        var moduleCount = catalog.GetModules().Count;

        Assert.Equal(1, moduleCount);
        Assert.Equal("SecondModule", catalog.Single().Type.Name);
    }

    [Fact]
    public async Task FolderOptionsAreUsedToLimitLoadedAssemblies()
    {
        var options = new FolderModuleCatalogOptions 
        { 
            TypeFinderOptions = new()
            {
                TypeFinderCriterias = new()
                {
                    TypeFinderCriteriaBuilder.Create().HasName("SecondModule").Tag("MyModule")
                }
            }
        };

        var catalog = new FolderModuleCatalog(_moduleFolder, options);

        await catalog.Initialize();

        var field = catalog.GetType().GetField("_catalogs", BindingFlags.Instance | BindingFlags.NonPublic);
        var loadedAssemblies = (List<AssemblyModuleCatalog>)field.GetValue(catalog);

        Assert.Single(loadedAssemblies);
    }

    [Fact]
    public async Task CanConfigureNamingOptions()
    {
        var options = new FolderModuleCatalogOptions()
        {
            ModuleNameOptions = new()
            {
                ModuleNameGenerator = (nameOptions, type) => type.FullName + "Modified"
            }
        };

        var catalog = new FolderModuleCatalog(_moduleFolder, options);

        await catalog.Initialize();

        var modules = catalog.GetModules();

        foreach (var module in modules)
            Assert.EndsWith("Modified", module.Name);
    }

    [Fact]
    public async Task CanUseReferencedDependencies()
    {
        ModuleLoadContextOptions.Defaults.UseHostApplicationAssemblies = UseHostApplicationAssembliesEnum.Never;

        Action<TypeFinderCriteriaBuilder> configureFinder = configure =>
        {
            configure.HasName("*JsonResolver*");
        };

        var folder1Catalog = 
            new FolderModuleCatalog($"..{Path.DirectorySeparatorChar}..{Path.DirectorySeparatorChar}..{Path.DirectorySeparatorChar}..{Path.DirectorySeparatorChar}..{Path.DirectorySeparatorChar}Assemblies{Path.DirectorySeparatorChar}output{Path.DirectorySeparatorChar}JsonNew{Path.DirectorySeparatorChar}net7.0", configureFinder);
        var folder2Catalog =
            new FolderModuleCatalog($"..{Path.DirectorySeparatorChar}..{Path.DirectorySeparatorChar}..{Path.DirectorySeparatorChar}..{Path.DirectorySeparatorChar}..{Path.DirectorySeparatorChar}Assemblies{Path.DirectorySeparatorChar}output{Path.DirectorySeparatorChar}JsonOld{Path.DirectorySeparatorChar}net7.0", configureFinder);

        await folder1Catalog.Initialize();
        await folder2Catalog.Initialize();

        var newModule = folder1Catalog.Single();
        var oldModule = folder2Catalog.Single();

        dynamic newModuleJsonResolver = Activator.CreateInstance(newModule);
        var newModuleVersion = newModuleJsonResolver.GetVersion();

        dynamic oldModuleJsonResolver = Activator.CreateInstance(oldModule);
        var oldModuleVersion = oldModuleJsonResolver.GetVersion();

        Assert.Equal("13.0.0.0", newModuleVersion);
        Assert.Equal("13.0.0.0", oldModuleVersion);
    }

    [Fact]
    public async Task CanUseSelectedHostsDependencies()
    {
        var json = JsonConvert.SerializeObject(1);
        var loggin = new LoggerFactory();
        var options = new FolderModuleCatalogOptions
        {
            TypeFinderCriteria = new()
            {
                Name = "*JsonResolver*"
            },
            ModuleLoadContextOptions = new()
            {
                UseHostApplicationAssemblies = UseHostApplicationAssembliesEnum.Selected,
                HostApplicationAssemblies = new()
                {
                    typeof(LoggerFactory).Assembly.GetName()
                }
            }
        };

        var catalog =
            new FolderModuleCatalog($"..{Path.DirectorySeparatorChar}..{Path.DirectorySeparatorChar}..{Path.DirectorySeparatorChar}..{Path.DirectorySeparatorChar}..{Path.DirectorySeparatorChar}Assemblies{Path.DirectorySeparatorChar}output{Path.DirectorySeparatorChar}JsonOld{Path.DirectorySeparatorChar}net7.0", options);

        await catalog.Initialize();

        var oldModule = catalog.Single();
        dynamic oldModuleJsonResolver = Activator.CreateInstance(oldModule);
        var oldModuleVersion = oldModuleJsonResolver.GetVersion();
        var loggerVersion = oldModuleJsonResolver.GetLoggingVersion();

        Assert.Equal("7.0.0.0", loggerVersion);
        Assert.Equal("13.0.0.0", oldModuleVersion);
    }

    [Collection(nameof(NotThreadSafeResourceCollection))]
    public class DefaultOptions : IDisposable
    {
        private readonly string _moduleFolder =
            $"..{Path.DirectorySeparatorChar}..{Path.DirectorySeparatorChar}..{Path.DirectorySeparatorChar}..{Path.DirectorySeparatorChar}..{Path.DirectorySeparatorChar}Assemblies{Path.DirectorySeparatorChar}output{Path.DirectorySeparatorChar}net7.0";

        public DefaultOptions()
        {
            FolderModuleCatalogOptions.Defaults.ModuleNameOptions = new()
            {
                ModuleNameGenerator = (nameOptions, type) => type.FullName + "Modified"
            };
        }

        [Fact]
        public async Task CanConfigureDefaultNamingOptions()
        {
            var catalog = new FolderModuleCatalog(_moduleFolder);

            await catalog.Initialize();

            var modules = catalog.GetModules();

            foreach (var module in modules)
                Assert.EndsWith("Modified", module.Name);
        }

        [Fact]
        public async Task DefaultAssemblyNamingOptionsDoesntAffectFolderCatalogs()
        {
            AssemblyModuleCatalogOptions.Defaults.ModuleNameOptions = new()
            {
                ModuleNameGenerator = (nameOptions, type) => type.FullName + "ModifiedAssembly"
            };

            var catalog = new FolderModuleCatalog(_moduleFolder);

            await catalog.Initialize();

            var modules = catalog.GetModules();

            foreach (var module in modules)
                Assert.False(module.Name.EndsWith("ModifiedAssembly"));
        }

        public void Dispose()
        {
            AssemblyModuleCatalogOptions.Defaults.ModuleNameOptions = new ModuleNameOptions();
            FolderModuleCatalogOptions.Defaults.ModuleNameOptions = new ModuleNameOptions();
        }
    }
}
