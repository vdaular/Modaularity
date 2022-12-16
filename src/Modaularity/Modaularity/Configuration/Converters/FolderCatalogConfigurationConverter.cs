using Microsoft.Extensions.Configuration;
using Modaularity.Abstractions;
using Modaularity.Catalogs.Folders;

namespace Modaularity.Configuration.Converters;

public class FolderCatalogConfigurationConverter : IConfigurationToCatalogConverter
{

    public bool CanConvert(string type) 
        => string.Equals(type, "Folder", StringComparison.InvariantCultureIgnoreCase);

    public IModuleCatalog Convert(IConfigurationSection section)
    {
        var path = section.GetValue<string>("Path")
            ?? throw new ArgumentException("Modaularity FolderCatalog requiere una ruta");

        var options = new CatalogFolderOptions();
        section.Bind("Options", options);

        var folderOptions = new FolderModuleCatalogOptions();

        folderOptions.IncludeSubfolders = options.IncludeSubfolders ?? folderOptions.IncludeSubfolders;
        folderOptions.SearchPatterns = options.SearchPatterns ?? folderOptions.SearchPatterns;

        return new FolderModuleCatalog(path, folderOptions);
    }

    private class CatalogFolderOptions
    {
        public bool? IncludeSubfolders { get; set; }
        public List<string>? SearchPatterns { get; set; }
    }
}
