using Modaularity.Abstractions;
using Modaularity.Context;
using Modaularity.TypeFinding;

namespace Modaularity.Catalogs.Types;

public class TypeModuleCatalog : IModuleCatalog
{   
    private readonly Type? _moduleType;
    private readonly TypeModuleCatalogOptions? _options;
    private Module? _module;

    public TypeModuleCatalogOptions Options
    {
        get { return _options; }
    }

    public TypeModuleCatalog(Type? moduleType, Action<ModuleNameOptions>? configure, ModuleNameOptions? nameOptions, TypeModuleCatalogOptions? options)
    {
        if (moduleType == null) 
            throw new ArgumentNullException(nameof(moduleType));

        _moduleType = moduleType;
        _options = options ?? new TypeModuleCatalogOptions();

        if (!_options.TypeFinderCriterias.Any())
            _options.TypeFinderCriterias.Add(string.Empty, new TypeFinderCriteria() { Query = (context, type) => true });

        if (_options.TypeFindingContext == null)
            _options.TypeFindingContext = new ModuleAssemblyLoadContext(moduleType.Assembly);

        if (_options.TypeFinderOptions == null)
            _options.TypeFinderOptions = new();

        if (_options.TypeFinderOptions.TypeFinderCriterias?.Any() != true)
        {
            _options.TypeFinderOptions.TypeFinderCriterias = new();

            if (_options.TypeFinderCriterias?.Any() == true)
            {
                foreach (var typeFinderCriteria in _options.TypeFinderCriterias)
                {
                    var typeFinder = typeFinderCriteria.Value;
                    typeFinder.Tags = new() { typeFinderCriteria.Key };
                    _options.TypeFinderOptions.TypeFinderCriterias.Add(typeFinder);
                }
            }
        }

        if (configure == null && nameOptions == null)
            return;

        var naming = nameOptions ?? new ModuleNameOptions();
        configure?.Invoke(naming);

        _options.ModuleNameOptions = naming;
    }

    public TypeModuleCatalog(Type? moduleType, TypeModuleCatalogOptions? options) : this(moduleType, null, null, options) { }

    public TypeModuleCatalog(Type? moduleType, Action<ModuleNameOptions>? configure) : this(moduleType, configure, null, null) { }

    public TypeModuleCatalog(Type? moduleType, ModuleNameOptions? nameOptions) : this(moduleType, null, nameOptions, null) { }

    public TypeModuleCatalog(Type? moduleType) : this(moduleType, null, null, null) { }

    public bool IsInitialized { get; private set; }

    public Module Get(string? name, Version version)
    {
        if (!string.Equals(name, _module.Name, StringComparison.InvariantCultureIgnoreCase) ||
            version != _module.Version)
            return null;

        return _module;
    }

    public List<Module> GetModules() => new List<Module>() { _module };

    public Task Initialize()
    {
        var namingOptions = _options.ModuleNameOptions;
        var version = namingOptions.ModuleVersionGenerator(namingOptions, _moduleType);
        var moduleName = namingOptions.ModuleNameGenerator(namingOptions, _moduleType);
        var description = namingOptions.ModuleDescriptionGenerator(namingOptions, _moduleType);
        var productVersion = namingOptions.ModuleProductVersionGenerator(namingOptions, _moduleType);

        var tags = new List<string>();
        var finder = new TypeFinder();

        foreach (var typeFinderCriteria in _options.TypeFinderOptions.TypeFinderCriterias)
        {
            var isMatch = finder.IsMatch(typeFinderCriteria, _moduleType, _options.TypeFindingContext);

            if (isMatch)
                if (typeFinderCriteria.Tags?.Any() == true)
                    tags.AddRange(typeFinderCriteria.Tags);
        }

        _module = new Module(_moduleType.Assembly, _moduleType, moduleName, version, this, description, productVersion, tags: tags);

        IsInitialized = true;

        return Task.CompletedTask;
    }
}
