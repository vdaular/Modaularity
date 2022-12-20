using Microsoft.Extensions.Logging;
using Modaularity.Abstractions;
using Modaularity.Catalogs.Assemblies;
using Modaularity.Context;
using Newtonsoft.Json;
using Xunit.Abstractions;

namespace Modaularity.Tests;

public class AssemblyModuleCatalogTests
{
	private readonly ITestOutputHelper _testOutputHelper;
	private char separator = Path.DirectorySeparatorChar;

	public AssemblyModuleCatalogTests(ITestOutputHelper testOutputHelper)
	{
		_testOutputHelper = testOutputHelper;
	}

	[Fact]
	public async Task CanInitialize()
	{
		var folder = Environment.CurrentDirectory;
		var catalog = new AssemblyModuleCatalog($"..{separator}..{separator}..{separator}..{separator}..{separator}Assemblies{separator}output{separator}net7.0{separator}TestAssembly1.dll");
		await catalog.Initialize();

		var allModules = catalog.GetModules();

		Assert.NotEmpty(allModules);
	}

	[Fact]
	public async Task CanInitializeWithCriteria()
	{
		var catalog = new AssemblyModuleCatalog($"..{separator}..{separator}..{separator}..{separator}..{separator}Assemblies{separator}output{separator}net7.0{separator}TestAssembly1.dll",
			configure =>
			{
				configure.HasName("*Module*");
			});

		await catalog.Initialize();

		var allModules = catalog.GetModules();

		Assert.Single(allModules);
	}

	[Fact]
	public async Task CanConfigureNamingOptions()
	{
		var options = new AssemblyModuleCatalogOptions()
		{
			ModuleNameOptions = new ModuleNameOptions() { ModuleNameGenerator = (nameOptions, type) => type.FullName + "Modified" }
		};

		var catalog = new AssemblyModuleCatalog($"..{separator}..{separator}..{separator}..{separator}..{separator}Assemblies{separator}output{separator}net7.0{separator}TestAssembly1.dll", options);

		await catalog.Initialize();

		var allModules = catalog.GetModules();

		foreach (var module in allModules)
			Assert.EndsWith("Modified", module.Name);
	}

	[Fact]
	public async Task ByDefaultOnlyContainsPublicNonAbstractClasses()
	{
		var catalog = new AssemblyModuleCatalog($"..{separator}..{separator}..{separator}..{separator}..{separator}Assemblies{separator}output{separator}net7.0{separator}TestAssembly1.dll");
		await catalog.Initialize();

		var allModules = catalog.GetModules();
		var module = allModules.Single();

		Assert.False(module.Type.IsAbstract);
		Assert.False(module.Type.IsInterface);
	}

	[Fact]
	public async Task CanIncludeAbstractClassesUsingCriteria()
	{
		var catalog = new AssemblyModuleCatalog($"..{separator}..{separator}..{separator}..{separator}..{separator}Assemblies{separator}output{separator}net7.0{separator}TestAssembly1.dll", builder =>
		{
			builder.IsAbstract(true);
		});

		await catalog.Initialize();

		var allModules = catalog.GetModules();
		var module = allModules.Single();

		Assert.True(module.Type.IsAbstract);
	}

	[Fact]
	public async Task ThrowsIfAssemblyNotFound()
	{
        var catalog = new AssemblyModuleCatalog($"..{separator}..{separator}..{separator}..{separator}..{separator}Assemblies{separator}output{separator}net7.0{separator}notexists.dll");

		await Assert.ThrowsAsync<ArgumentException>(async () => await catalog.Initialize());
    }

	[Fact]
	public void ThrowsIfAssemblyPathMissing()
	{
		Assert.Throws<ArgumentNullException>(() => new AssemblyModuleCatalog(""));

		string path = null;

		Assert.Throws<ArgumentNullException>(() => new AssemblyModuleCatalog(path));
	}

