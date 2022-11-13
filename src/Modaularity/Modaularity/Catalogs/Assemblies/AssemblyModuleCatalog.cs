using Modaularity.Abstractions;
using Modaularity.Catalogs.Types;
using Modaularity.Context;
using Modaularity.TypeFinding;
using System.Reflection;
using Module = Modaularity.Abstractions.Module;

namespace Modaularity.Catalogs.Assemblies;

public class AssemblyModuleCatalog : IModuleCatalog
{
    private readonly string? _assemblyPath;
    private Assembly? _assembly;
    private readonly AssemblyModuleCatalogOptions? _options;
    private ModuleAssemblyLoadContext? _moduleAssemblyLoadContext;
    private List<TypeModuleCatalog>? _modules = null;

    public AssemblyModuleCatalog(string? assemblyPath = null, Assembly? assembly = null, Predicate<Type>? filter = null,
        Dictionary<string, Predicate<Type>>? taggedFilters = null, Action<TypeFinderCriteriaBuilder>? configureFinder = null,
        TypeFinderCriteria? criteria = null, AssemblyModuleCatalogOptions? options = null)
    {
        if (assembly == null)
        {
            _assembly = assembly;
            _assemblyPath = _assembly.Location;
        }
        else if (!string.IsNullOrWhiteSpace(assemblyPath))
            _assemblyPath = assemblyPath;
        else
            throw new ArgumentNullException($"Se debe setear {nameof(assembly)} o {nameof(assemblyPath)}.");

        _options = options ?? new();

        SetFilter(filter, taggedFilters, criteria, configureFinder);
    }

    public AssemblyModuleCatalog(Assembly? assembly, Dictionary<string, Predicate<Type>> taggedFilters,
        AssemblyModuleCatalogOptions? options = null) : this(null, assembly, null, taggedFilters, null, null, options) { }

    public AssemblyModuleCatalog(string assemblyPath, Dictionary<string, Predicate<Type>> taggedFilters,
        AssemblyModuleCatalogOptions? options = null) : this(assemblyPath, null, null, taggedFilters, null, null, options) { }

    public AssemblyModuleCatalog(Assembly assembly, Predicate<Type>? filter = null, AssemblyModuleCatalogOptions? options = null)
        : this(null, assembly, filter, null, null, null, options){ }

    public AssemblyModuleCatalog(string assemblyPath, Predicate<Type>? filter = null, 
        AssemblyModuleCatalogOptions? options = null) : this(assemblyPath, null, filter, null, null, null, options) { }

    public AssemblyModuleCatalog(Assembly assembly, Action<TypeFinderCriteriaBuilder>? configureFinder = null,
        AssemblyModuleCatalogOptions? options = null) : this(null, assembly, null, null, configureFinder, null, options) { }

    public AssemblyModuleCatalog(string assemblyPath, Action<TypeFinderCriteriaBuilder>? configureFinder = null,
        AssemblyModuleCatalogOptions? options = null) : this(assemblyPath, null, null, null, configureFinder, null, options) { }

    public AssemblyModuleCatalog(string assemblyPath, TypeFinderCriteria? criteria = null, AssemblyModuleCatalogOptions? options = null) 
        : this(assemblyPath, null, null, null, null, criteria, options) { }

    public AssemblyModuleCatalog(Assembly assembly, AssemblyModuleCatalogOptions? options = null) 
        : this(null, assembly, null, null, null, null, options) { }

    public AssemblyModuleCatalog(string assemblyPath, AssemblyModuleCatalogOptions? options = null) 
        : this(assemblyPath, null, null, null, null, null, options) { }

    public AssemblyModuleCatalog(Assembly assembly) : this(null, assembly) { }

    public AssemblyModuleCatalog(string assemblyPath) : this(assemblyPath, null, null, null) { }

