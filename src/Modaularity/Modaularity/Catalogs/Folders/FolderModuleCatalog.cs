using Modaularity.Abstractions;
using Modaularity.Catalogs.Assemblies;
using Modaularity.Context;
using Modaularity.TypeFinding;
using System.Reflection;
using System.Reflection.PortableExecutable;
using System.Runtime.InteropServices;
using System.Runtime.Loader;
using Module = Modaularity.Abstractions.Module;

namespace Modaularity.Catalogs.Folders;

public class FolderModuleCatalog : IModuleCatalog
{
    private readonly string _folderPath;
    private readonly FolderModuleCatalogOptions _options;
    private readonly List<AssemblyModuleCatalog> _catalogs = new();

    public bool IsInitialized { get; private set; }

    private List<Module> Modules
    {
        get
        {
            return _catalogs.SelectMany(x => x.GetModules()).ToList();
        }
    }

    public FolderModuleCatalog(string folderPath, Action<TypeFinderCriteriaBuilder>? configureFinder, 
        TypeFinderCriteria? finderCriteria, FolderModuleCatalogOptions? options)
    {
        if (string.IsNullOrWhiteSpace(folderPath))
            throw new ArgumentNullException(nameof(folderPath));

        _folderPath = folderPath;
        _options = options ?? new();

        if (_options.TypeFinderOptions == null)
            _options.TypeFinderOptions = new();

        if (_options.TypeFinderOptions.TypeFinderCriterias == null)
            _options.TypeFinderOptions.TypeFinderCriterias = new();

        if (configureFinder != null)
        {
            var builder = new TypeFinderCriteriaBuilder();
            configureFinder(builder);

            var criteria = builder.Build();

            _options.TypeFinderOptions.TypeFinderCriterias.Add(criteria);
        }

        if (finderCriteria != null)
            _options.TypeFinderOptions.TypeFinderCriterias.Add(finderCriteria);

        if (_options.TypeFinderCriteria != null)
            _options.TypeFinderOptions.TypeFinderCriterias.Add(_options.TypeFinderCriteria);

        if (_options.TypeFinderCriterias?.Any() == true)
        {
            foreach (var typeFinderCriteria in _options.TypeFinderCriterias)
            {
                var crit = typeFinderCriteria.Value;
                crit.Tags = new() { typeFinderCriteria.Key };

                _options.TypeFinderOptions.TypeFinderCriterias.Add(crit);
            }
        }
    }

    public FolderModuleCatalog(string folderPath, Action<TypeFinderCriteriaBuilder>? configureFinder, 
        FolderModuleCatalogOptions? options) : this(folderPath, configureFinder, null, options) { }

    public FolderModuleCatalog(string folderPath, TypeFinderCriteria? finderCriteria, FolderModuleCatalogOptions? options)
        : this(folderPath, null, finderCriteria, options) { }

    public FolderModuleCatalog(string folderPath, TypeFinderCriteria? finderCriteria) 
        : this(folderPath, finderCriteria, null) { }

    public FolderModuleCatalog(string folderPath, Action<TypeFinderCriteriaBuilder>? configureFinder) 
        : this(folderPath, configureFinder, null, null) { }

    public FolderModuleCatalog(string folderPath, FolderModuleCatalogOptions? options) 
        : this(folderPath, null, null, options) { }

    public FolderModuleCatalog(string folderPath) : this(folderPath, new FolderModuleCatalogOptions()) { }

    public Module? Get(string name, Version version)
    {
        foreach (var assemblyModuleCatalog in _catalogs)
        {
            var module = assemblyModuleCatalog.Get(name, version);

            if (module == null)
                continue;

            return module;
        }

        return null;
    }

    public List<Module> GetModules() => Modules;

    public async Task Initialize()
    {
        var foundFiles = new List<string>();

        foreach (var searchPattern in _options.SearchPatterns)
        {
            var dllFiles = Directory.GetFiles(_folderPath, searchPattern,
                _options.IncludeSubfolders ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly);

            foundFiles.AddRange(dllFiles);
        }

        foundFiles = foundFiles.Distinct().ToList();

        foreach (var assemblyPath in foundFiles)
        {
            var isModuleAssembly = IsModuleAssembly(assemblyPath);

            if (isModuleAssembly == false)
                continue;

            var assemblyCatalogOptions = new AssemblyModuleCatalogOptions
            {
                ModuleLoadContextOptions = _options.ModuleLoadContextOptions,
                TypeFinderOptions = _options.TypeFinderOptions,
                ModuleNameOptions = _options.ModuleNameOptions
            };

            var assemblyCatalog = new AssemblyModuleCatalog(assemblyPath, assemblyCatalogOptions);
            await assemblyCatalog.Initialize();

            _catalogs.Add(assemblyCatalog);
        }
    }

