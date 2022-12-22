using Modaularity.Abstractions;
using Modaularity.Catalogs.Roslyn;
using System.Diagnostics;

namespace Modaularity.Tests.Catalogs.Roslyn;

public class RoslynModuleCatalogTests
{
    [Fact]
    public async Task ThrowsWithInvalidCode()
    {
        var invalidCode =
            """
                public class MyClass
                {
                    public void RunThings
                    {
                        var y = 0;
                        var a = 1;

                        a = y + 10;

                        Debug.WriteLine(y + a);
                    }
                }
            """;

        await Assert.ThrowsAsync<InvalidCodeException>(async () => await TestHelpers.CreateCatalog(invalidCode));
    }

    [Fact]
    public async Task ThrowsWithEmptyScript()
    {
        var invalidCode = "";

        await Assert.ThrowsAsync<ArgumentOutOfRangeException>(async () => await TestHelpers.CreateCatalog(invalidCode));
    }

    [Fact]
    public async Task CanHandleScript()
    {
        var code = "Debug.WriteLine(\"Hello world!\");";
        var types = await TestHelpers.CreateCatalog(code);

        Assert.Single(types);

        var method = types.First().GetMethods().First();

        Assert.Equal(typeof(Task), method.ReturnParameter.ParameterType);
    }

    [Fact]
    public async Task ScriptAssemblyContainsValidVersion()
    {
        var code = "Debug.WriteLine(\"Hello world!\");";
        var type = (await TestHelpers.CreateCatalog(code)).Single();
        var versionInfo = FileVersionInfo.GetVersionInfo(type.Assembly.Location);
        var fileVersion = versionInfo.FileVersion;

        Assert.NotNull(fileVersion);
    }

    [Fact]
    public async Task ScriptContainsVersion1000ByDefault()
    {
        var code = "Debug.WriteLine(\"Hello world!\");";
        var type = (await TestHelpers.CreateCatalog(code)).Single();
        var versionInfo = FileVersionInfo.GetVersionInfo(type.Assembly.Location);
        var fileVersion = versionInfo.FileVersion;

        Assert.Equal("1.0.0.0", fileVersion);
    }

    [Fact]
    public async Task CanHandleRegular()
    {
        var code =
            """
                public class MyClass
                {
                    public void RunThings()
                    {
                        var y = 0;
                        var a = 1;

                        a = y + 10;

                        Debug.WriteLine(y + a);
                    }
                }
            """;

        var types = await TestHelpers.CompileRegular(code);

        Assert.Single(types);

        var method = types.First().GetMethods().First();

        Assert.Equal(typeof(void), method.ReturnParameter.ParameterType);
    }

    [Fact]
    public async Task CanCreateModuleWithoutName()
    {
        var code =
           """
                public class MyClass
                {
                    public void RunThings()
                    {
                        var y = 0;
                        var a = 1;

                        a = y + 10;

                        Debug.WriteLine(y + a);
                    }
                }
            """;

        var catalog = new RoslynModuleCatalog(code);

        await catalog.Initialize();

        var modules = catalog.GetModules();
        var firstModuleName = modules.First().Name;

        Assert.NotEmpty(firstModuleName);
    }

    [Fact]
    public async Task CanCreateModuleWithName()
    {
        var code =
           """
                public class MyClass
                {
                    public void RunThings()
                    {
                        var y = 0;
                        var a = 1;

                        a = y + 10;

                        Debug.WriteLine(y + a);
                    }
                }
            """;

        var options = new RoslynModuleCatalogOptions()
        {
            ModuleName = "MyModule"
        };

        var catalog = new RoslynModuleCatalog(code, options);

        await catalog.Initialize();

        var module = catalog.Get("MyModule", new Version(1, 0, 0, 0));

        Assert.NotNull(module);
    }

