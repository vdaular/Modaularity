using Microsoft.Extensions.Configuration;
using Modaularity.Abstractions;
using Modaularity.Catalogs.Assemblies;

namespace Modaularity.Configuration.Converters;

public class AssemblyCatalogConfigurationConverter : IConfigurationToCatalogConverter
{
    public bool CanConvert(string type)
        => string.Equals(type, "Assembly", StringComparison.InvariantCultureIgnoreCase);

    public IModuleCatalog Convert(IConfigurationSection section)
    {
        string? path = section.GetValue<string>("Path");

        return new AssemblyModuleCatalog(path);
    }
}
