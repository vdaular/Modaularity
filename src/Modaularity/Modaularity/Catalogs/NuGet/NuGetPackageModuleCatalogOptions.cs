using DotNetNuGetDownloader;
using Modaularity.Abstractions;
using Modaularity.TypeFinding;
using NuGet.Common;

namespace Modaularity.Catalogs.NuGet;

public class NuGetPackageModuleCatalogOptions
{
    public Func<ILogger> LoggerFactory { get; set; } = Defaults.LoggerFactory;
    public TypeFinderOptions TypeFinderOptions { get; set; } = new();
    public ModuleNameOptions ModuleNameOptions { get; set; } = Defaults.ModuleNameOptions;
    public bool IncludeSystemFeedsAsSecondary { get; set; } = false;
    public string TargetFramework { get; set; } = Defaults.TargetFramework;
    public bool ForcePackageCaching { get; set; } = false;
    public string CustomPackagesFolder { get; set; } = string.Empty;
    public bool AutoRetryPackageDownload { get; set; } = false;

    public static class Defaults
    {
        public static Func<ILogger> LoggerFactory { get; set; } = () => new ConsoleLogger();
        public static ModuleNameOptions ModuleNameOptions { get; set; } = new();
        public static string TargetFramework { get; set; } = string.Empty;
    }
}
