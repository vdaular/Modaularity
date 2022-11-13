using Microsoft.Extensions.Configuration;
using Modaularity.Abstractions;

namespace Modaularity.Configuration.Converters;

public interface IConfigurationCatalogConverter
{
    bool CanConvert(string type);
    IModuleCatalog Convert(IConfigurationSection section);
}
