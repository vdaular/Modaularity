using Modaularity.Abstractions;
using Modaularity.Catalogs.Assemblies;
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
}
