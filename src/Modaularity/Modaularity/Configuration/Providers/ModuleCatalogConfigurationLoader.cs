using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using Modaularity.Abstractions;

namespace Modaularity.Configuration.Providers;

public class ModuleCatalogConfigurationLoader : IModuleCatalogConfigurationLoader
{
    private ModaularityOptions _options;

    public virtual string CatalogsKey => "Catalogs";

    public ModuleCatalogConfigurationLoader(IOptions<ModaularityOptions> options)
    {
        _options = options.Value;
    }

    public List<CatalogConfiguration> GetCatalogConfigurations(IConfiguration configuration)
    {
        var catalogs = new List<CatalogConfiguration>();

        configuration.Bind($"{_options.ConfigurationSection}:{CatalogsKey}", catalogs);

        return catalogs;
    }
}