    private bool IsModuleAssembly(string assemblyPath)
    {
        using (Stream stream = File.OpenRead(assemblyPath))
        using (var reader = new PEReader(stream))
        {
            if (!reader.HasMetadata)
                return false;

            if (_options.TypeFinderOptions?.TypeFinderCriterias?.Any() != true)
                return true;

            var runtimeDirectory = RuntimeEnvironment.GetRuntimeDirectory();
            var runtimeAssemblies = Directory.GetFiles(runtimeDirectory, "*.dll");
            var paths = new List<string>(runtimeAssemblies) { assemblyPath };

            if (_options.ModuleLoadContextOptions.AdditionalRuntimePaths?.Any() == true)
            {
                foreach (var additionalRuntimePath in _options.ModuleLoadContextOptions.AdditionalRuntimePaths)
                {
                    var dlls = Directory.GetFiles(additionalRuntimePath, "*.dll");
                    paths.AddRange(dlls);
                }
            }

            if (_options.ModuleLoadContextOptions.UseHostApplicationAssemblies == UseHostApplicationAssembliesEnum.Always)
            {
                var hostApplicationPath = Environment.CurrentDirectory;
                var hostDlls = Directory.GetFiles(hostApplicationPath, "*.dll", SearchOption.AllDirectories);

                paths.AddRange(hostDlls);
                
                AddSharedFrameworkDlls(hostApplicationPath, runtimeDirectory, paths);
            }
            else if (_options.ModuleLoadContextOptions.UseHostApplicationAssemblies == UseHostApplicationAssembliesEnum.Never)
            {
                var modulePath = Path.GetDirectoryName(assemblyPath);
                string[] dllsInModulePath = Directory.GetFiles(modulePath, "*.dll", SearchOption.AllDirectories);

                paths.AddRange(dllsInModulePath);
            }
            else if (_options.ModuleLoadContextOptions.UseHostApplicationAssemblies == UseHostApplicationAssembliesEnum.Selected)
            {
                foreach (var hostApplicationAssembly in _options.ModuleLoadContextOptions.HostApplicationAssemblies)
                {
                    var assembly = Assembly.Load(hostApplicationAssembly);
                    paths.Add(assembly.Location);
                }
            }

            paths = paths.Distinct().ToList();

            var duplicateDlls = paths.Select(x => new { FullPath = x, FileName = Path.GetFileName(x) })
                .GroupBy(x => x.FileName)
                .Where(x => x.Count() > 1)
                .ToList();

            var removed = new List<string>();

            foreach (var duplicateDll in duplicateDlls)
                foreach (var duplicateDllPath in duplicateDll.Skip(1))
                    removed.Add(duplicateDllPath.FullPath);

            foreach (var re in removed)
                paths.Remove(re);

            var resolver = new PathAssemblyResolver(paths);

            using (var metadaContext = new MetadataLoadContext(resolver))
            {
                var metadataModuleLoadContext = new MetadataTypeFindingContext(metadaContext);
                var readonlyAssembly = metadaContext.LoadFromAssemblyPath(assemblyPath);
                var typeFinder = new TypeFinder();

                foreach (var finderCriteria in _options.TypeFinderOptions.TypeFinderCriterias)
                {
                    var typesFound = typeFinder.Find(finderCriteria, readonlyAssembly, metadataModuleLoadContext);

                    if (typesFound?.Any() == true)
                        return true;
                }
            }
        }

        return false;
    }

    private void AddSharedFrameworkDlls(string hostApplicationPath, string runtimeDirectory, List<string> paths)
    {
        var defaultAssemblies = AssemblyLoadContext.Default.Assemblies.ToList();
        var defaultAssemblyDirectories = defaultAssemblies
            .Where(x => x.IsDynamic == false)
            .Where(x => string.IsNullOrWhiteSpace(x.Location) == false)
            .GroupBy(x => Path.GetDirectoryName(x.Location))
            .Select(x => x.Key)
            .ToList();

        foreach (var assemblyDirectory in defaultAssemblyDirectories)
        {
            if (string.Equals(assemblyDirectory.TrimEnd('\\').TrimEnd('/'), hostApplicationPath.TrimEnd('\\').TrimEnd('/')))
                continue;

            if (string.Equals(assemblyDirectory.TrimEnd('\\').TrimEnd('/'), runtimeDirectory.TrimEnd('\\').TrimEnd('/')))
                continue;

            if (_options.ModuleLoadContextOptions.AdditionalRuntimePaths == null)
                _options.ModuleLoadContextOptions.AdditionalRuntimePaths = new();

            if (_options.ModuleLoadContextOptions.AdditionalRuntimePaths.Contains(assemblyDirectory) == false)
                _options.ModuleLoadContextOptions.AdditionalRuntimePaths.Add(assemblyDirectory);

            var dlls = Directory.GetFiles(assemblyDirectory, "*.dll");

            paths.AddRange(dlls);
        }
    }
}
