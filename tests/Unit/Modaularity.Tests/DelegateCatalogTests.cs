using Modaularity.Catalogs.Delegates;
using Xunit.Abstractions;

namespace Modaularity.Tests;

public class DelegateCatalogTests
{
    private readonly ITestOutputHelper _testOutputHelper;

	public DelegateCatalogTests(ITestOutputHelper testOutputHelper)
	{
		_testOutputHelper = testOutputHelper;
	}

	[Fact]
	public async Task CanInitialize()
	{
		var catalog = new DelegateModuleCatalog(new Action(() =>
		{
			_testOutputHelper.WriteLine("Hello from test");
		}));

		await catalog.Initialize();

		var allModules = catalog.GetModules();

		Assert.NotEmpty(allModules);
	}
}
