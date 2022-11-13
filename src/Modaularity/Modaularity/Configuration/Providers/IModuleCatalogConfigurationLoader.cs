using Microsoft.Extensions.Configuration;

namespace Modaularity.Configuration.Providers;

public interface IModuleCatalogConfigurationLoader
{
    public string CatalogsKey { get; }

    List<CatalogConfiguration> GetCatalogConfigurations(IConfiguration configuration);
}
