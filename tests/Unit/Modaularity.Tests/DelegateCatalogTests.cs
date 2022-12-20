using Modaularity.Abstractions;
using Modaularity.Catalogs.Delegates;
using System.Reflection;
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

	[Fact]
	public async Task CanInitializeFunc()
	{
		var catalog = new DelegateModuleCatalog(new Func<int, bool>(i => true));

		await catalog.Initialize();

		var allModules = catalog.GetModules();

		Assert.NotEmpty(allModules);
	}

	[Fact]
	public async Task CanInitializeAsyncAction()
	{
		var catalog = new DelegateModuleCatalog(new Action<int>(async i =>
		{
			await Task.Delay(TimeSpan.FromMilliseconds(100));
			_testOutputHelper.WriteLine("Hello from test");
		}));

		await catalog.Initialize();

		var allModules = catalog.GetModules();

		Assert.NotEmpty(allModules);
	}

	[Fact]
	public async Task CanInitilizeAsyncFunc()
	{
		var catalog = new DelegateModuleCatalog(new Func<Base64FormattingOptions, Task<bool>>(async i =>
		{
			await Task.Delay(TimeSpan.FromMilliseconds(100));
			_testOutputHelper.WriteLine("Hello from test");

			return true;
		}));

		await catalog.Initialize();

		var allModules = catalog.GetModules();

		Assert.NotEmpty(allModules);
	}

	[Fact]
	public async Task ByDefaultNoProperties()
	{
		var catalog = new DelegateModuleCatalog(new Func<int, Task<bool>>(async i =>
		{
            await Task.Delay(TimeSpan.FromMilliseconds(100));
            _testOutputHelper.WriteLine("Hello from test");

            return true;
        }));

		await catalog.Initialize();

		var moduleType = catalog.Single().Type;

		Assert.Empty(moduleType.GetProperties());
	}

	[Fact]
	public async Task ByDefaultRunMethod()
	{
		var catalog = new DelegateModuleCatalog(new Func<int, Task<bool>>(async i =>
		{
            await Task.Delay(TimeSpan.FromMilliseconds(100));
            _testOutputHelper.WriteLine("Hello from test");

            return true;
        }));

		await catalog.Initialize();

		var moduleType = catalog.Single().Type;
		var methodInfos = moduleType.GetMethods(BindingFlags.Instance | BindingFlags.Public | BindingFlags.DeclaredOnly);

		Assert.Single(methodInfos);
		Assert.Equal("Run", methodInfos.Single().Name);
	}

	[Fact]
	public async Task ByDefaultGeneratedNamespace()
	{
		var catalog = new DelegateModuleCatalog(new Func<int, Task<bool>>(async i =>
		{
            await Task.Delay(TimeSpan.FromMilliseconds(100));
            _testOutputHelper.WriteLine("Hello from test");

            return true;
        }));

		await catalog.Initialize();

		var moduleType = catalog.Single().Type;

		Assert.Equal("GeneratedNamespace", moduleType.Namespace);
	}

	[Fact]
	public async Task CanConfigureModuleNameFromConstructor()
	{
		var catalog = new DelegateModuleCatalog(new Func<int, Task<bool>>(async i =>
		{
            await Task.Delay(TimeSpan.FromMilliseconds(100));
            _testOutputHelper.WriteLine("Hello from test");

            return true;
        }), "HelloModule");

		await catalog.Initialize();

		Assert.Equal("HelloModule", catalog.Single().Name);
	}

	[Fact]
	public async Task CanConfigureNamespace()
	{
		var catalog = new DelegateModuleCatalog(new Func<int, Task<bool>>(async i =>
		{
            await Task.Delay(TimeSpan.FromMilliseconds(100));
            _testOutputHelper.WriteLine("Hello from test");

            return true;
        }), new DelegateModuleCatalogOptions()
		{
			NamespaceName = "HelloThereNs"
		});

		await catalog.Initialize();

		var moduleType = catalog.Single().Type;

		Assert.Equal("HelloThereNs", moduleType.Namespace);
	}
}
