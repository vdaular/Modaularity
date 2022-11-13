using DotNetTypeGenerator;
using DotNetTypeGenerator.Delegates;
using Modaularity.Abstractions;
using Modaularity.Catalogs.Types;
using Modaularity.TypeFinding;

namespace Modaularity.Catalogs.Delegates;

public class DelegateModuleCatalog : IModuleCatalog
{
    private TypeModuleCatalog? _catalog;
    private readonly MulticastDelegate _multicastDelegate;
    private readonly DelegateModuleCatalogOptions? _options;

    public DelegateModuleCatalog(MulticastDelegate multicastDelegate, List<DelegateConversionRule>? conversionRules = null, 
        ModuleNameOptions? nameOptions = null, DelegateModuleCatalogOptions? options = null, string? moduleName = null)
    {
        if (multicastDelegate == null) 
            throw new ArgumentNullException(nameof(multicastDelegate));

        _multicastDelegate = multicastDelegate;

        if (conversionRules == null)
            conversionRules = new();

        if (options != null)
            _options = options;
        else
            _options = new();

        _options.ConversionRules = conversionRules;

        if (nameOptions == null)
            nameOptions = new();

        _options.NameOptions = nameOptions;

        if (!string.IsNullOrWhiteSpace(moduleName))
            _options.NameOptions.ModuleNameGenerator = (moduleNameOptiones, type) => moduleName;
    }

    public DelegateModuleCatalog(MulticastDelegate multicastDelegate, string moduleName = "")
        : this(multicastDelegate, null, null, null, moduleName) { }

    public DelegateModuleCatalog(TypeModuleCatalog catalog, MulticastDelegate multicastDelegate, DelegateModuleCatalogOptions options) 
        : this(multicastDelegate, options?.ConversionRules, options?.NameOptions, options) { }

    public bool IsInitialized { get; set; }

    public Module Get(string name, Version version) => _catalog.Get(name, version);

    public List<Module> GetModules() => _catalog.GetModules();

    public async Task Initialize()
    {
        var converter = new DelegateToTypeWrapper();
        var delegateToTypeWrapperOptions = ConvertOptions();
        var assembly = converter.CreateType(_multicastDelegate, delegateToTypeWrapperOptions);
        var options = new TypeModuleCatalogOptions() { ModuleNameOptions = _options.NameOptions };

        if (_options.Tags?.Any() == true)
            options.TypeFinderOptions = new TypeFinderOptions 
            { 
                TypeFinderCriterias = new() { TypeFinderCriteriaBuilder.Create().Tag(_options.Tags.ToArray()) }
            };

        _catalog = new TypeModuleCatalog(assembly, options);
        await _catalog.Initialize();

        IsInitialized = true;
    }

    private DelegateToTypeWrapperOptions ConvertOptions()
    {
        var conversionRules = GetConversionRules();

        return new DelegateToTypeWrapperOptions()
        {
            ConversionRules = conversionRules,
            MethodName = _options.MethodName,
            NamespaceName = _options.NamespaceName,
            TypeName = _options.TypeName,
            MethodNameGenerator = wrapperOptions => _options.MethodNameGenerator(_options),
            NamespaceNameGenerator = wrapperOptions => _options.NamespaceNameGenerator(_options),
            TypeNameGenerator = wrapperOptiones => _options.TypeNameGenerator(_options)
        };
    }

    private List<ParameterConversionRule> GetConversionRules()
    {
        var conversionRules = new List<ParameterConversionRule>();

        foreach (var conversionRule in _options.ConversionRules)
        {
            var paramConversion = new ParameterConversionRule(conversionRule.CanHandle, info => 
            { 
                var handleResult = conversionRule.Handle(info);

                return new DotNetTypeGenerator.ParameterConversion()
                {
                    Name = handleResult.Name,
                    ToConstructor = handleResult.ToConstructor,
                    ToPublicProperty = handleResult.ToPublicProperty
                };
            });

            conversionRules.Add(paramConversion);
        }

        return conversionRules;
    }
}
