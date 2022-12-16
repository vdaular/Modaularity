using Microsoft.Extensions.Configuration;
using Modaularity.Abstractions;

namespace Modaularity.Configuration.Converters;

public interface IConfigurationToCatalogConverter
{
    bool CanConvert(string type);
    IModuleCatalog Convert(IConfigurationSection section);
}
