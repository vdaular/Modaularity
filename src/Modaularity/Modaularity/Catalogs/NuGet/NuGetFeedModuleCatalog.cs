using DotNetNuGetDownloader;
using Modaularity.Abstractions;
using Modaularity.TypeFinding;

namespace Modaularity.Catalogs.NuGet;

public class NuGetFeedModuleCatalog : IModuleCatalog
{
    private readonly NuGetFeed _packageFeed;
    private readonly string? _searchTerm;
    private readonly int _maxPackages;
    private readonly bool _includePrerelease;

    private readonly HashSet<string> _moduleAssemblyNames = new();
    private List<NuGetPackageModuleCatalog> _moduleCatalogs = new();
    private readonly NuGetFeedModuleCatalogOptions _options;

    public string? PackagesFolder { get; }

    public NuGetFeedModuleCatalog(NuGetFeed packageFeed, string? searchTerm = null, bool includePrerelease = false,
        int maxPackages = 128, string? packagesFolder = null, Action<TypeFinderCriteriaBuilder>? configureFinder = null,
        Dictionary<string, TypeFinderCriteria>? criterias = null, NuGetFeedModuleCatalogOptions? options = null)
    {
        _packageFeed = packageFeed;
        _searchTerm = searchTerm;
        _includePrerelease = includePrerelease;
        _maxPackages = maxPackages;

        PackagesFolder = packagesFolder ?? Path.Combine(Path.GetTempPath(), "NuGetFeedModuleCatalog", Path.GetRandomFileName());

        if (!Directory.Exists(PackagesFolder))
            Directory.CreateDirectory(PackagesFolder);

        if (criterias == null)
            criterias = new();

        _options = options ?? new();

        if (configureFinder != null)
        {
            var builder = new TypeFinderCriteriaBuilder();
            configureFinder(builder);

            var criteria = builder.Build();

            _options.TypeFinderOptions.TypeFinderCriterias.Add(criteria);
        }

        foreach (var finderCriteria in criterias)
        {
            finderCriteria.Value.Tags = new() { finderCriteria.Key };

            _options.TypeFinderOptions.TypeFinderCriterias.Add(finderCriteria.Value);
        }
    }

    public bool IsInitialized { get; private set; }

    public Module? Get(string name, Version version)
    {
        foreach (var moduleCatalog in _moduleCatalogs)
        {
            var result = moduleCatalog.Get(name, version);

            if (result == null)
                continue;

            return result;
        }

        return null;
    }

    public List<Module> GetModules() => _moduleCatalogs.SelectMany(x => x.GetModules()).ToList();

    public async Task Initialize()
    {
        var nuGetDownloader = new NuGetDownloader(_options.LoggerFactory());
        var packages = await nuGetDownloader.SearchPackagesAsync(_packageFeed, _searchTerm, _maxPackages);

        foreach (var packageAndRepo in packages)
        {
            var options = new NuGetPackageModuleCatalogOptions()
            {
                TypeFinderOptions = _options.TypeFinderOptions,
                ModuleNameOptions = _options.ModuleNameOptions,
                ForcePackageCaching = _options.ForcePackageCaching,
                AutoRetryPackageDownload = _options.AutoRetryPackageDownload
            };

            var packageCatalog = new NuGetPackageModuleCatalog(packageAndRepo.Package.Identity.Id, 
                packageAndRepo.Package.Identity.Version.ToString(), _includePrerelease, _packageFeed, 
                PackagesFolder, options: options);

            await packageCatalog.Initialize();

            _moduleCatalogs.Add(packageCatalog);
        }

        IsInitialized = true;
    }
}
