using DotNetTypeGenerator;
using System.Reflection;
using System.Text;

namespace Modaularity.Catalogs.Roslyn;

public class RegularCodeInitializer
{
    private readonly string _code;
    private readonly RoslynModuleCatalogOptions _options;

    public RegularCodeInitializer(string code, RoslynModuleCatalogOptions options)
    {
        if (string.IsNullOrWhiteSpace(code))
            throw new ArgumentOutOfRangeException(nameof(code), code, "El script no puede ser null o vacío");

        _code = code;
        _options = options ?? new();
    }

    public Task<Assembly> CreateAssembly()
    {
        try
        {
            var generator = new AssemblyGenerator();
            generator.ReferenceAssemblyContainingType<Action>();

            if (_options.AdditionalReferences?.Any() == true)
                foreach (var assembly in _options.AdditionalReferences)
                    generator.ReferenceAssembly(assembly);

            var code = new StringBuilder();
            code.AppendLine("using System;");
            code.AppendLine("using System.Diagnostics;");
            code.AppendLine("using System.Threading.Tasks;");
            code.AppendLine("using System.Text;");
            code.AppendLine("using System.Collections;");
            code.AppendLine("using System.Collections.Generic;");

            if (_options.AdditionalNamespaces?.Any() == true)
                foreach (var ns in _options.AdditionalNamespaces)
                    code.AppendLine($"using {ns};");

            code.AppendLine(_code);
            
            var assemblySourceCode = code.ToString();
            var result = generator.GenerateAssembly(assemblySourceCode);

            return Task.FromResult(result);
        }
        catch (Exception ex)
        {
            throw new InvalidCodeException($"No es posible crear el ensamblado desde el código regular. Código: {_code}", ex);
        }
    }
}
