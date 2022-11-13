namespace Modaularity.Abstractions;

public class ModuleFrameworkOptions
{
    public bool UseConfiguration { get; set; } = true;
    public string ConfigurationSection { get; set; } = "ModuleFramework";
}