    [Fact]
    public async Task ModuleDefaultsToVersion1000()
    {
        var code =
           """
                public class MyClass
                {
                    public void RunThings()
                    {
                        var y = 0;
                        var a = 1;

                        a = y + 10;

                        Debug.WriteLine(y + a);
                    }
                }
            """;

        var options = new RoslynModuleCatalogOptions()
        {
            ModuleName = "MyModule"
        };

        var catalog = new RoslynModuleCatalog(code, options);

        await catalog.Initialize();

        var module = catalog.Get("MyModule", new Version(1, 0, 0, 0));

        Assert.Equal(new Version(1, 0, 0, 0), module.Version);
    }

    [Fact]
    public async Task CanCreateModuleWithNameAndVersion()
    {
        var code =
           """
                public class MyClass
                {
                    public void RunThings()
                    {
                        var y = 0;
                        var a = 1;

                        a = y + 10;

                        Debug.WriteLine(y + a);
                    }
                }
            """;

        var options = new RoslynModuleCatalogOptions()
        {
            ModuleName = "MyModule",
            ModuleVersion = new Version(1, 1)
        };

        var catalog = new RoslynModuleCatalog(code, options);

        await catalog.Initialize();

        var module = catalog.Get("MyModule", new Version(1, 1));

        Assert.NotNull(module);
    }

    [Fact]
    public async Task CanGetAllFromCatalog()
    {
        var code =
           """
                public class MyClass
                {
                    public void RunThings()
                    {
                        var y = 0;
                        var a = 1;

                        a = y + 10;

                        Debug.WriteLine(y + a);
                    }
                }
            """;

        var catalog = new RoslynModuleCatalog(code);

        await catalog.Initialize();

        var modules = catalog.GetModules();
        var module = modules.FirstOrDefault();

        Assert.NotNull(module);
    }

    [Fact]
    public async Task CanCreateModuleNameWithGenerator()
    {
        var code =
           """
                public class MyClass
                {
                    public void RunThings()
                    {
                        var y = 0;
                        var a = 1;

                        a = y + 10;

                        Debug.WriteLine(y + a);
                    }
                }
            """;

        var options = new RoslynModuleCatalogOptions()
        {
            ModuleNameOptions = new ModuleNameOptions()
            {
                ModuleNameGenerator = (nameOptions, type) => "HelloThereFromGenerator"
            }
        };

        var catalog = new RoslynModuleCatalog(code, options);

        await catalog.Initialize();

        var module = catalog.Get("HelloThereFromGenerator", Version.Parse("1.0.0.0"));

        Assert.NotNull(module);
    }

    [Fact]
    public async Task CanCreateModuleVersionWithGenerator()
    {
        var code =
           """
                public class MyClass
                {
                    public void RunThings()
                    {
                        var y = 0;
                        var a = 1;

                        a = y + 10;

                        Debug.WriteLine(y + a);
                    }
                }
            """;

        var options = new RoslynModuleCatalogOptions()
        {
            ModuleNameOptions = new ModuleNameOptions()
            {
                ModuleVersionGenerator = (nameOptions, type) => new Version(2, 0, 0)
            }
        };

        var catalog = new RoslynModuleCatalog(code, options);

        await catalog.Initialize();

        var module = catalog.Single();

        Assert.Equal(new Version(2, 0, 0), module.Version);
    }

    [Fact]
    public async Task CanTagCode()
    {
        var code =
           """
                public class MyClass
                {
                    public void RunThings()
                    {
                        var y = 0;
                        var a = 1;

                        a = y + 10;

                        Debug.WriteLine(y + a);
                    }
                }
            """;

        var catalog = new RoslynModuleCatalog(code, new()
        {
            Tags = new()
            {
                "CustomTag"
            }
        });

        await catalog.Initialize();

        var module = catalog.Single();

        Assert.Equal("CustomTag", module.Tag);
    }

    [Fact]
    public async Task CanTagScript()
    {
        var code = "Debug.WriteLine(\"Hello world!\");";
        var catalog = new RoslynModuleCatalog(code, new()
        {
            Tags = new()
            {
                "CustomTag"
            }
        });

        await catalog.Initialize();

        var module = catalog.Single();

        Assert.Equal("CustomTag", module.Tag);
    }
}
