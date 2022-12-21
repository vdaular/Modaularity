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
}
