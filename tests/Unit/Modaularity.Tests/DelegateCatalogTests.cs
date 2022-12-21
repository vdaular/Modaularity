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

	[Fact]
	public async Task CanConfigureNamespaceUsingGenerator()
	{
		var catalog = new DelegateModuleCatalog(new Func<int, Task<bool>>(async i =>
		{
            await Task.Delay(TimeSpan.FromMilliseconds(100));
            _testOutputHelper.WriteLine("Hello from test");

            return true;
        }), new DelegateModuleCatalogOptions()
		{
			NamespaceNameGenerator = options => "GeneratorNS"
		});

		await catalog.Initialize();

		var moduleType = catalog.Single().Type;

		Assert.Equal("GeneratorNS", moduleType.Namespace);
	}

	[Fact]
	public async Task ByDefaultGeneratedTypeName()
	{
		var catalog = new DelegateModuleCatalog(new Func<int, Task<bool>>(async i =>
		{
            await Task.Delay(TimeSpan.FromMilliseconds(100));
            _testOutputHelper.WriteLine("Hello from test");

            return true;
        }));

		await catalog.Initialize();

		var moduleType = catalog.Single().Type;

		Assert.Equal("GeneratedType", moduleType.Name);
	}

	[Fact]
	public async Task CanConfigureTypeName()
	{
		var catalog = new DelegateModuleCatalog(new Func<int, Task<bool>>(async i =>
		{
            await Task.Delay(TimeSpan.FromMilliseconds(100));
            _testOutputHelper.WriteLine("Hello from test");

            return true;
        }), new DelegateModuleCatalogOptions()
		{
			TypeName = "HelloThereType"
		});

		await catalog.Initialize();

		var moduleType = catalog.Single().Type;

		Assert.Equal("HelloThereType", moduleType.Name);
	}

	[Fact]
	public async Task CanConfigureTypeNameUsingGenerator()
	{
		var catalog = new DelegateModuleCatalog(new Func<int, Task<bool>>(async i =>
		{
            await Task.Delay(TimeSpan.FromMilliseconds(100));
            _testOutputHelper.WriteLine("Hello from test");

            return true;
        }), new DelegateModuleCatalogOptions()
		{
			TypeNameGenerator = options => "GeneratorTypeName"
		});

		await catalog.Initialize();

		var moduleType = catalog.Single().Type;

		Assert.Equal("GeneratorTypeName", moduleType.Name);
	}

	[Fact]
	public async Task CanConfigureGeneratedMethodName()
	{
		var options = new DelegateModuleCatalogOptions()
		{
			MethodName = "HelloMethod"
		};

		var catalog = new DelegateModuleCatalog(new Func<int, Task<bool>>(async i =>
		{
            await Task.Delay(TimeSpan.FromMilliseconds(100));
            _testOutputHelper.WriteLine("Hello from test");

            return true;
        }), options);

		await catalog.Initialize();

		var moduleType = catalog.Single().Type;
		var methodInfos = moduleType.GetMethods(BindingFlags.Instance | BindingFlags.Public | BindingFlags.DeclaredOnly);

		Assert.Equal("HelloMethod", methodInfos.Single().Name);
	}

	[Fact]
	public async Task CanConfigureGeneratedMethodNameUsingGenerator()
	{
		var options = new DelegateModuleCatalogOptions()
		{
			MethodNameGenerator = catalogOptions => "MethodGeneratorName"
		};

		var catalog = new DelegateModuleCatalog(new Func<int, Task<bool>>(async i =>
		{
            await Task.Delay(TimeSpan.FromMilliseconds(100));
            _testOutputHelper.WriteLine("Hello from test");

            return true;
        }), options);

		await catalog.Initialize();

		var moduleType = catalog.Single().Type;
		var methodInfos = moduleType.GetMethods(BindingFlags.Instance | BindingFlags.Public | BindingFlags.DeclaredOnly);

		Assert.Equal("MethodGeneratorName", methodInfos.Single().Name);
	}

	[Fact]
	public async Task ByDefaultNoConstructorParameters()
	{
		var catalog = new DelegateModuleCatalog(new Func<int, Task<bool>>(async i =>
		{
            await Task.Delay(TimeSpan.FromMilliseconds(100));
            _testOutputHelper.WriteLine("Hello from test");

            return true;
        }));

		await catalog.Initialize();

		var moduleType = catalog.Single().Type;

		Assert.Single(moduleType.GetConstructors());

		foreach (var constructorInfo in moduleType.GetConstructors())
			Assert.Empty(constructorInfo.GetParameters());
	}

	[Fact]
	public async Task CanConvertParameterToProperty()
	{
		var rules = new List<DelegateConversionRule>()
		{
			new DelegateConversionRule(info => info.ParameterType == typeof(int), nfo => new ParameterConversion()
			{
				ToPublicProperty= true
			})
		};

		var catalog = new DelegateModuleCatalog(new Func<int, Task<bool>>(async i =>
		{
            await Task.Delay(TimeSpan.FromMilliseconds(100));
            _testOutputHelper.WriteLine("Hello from test");

            return true;
        }), rules);

		await catalog.Initialize();

		var moduleType = catalog.Single().Type;

		Assert.Single(moduleType.GetProperties());
	}

	[Fact]
	public async Task CanConvertParameterToConstructorParameter()
	{
		var rules = new List<DelegateConversionRule>()
		{
			new DelegateConversionRule(info => info.ParameterType == typeof(int), nfo => new ParameterConversion()
			{
				ToConstructor = true
			})
		};

		var catalog = new DelegateModuleCatalog(new Func<int, Task<bool>>(async i =>
		{
            await Task.Delay(TimeSpan.FromMilliseconds(100));
            _testOutputHelper.WriteLine("Hello from test");

            return true;
        }), rules);

		await catalog.Initialize();

		var moduleType = catalog.Single().Type;

		Assert.Single(moduleType.GetConstructors());
		Assert.Single(moduleType.GetConstructors().Single().GetParameters());
	}

	[Fact]
	public async Task CanConvertMultipleParametersToConstructorAndPropertyParameters()
	{
		var rules = new List<DelegateConversionRule>()
		{
			new DelegateConversionRule(info => info.ParameterType == typeof(int), nfo => new ParameterConversion()
			{
				ToConstructor = true
			}),
			new DelegateConversionRule(info => info.ParameterType == typeof(string), nfo => new ParameterConversion()
			{
				ToPublicProperty = true
			}),
			new DelegateConversionRule(info => info.ParameterType == typeof(bool), nfo => new ParameterConversion()
			{
				ToPublicProperty = true
			}),
			new DelegateConversionRule(info => info.ParameterType == typeof(decimal), nfo => new ParameterConversion()
			{
				ToConstructor = true
			})
		};

		var catalog = new DelegateModuleCatalog(new Func<int, string, bool, decimal, char, bool>((i, s, arg3, arg4, c) =>
		{
			_testOutputHelper.WriteLine("Hello from test");

			return true;
		}), rules);

		await catalog.Initialize();

		var moduleType = catalog.Single().Type;

		Assert.Single(moduleType.GetConstructors());
		Assert.Equal(2, moduleType.GetConstructors().Single().GetParameters().Length);
		Assert.Equal(2, moduleType.GetProperties().Length);

		dynamic obj = Activator.CreateInstance(moduleType, new object[] { 30, new decimal(22) });
        obj.S = "hello";
		obj.Arg3 = true;

		var res = obj.Run('g');

		Assert.True(res);
	}

	[Fact]
	public async Task CanSetModuleName()
	{
		var catalog = new DelegateModuleCatalog(new Action(() =>
		{
			_testOutputHelper.WriteLine("Hello from test");
		}), nameOptions: new()
		{
			ModuleNameGenerator = (options, type) => "CustomModule"
		});

		await catalog.Initialize();

		var module = catalog.Single();

		Assert.Equal("CustomModule", module.Name);
	}

	[Fact]
	public async Task CanSetModuleNameAndVersion()
	{
		var catalog = new DelegateModuleCatalog(new Action(() =>
		{
			_testOutputHelper.WriteLine("Hello from test");
		}), nameOptions: new()
		{
			ModuleNameGenerator = (options, type) => "CustomModule",
			ModuleVersionGenerator = (options, type) => new Version(2, 3, 5)
		});

		await catalog.Initialize();

		var module = catalog.Single();

		Assert.Equal("CustomModule", module.Name);
		Assert.Equal(new Version(2, 3, 5), module.Version);
	}

	[Fact]
	public async Task CanConfigureModulesHaveUniqueNames()
	{
		var randomGenerator = new Random(Guid.NewGuid().GetHashCode());
		var nameOptions = new ModuleNameOptions()
		{
			ModuleNameGenerator = (options, type) => $"CustomModule{randomGenerator.Next(int.MaxValue)}"
		};

		var catalog = new DelegateModuleCatalog(new Action(() =>
		{
			_testOutputHelper.WriteLine("Hello from test");
		}), nameOptions: nameOptions);

		var catalog2 = new DelegateModuleCatalog(new Action(() =>
		{
			_testOutputHelper.WriteLine("Hello from test");
		}), nameOptions: nameOptions);

		await catalog.Initialize();
		await catalog2.Initialize();

		var firstModule = catalog.Single();
		var secondModule = catalog2.Single();

		Assert.NotEqual(firstModule.Name, secondModule.Name);
	}
}
