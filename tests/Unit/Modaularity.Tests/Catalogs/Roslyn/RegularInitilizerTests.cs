using Modaularity.Catalogs.Roslyn;

namespace Modaularity.Tests.Catalogs.Roslyn;

public class RegularInitilizerTests
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

        await Assert.ThrowsAsync<InvalidCodeException>(async () => await TestHelpers.CompileRegular(invalidCode));
    }

    [Fact]
    public async Task ThrowsWithEmptyScript()
    {
        var invalidCode = "";

        await Assert.ThrowsAsync<ArgumentOutOfRangeException>(async () => await TestHelpers.CompileRegular(invalidCode));
    }

    [Fact]
    public async Task CanCompileCode()
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

        await TestHelpers.CompileRegular(code);
    }

    [Fact]
    public async Task CanAddReference()
    {
        var code =
            """
                public class MyClass
                {
                    public void RunThings()
                    {
                        Newtonsoft.Json.JsonConvert.SerializeObject(15);
                    }
                }
            """;

        var options = new RoslynModuleCatalogOptions()
        {
            AdditionalReferences = new()
            {
                typeof(Newtonsoft.Json.JsonConvert).Assembly
            }
        };

        await TestHelpers.CompileRegular(code, options);
    }

    [Fact]
    public async Task CanAddNamespace()
    {
        var code =
            """
                public class MyClass
                {
                    public void RunThings()
                    {
                        JsonConvert.SerializeObject(15);
                    }
                }
            """;

        var options = new RoslynModuleCatalogOptions()
        {
            AdditionalReferences = new()
            {
                typeof(Newtonsoft.Json.JsonConvert).Assembly
            },
            AdditionalNamespaces = new()
            {
                "Newtonsoft.Json"
            }
        };

        await TestHelpers.CompileRegular(code, options);
    }
}