    private void SetFilter(Predicate<Type>? filter, Dictionary<string, Predicate<Type>>? taggedFilters, TypeFinderCriteria? criteria, Action<TypeFinderCriteriaBuilder>? configureFinder)
    {
        if (_options.TypeFinderOptions == null)
            _options.TypeFinderOptions = new();

        if (_options.TypeFinderOptions.TypeFinderCriterias == null)
            _options.TypeFinderOptions.TypeFinderCriterias = new();

        if (filter != null)
        {
            var filterCriteria = new TypeFinderCriteria { Query = (context, type) => filter(type) };

            filterCriteria.Tags.Add(string.Empty);

            _options.TypeFinderOptions.TypeFinderCriterias.Add(filterCriteria);
        }

        if (taggedFilters?.Any() == true)
        {
            foreach (var taggedFilter in taggedFilters)
            {
                var taggedCriteria = new TypeFinderCriteria { Query = (context, type) => taggedFilter.Value(type) };
                
                taggedCriteria.Tags.Add(taggedFilter.Key);

                _options.TypeFinderOptions.TypeFinderCriterias.Add(taggedCriteria);
            }
        }

        if (configureFinder != null)
        {
            var builder = new TypeFinderCriteriaBuilder();
            configureFinder(builder);

            var configuredCriteria = builder.Build();

            _options.TypeFinderOptions.TypeFinderCriterias.Add(configuredCriteria);
        }

        if (criteria != null)
            _options.TypeFinderOptions.TypeFinderCriterias.Add(criteria);

        if (_options.TypeFinderCriterias?.Any() == true)
        {
            foreach (var typeFinderCriteria in _options.TypeFinderCriterias)
            {
                var crit = typeFinderCriteria.Value;
                crit.Tags = new() { typeFinderCriteria.Key };

                _options.TypeFinderOptions.TypeFinderCriterias.Add(crit);
            }
        }

        if (_options.TypeFinderOptions.TypeFinderCriterias.Any() != true)
        {
            var findAll = TypeFinderCriteriaBuilder
                .Create()
                .Tag(string.Empty)
                .Build();

            _options.TypeFinderOptions.TypeFinderCriterias.Add(findAll);
        }
    }

    public bool IsInitialized { get; private set; }

    public Module Get(string name, Version version)
    {
        foreach (var moduleCatalog in _modules)
        {
            var foundModule = moduleCatalog.Get(name, version);

            if (foundModule == null)
                continue;

            return foundModule;
        }

        return null;
    }

    public List<Module> GetModules() => _modules.SelectMany(x => x.GetModules()).ToList();

    public async Task Initialize()
    {
        if (!string.IsNullOrWhiteSpace(_assemblyPath) && _assembly == null)
            if (!File.Exists(_assemblyPath))
                throw new ArgumentException($"No existe el ensamblado en la ruta {_assemblyPath}");

        if (_assembly == null && File.Exists(_assemblyPath) || File.Exists(_assemblyPath) && _moduleAssemblyLoadContext == null)
        {
            _moduleAssemblyLoadContext = new(_assemblyPath, _options.ModuleLoadContextOptions);
            _assembly = _moduleAssemblyLoadContext.Load();
        }

        _modules = new();

        var finder = new TypeFinder();
        var handledModuleTypes = new List<Type>();

        foreach (var typeFinderCriteria in _options.TypeFinderOptions.TypeFinderCriterias)
        {
            var moduleTypes = finder.Find(typeFinderCriteria, _assembly, _moduleAssemblyLoadContext);

            foreach (var type in moduleTypes)
            {
                if (handledModuleTypes.Contains(type))
                    continue;

                var typeModuleCatalog = new TypeModuleCatalog(type, new TypeModuleCatalogOptions()
                {
                    ModuleNameOptions = _options.ModuleNameOptions,
                    TypeFindingContext = _moduleAssemblyLoadContext,
                    TypeFinderOptions = _options.TypeFinderOptions
                });

                await typeModuleCatalog.Initialize();

                _modules.Add(typeModuleCatalog);

                handledModuleTypes.Add(type);
            }
        }

        IsInitialized = true;
    }
}
