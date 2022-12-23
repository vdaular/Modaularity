using Modaularity.Catalogs.Roslyn;

namespace Modaularity.Tests.Catalogs.Roslyn;

public class ScriptCodeInitializerTests
{
    [Fact]
    public async Task ThrowsWithInvalidScript()
    {
        var invalidCode = "not c#";

        await Assert.ThrowsAsync<InvalidCodeException>(async () => await TestHelpers.CompileScript(invalidCode));
    }

    [Fact]
    public async Task ThrowsWithEmptyScript()
    {
        var invalidCode = "";

        await Assert.ThrowsAsync<ArgumentOutOfRangeException>(async () => await TestHelpers.CompileScript(invalidCode));
    }

    [Fact]
    public async Task CanInitVoid()
    {
        var code = "Debug.WriteLine(\"Hello world!\");";
        var types = await TestHelpers.CompileScript(code);

        Assert.Single(types);

        var method = types.First().GetMethods().First();

        Assert.True(method.ReturnParameter.ParameterType == typeof(Task));
    }

    [Fact]
    public async Task CanInitVoidWithParameter()
    {
        var code = "string dah { get; set; } = \"Hello\"; var i = 5; Debug.WriteLine(dah);";
        var types = await TestHelpers.CompileScript(code);

        Assert.Single(types);

        var method = types.First().GetMethods().First();
        var methodParameters = method.GetParameters().ToList();

        Assert.Single(methodParameters);
        Assert.True(methodParameters.Single().ParameterType == typeof(string));
    }

    [Fact]
    public async Task CanInitVoidWithMultipleParameters()
    {
        var code = "int y { get; set; } = 5; int x { get; set; } = 20; x = y + 20; Debug.WriteLine(x);";
        var types = await TestHelpers.CompileScript(code);

        Assert.Single(types);

        var method = types.First().GetMethods().First();
        var methodParameters = method.GetParameters().ToList();

        Assert.Equal(2, methodParameters.Count);
        Assert.True(methodParameters.First().ParameterType == typeof(int));
    }

    [Fact]
    public async Task CanInitReturnString()
    {
        var code = "var i = 15; var x = \"Hello\"; return x;";
        var types = await TestHelpers.CompileScript(code);

        Assert.Single(types);

        var method = types.First().GetMethods().First();
        var returnParameter = method.ReturnParameter;

        Assert.NotNull(returnParameter);
        Assert.Equal(typeof(Task<string>), returnParameter.ParameterType);
    }

    [Fact]
    public async Task CanInitReturnStringWithParameters()
    {
        var code = "int y { get; set; } = 5; int x { get; set; } = 20; x = y + 20; Debug.WriteLine(x); return x.ToString();";
        var types = await TestHelpers.CompileScript(code);
        var method = types.First().GetMethods().First();
        var returnParameter = method.ReturnParameter;

        Assert.NotNull(returnParameter);
        Assert.Equal(typeof(Task<string>), returnParameter.ParameterType);
    }

    [Fact]
    public async Task CanNotInitReturnValueTuple()
    {
        var code = "var i = 15; var x = \"Hello\"; return (i, x);";
        
        await Assert.ThrowsAsync<InvalidCodeException>(() => TestHelpers.CompileScript(code));
    }

    [Fact]
    public async Task ThrowsWithMissingTypeNameGenerator()
    {
        var code = "var x = \"Hello\"; return x;";
        var options = new RoslynModuleCatalogOptions()
        {
            TypeNameGenerator = null
        };

        await Assert.ThrowsAsync<ArgumentNullException>(async () => await TestHelpers.CompileScript(code, options));
    }

    [Fact]
    public async Task ThrowsWithMissingNamespaceNameGenerator()
    {
        var code = "var x = \"Hello\"; return x;";
        var options = new RoslynModuleCatalogOptions()
        {
            NamespaceNameGenerator = null
        };

        await Assert.ThrowsAsync<ArgumentNullException>(async () => await TestHelpers.CompileScript(code, options));
    }

    [Fact]
    public async Task HasDefaultTypeName()
    {
        var code = "var x = \"Hello\"; return x;";
        var type = (await TestHelpers.CompileScript(code)).First();
        var defaultOptions = new RoslynModuleCatalogOptions();

        Assert.Equal(defaultOptions.TypeName, type.Name);
    }

    [Fact]
    public async Task HasDefaultNamespace()
    {
        var code = "var x = \"Hello\"; return x;";
        var type = (await TestHelpers.CompileScript(code)).First();
        var defaultOptions = new RoslynModuleCatalogOptions();

        Assert.Equal(defaultOptions.NamespaceName, type.Namespace);
    }

    [Fact]
    public async Task DefaultsToReturningTask()
    {
        var code = "var x = \"Hello\"; return x;";
        var type = (await TestHelpers.CompileScript(code)).First();
        var method = type.GetMethods().First();

        Assert.Equal(typeof(Task<string>), method.ReturnParameter.ParameterType);
    }

    [Fact]
    public async Task HasDefaultMethodName()
    {
        var code = "var x = \"Hello\"; return x;";
        var type = (await TestHelpers.CompileScript(code)).First();
        var method = type.GetMethods().First();

        Assert.Equal("Run", method.Name);
    }

    [Fact]
    public async Task CanConfigureMethodName()
    {
        var code = "var x = \"Hello\"; return x;";
        var options = new RoslynModuleCatalogOptions() 
        { 
            MethodName = "Execute"
        };

        var type = (await TestHelpers.CompileScript(code, options)).First();
        var method = type.GetMethods().First();

        Assert.Equal("Execute", method.Name);
    }

    [Fact]
    public async Task CanConfigureMethodNameGenerator()
    {
        var code = "var x = \"Hello\"; return x;";
        var options = new RoslynModuleCatalogOptions()
        {
            MethodNameGenerator = catalogOptions => "MyMethod"
        };

        var type = (await TestHelpers.CompileScript(code, options)).First();
        var method = type.GetMethods().First();

        Assert.Equal("MyMethod", method.Name);
    }

    [Fact]
    public async Task CanConfigureTypeName()
    {
        var code = "var x = \"Hello\"; return x;";
        var options = new RoslynModuleCatalogOptions()
        {
            TypeName = "MyTest"
        };

        var type = (await TestHelpers.CompileScript(code, options)).First();
        var method = type.GetMethods().First();

        Assert.Equal("MyTest", type.Name);
    }
}
