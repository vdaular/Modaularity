using Modaularity.Catalogs.Roslyn;

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
}