	[Fact]
	public async Task CanUseReferencedDependencies()
	{
        var json = JsonConvert.SerializeObject(1);
        _testOutputHelper.WriteLine(json);

		var options = new AssemblyModuleCatalogOptions()
		{
			ModuleLoadContextOptions = new ModuleLoadContextOptions()
			{
				UseHostApplicationAssemblies = UseHostApplicationAssembliesEnum.Never
			}
		};

		var assemblyCatalog1 = new AssemblyModuleCatalog($"..{separator}..{separator}..{separator}..{separator}..{separator}Assemblies{separator}output{separator}JsonNew{separator}net7.0{separator}JsonNet2.dll");
		await assemblyCatalog1.Initialize();

		var assemblyCatalog2 = new AssemblyModuleCatalog($"..{separator}..{separator}..{separator}..{separator}..{separator}Assemblies{separator}output{separator}JsonOld{separator}net7.0{separator}JsonNet1.dll");
		await assemblyCatalog2.Initialize();

		var newModule = assemblyCatalog1.Single();
		var oldModule = assemblyCatalog2.Single();

		dynamic newModuleJsonResolver = Activator.CreateInstance(newModule);
		var newModuleVersion = newModuleJsonResolver.GetVersion();

		dynamic oldModuleJsonResolver = Activator.CreateInstance(oldModule);
		var oldModuleVersion = oldModuleJsonResolver.GetVersion();

		Assert.Equal("13.0.0.0", newModuleVersion);
		Assert.Equal("13.0.0.0", oldModuleVersion);
    }

	[Fact]
	public async Task CanUseHostsDependencies()
	{
        var json = JsonConvert.SerializeObject(1);
        _testOutputHelper.WriteLine(json);

        var options = new AssemblyModuleCatalogOptions()
        {
            ModuleLoadContextOptions = new ModuleLoadContextOptions()
            {
                UseHostApplicationAssemblies = UseHostApplicationAssembliesEnum.Always
            }
        };

        var assemblyCatalog1 = new AssemblyModuleCatalog($"..{separator}..{separator}..{separator}..{separator}..{separator}Assemblies{separator}output{separator}JsonNew{separator}net7.0{separator}JsonNet2.dll");
        await assemblyCatalog1.Initialize();

        var assemblyCatalog2 = new AssemblyModuleCatalog($"..{separator}..{separator}..{separator}..{separator}..{separator}Assemblies{separator}output{separator}JsonOld{separator}net7.0{separator}JsonNet1.dll");
        await assemblyCatalog2.Initialize();

        var newModule = assemblyCatalog1.Single();
        var oldModule = assemblyCatalog2.Single();

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

        var logging = new LoggerFactory();

		var options = new AssemblyModuleCatalogOptions();

		options.ModuleLoadContextOptions = new()
		{
			UseHostApplicationAssemblies = UseHostApplicationAssembliesEnum.Selected,
			HostApplicationAssemblies = new()
			{
				typeof(LoggerFactory).Assembly.GetName()
			}
		};

		var catalog = new AssemblyModuleCatalog($"..{separator}..{separator}..{separator}..{separator}..{separator}Assemblies{separator}output{separator}JsonOld{separator}net7.0{separator}JsonNet1.dll");
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
        private char separator = Path.DirectorySeparatorChar;

        public DefaultOptions()
		{
			AssemblyModuleCatalogOptions.Defaults.ModuleNameOptions = new()
			{
				ModuleNameGenerator = (nameOptions, type) => type.FullName + "Modified"
			};
		}

		[Fact]
		public async Task CanConfigureDefaultNamingOptions()
		{
            var catalog = new AssemblyModuleCatalog($"..{separator}..{separator}..{separator}..{separator}..{separator}Assemblies{separator}output{separator}net7.0{separator}TestAssembly1.dll");

			await catalog.Initialize();

			var allModules = catalog.GetModules();

			foreach (var module in allModules)
				Assert.EndsWith("Modified", module.Name);
        }

		[Fact]
		public async Task CanOverrideDefaultNamingOptions()
		{
			var options = new AssemblyModuleCatalogOptions() 
			{ 
				ModuleNameOptions = new()
				{
					ModuleNameGenerator = (nameOptions, type) => type.FullName + "Overriden"
				}
			};

            var catalog = new AssemblyModuleCatalog($"..{separator}..{separator}..{separator}..{separator}..{separator}Assemblies{separator}output{separator}net7.0{separator}TestAssembly1.dll");
            var catalog2 = new AssemblyModuleCatalog($"..{separator}..{separator}..{separator}..{separator}..{separator}Assemblies{separator}output{separator}net7.0{separator}TestAssembly2.dll", options);

			await catalog.Initialize();
			await catalog2.Initialize();

			var catalog1Modules = catalog.GetModules();

			foreach (var module in catalog1Modules)
				Assert.EndsWith("Modified", module.Name);

			var catalog2Modules = catalog2.GetModules();

			foreach (var module in catalog2Modules)
				Assert.EndsWith("Overriden", module.Name);
        }

        public void Dispose()
        {
			AssemblyModuleCatalogOptions.Defaults.ModuleNameOptions = new();
        }
    }
}
