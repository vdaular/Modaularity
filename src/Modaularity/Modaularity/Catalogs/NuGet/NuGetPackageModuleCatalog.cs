using DotNetNuGetDownloader;
using Modaularity.Abstractions;
using Modaularity.Catalogs.Assemblies;
using Modaularity.Context;
using Modaularity.TypeFinding;
using System.Text.Json;

namespace Modaularity.Catalogs.NuGet;

public class NuGetPackageModuleCatalog : IModuleCatalog
{
    private readonly NuGetFeed? _packageFeed;
    private readonly string? _packageName;
    private readonly string? _packageVersion;
    private readonly bool _includePrerelease;

    private readonly HashSet<string> _moduleAssemblyFilePaths = new();
    private readonly List<AssemblyModuleCatalog> _moduleCatalogs = new();
    private readonly NuGetPackageModuleCatalogOptions? _options;

    public string PackagesFolder { get; }

    private bool HasCustomPackagesFolder
    {
        get
        {
            return string.IsNullOrWhiteSpace(PackagesFolder) == false;
        }
    }

    private bool ForcePackageCache
    {
        get
        {
            if (HasCustomPackagesFolder == false)
                return false;

            if (_options?.ForcePackageCaching != true)
                return false;

            return true;
        }
    }

    private string? NuGetDownloadResultFilePath
    {
        get
        {
            if (string.IsNullOrWhiteSpace(PackagesFolder))
                return null;

            var result = Path.Combine(PackagesFolder, ".nuGetDownloadResult.json");

            return result;
        }
    }

    public NuGetPackageModuleCatalog(string packageName, string? packageVersion = null, 
        bool includePrerelease = false, NuGetFeed? packageFeed = null, string? packagesFolder = null, 
        Action<TypeFinderCriteriaBuilder>? configureFinder = null, 
        Dictionary<string, TypeFinderCriteria>? criterias = null, 
        NuGetPackageModuleCatalogOptions? options = null)
    {
        _packageName = packageName;
        _packageVersion = packageVersion;
        _includePrerelease = includePrerelease;
        _packageFeed = packageFeed;

        PackagesFolder = packagesFolder ?? options?.CustomPackagesFolder;

        if (string.IsNullOrWhiteSpace(PackagesFolder))
            PackagesFolder = Path.Combine(Path.GetTempPath(), "NuGetPackageModuleCatalog", Path.GetRandomFileName());

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
        foreach (var assemblyModuleCatalog in _moduleCatalogs)
        {
            var result = assemblyModuleCatalog.Get(name, version);

            if (result == null)
                continue;

            return result;
        }

        return null;
    }

    public List<Module> GetModules() => _moduleCatalogs.SelectMany(x => x.GetModules()).ToList();

    public async Task Initialize()
    {
        NuGetDownloadResult? nuGetDonwloadResult = null;

        var logger = _options.LoggerFactory();

        if (ForcePackageCache && File.Exists(NuGetDownloadResultFilePath))
        {
            try
            {
                var jsonFromDisk = await File.ReadAllTextAsync(NuGetDownloadResultFilePath);

                nuGetDonwloadResult = JsonSerializer.Deserialize<NuGetDownloadResult?>(jsonFromDisk);

                logger?.LogDebug($"Se utiliza el paquete previamente descargado desde {PackagesFolder}");
            }
            catch (Exception ex)
            {
                logger?.LogDebug($"Falló la deserialización de NuGetDownloadResult desde la ruta {NuGetDownloadResultFilePath}: {ex}");
            }
        }

        if (nuGetDonwloadResult == null)
        {
            var nuGetDownloader = new NuGetDownloader(_options.LoggerFactory());

            nuGetDonwloadResult = await nuGetDownloader.DownloadAsync(PackagesFolder, _packageName, _packageVersion,
                _includePrerelease, _packageFeed, includeSecondaryRepositories: _options.IncludeSystemFeedsAsSecondary,
                targetFramework: _options.TargetFramework, autoRetryOnFail: _options.AutoRetryPackageDownload)
                .ConfigureAwait(false);
        }

        foreach (var f in nuGetDonwloadResult.PackageAssemblyFiles)
            _moduleAssemblyFilePaths.Add(Path.Combine(PackagesFolder, f));

        foreach (var moduleAssemblyFilePath in _moduleAssemblyFilePaths)
        {
            var options = new AssemblyModuleCatalogOptions
            {
                TypeFinderOptions = _options.TypeFinderOptions,
                ModuleNameOptions = _options.ModuleNameOptions
            };

            var downloadedRuntimeDlls = nuGetDonwloadResult.RuntimeDlls.Where(x => x.IsRecomended).ToList();
            var runtimeAssemblyHints = new List<RuntimeAssemblyHint>();

            foreach (var runtimeDll in downloadedRuntimeDlls)
            {
                var runtimeAssembly = new RuntimeAssemblyHint(runtimeDll.FileName, runtimeDll.FullFilePath,
                    runtimeDll.IsNative);
                runtimeAssemblyHints.Add(runtimeAssembly);
            }

            options.ModuleLoadContextOptions.RuntimeAssemblyHints = runtimeAssemblyHints;

            var assemblyCatalog = new AssemblyModuleCatalog(moduleAssemblyFilePath, options);
            await assemblyCatalog.Initialize();

            _moduleCatalogs.Add(assemblyCatalog);
        }

        IsInitialized = true;

        if (ForcePackageCache)
        {
            try
            {
                var jsonToWrite = JsonSerializer.Serialize(nuGetDonwloadResult, 
                    options: new JsonSerializerOptions { WriteIndented = true });

                await File.WriteAllTextAsync(NuGetDownloadResultFilePath, jsonToWrite);

                logger?.LogDebug($"Se almacenan los detalles del paquete descargado en {NuGetDownloadResultFilePath}");
            }
            catch (Exception ex)
            {
                logger?.LogError($"Falló el almacenamiento de los detalles del paquete descargado en {NuGetDownloadResultFilePath}: {ex}");
            }
        }
    }
}
