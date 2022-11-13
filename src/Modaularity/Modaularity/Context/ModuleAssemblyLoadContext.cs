using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Modaularity.TypeFinding;
using System.Reflection;
using System.Runtime.Loader;

namespace Modaularity.Context;

public class ModuleAssemblyLoadContext : AssemblyLoadContext, ITypeFindingContext
{
    private readonly string? _pluginPath;
    private readonly AssemblyDependencyResolver? _resolver;
    private readonly ModuleLoadContextOptions? _options;
    private readonly List<RuntimeAssemblyHint> _runtimeAssemblyHints;

    public ModuleAssemblyLoadContext(string? pluginPath, ModuleLoadContextOptions? options = null) : base(true)
    {
        _pluginPath = pluginPath;
        _resolver = new(pluginPath);
        _options = options ?? new ModuleLoadContextOptions();
        _runtimeAssemblyHints = _options.RuntimeAssemblyHints;

        if (_runtimeAssemblyHints == null)
            _runtimeAssemblyHints = new();
    }

    public ModuleAssemblyLoadContext(Assembly assembly, ModuleLoadContextOptions? options = null) : this(assembly.Location, options) { }

    public Assembly Load()
    {
        var assemblyName = new AssemblyName(Path.GetFileNameWithoutExtension(_pluginPath));
        var result = LoadFromAssemblyName(assemblyName);

        return result;
    }

    protected override Assembly? Load(AssemblyName assemblyName)
    {
        Log(LogLevel.Debug, $"Cargando {assemblyName}");

        if (TryUseHostApplicationAssembly(assemblyName))
        {
            var foundFromHostApplication = LoadHostApplicationAssembly(assemblyName);

            if (foundFromHostApplication)
            {
                Log(LogLevel.Debug, $"Ensamblado {assemblyName} está disponible a través del AssemblyLoadContext de la aplicación host. Se usa.");

                return null;
            }

            Log(LogLevel.Debug, $"AssemblyLoadContext de la aplicación host no contiene {assemblyName}. Se intenta resolver a través de las referencias del módulo.");
        }

        string? assemblyPath;
        var assemblyFileName = $"{assemblyName.Name}.dll";

        if (_runtimeAssemblyHints.Any(x => string.Equals(assemblyName, x.FileName)))
        {
            Log(LogLevel.Debug, $"Se encontró rastro de ensamblado para {assemblyName}");
            assemblyPath = _runtimeAssemblyHints.First(x => string.Equals(assemblyFileName, x.FileName)).Path;
        }
        else
        {
            Log(LogLevel.Debug, $"No se encontró rastro de ensamblado para {assemblyName}. Se utiliza el resolutor por defecto para localizar el archivo");
            assemblyPath = _resolver.ResolveAssemblyToPath(assemblyName);
        }

        if (assemblyPath != null)
        {
            Log(LogLevel.Debug, $"Cargando {assemblyName} en AssemblyLoadContext desde {assemblyPath}");
            var result = LoadFromAssemblyPath(assemblyPath);

            return result;
        }

        if (_options.UseHostApplicationAssemblies == UseHostApplicationAssembliesEnum.PreferModule)
        {
            var foundFromHostApplication = LoadHostApplicationAssembly(assemblyName);

            if (foundFromHostApplication)
            {
                Log(LogLevel.Debug, $"Ensamblado {assemblyName} no disponible desde las referencias del módulo pero está disponible a través del AssemblyLoadContext de la aplicación host. Se usa.");

                return null;
            }
        }

        if (_options.AdditionalRuntimePaths?.Any() == true)
        {
            Log(LogLevel.Warning, $"No fue posible localizar el ensamblado utilizando {assemblyName}. Se agregarán rutas adicionales usando {nameof(ModuleLoadContextOptions.Defaults.AdditionalRuntimePaths)}");

            return null;
        }

        foreach (var runtimePath in _options.AdditionalRuntimePaths)
        {
            var fileName = assemblyFileName;
            var filePath = Directory.GetFiles(runtimePath, fileName, SearchOption.AllDirectories).FirstOrDefault();

            if (filePath != null)
            {
                Log(LogLevel.Debug, $"Se encontró {assemblyName} en {filePath} utilizando {runtimePath}");

                return LoadFromAssemblyPath(filePath);
            }
        }

        Log(LogLevel.Warning, $"No fue posible localizar el ensamblado usando {assemblyName}. No fue posible hallar el ensamblado desde AdditionalRuntimePaths. Por favor, intente agregar rutas usando {nameof(ModuleLoadContextOptions.Defaults.AdditionalRuntimePaths)}");

        return null;
    }

    private bool LoadHostApplicationAssembly(AssemblyName assemblyName)
    {
        try
        {
            Default.LoadFromAssemblyName(assemblyName);

            return true;
        }
        catch
        {
            return false;
        }
    }

    private void Log(LogLevel logLevel, string message, Exception? ex = null)
    {
        var logger = GetLogger();

        logger.Log(logLevel, ex, message);
    }

    private static string loggerLock = "lock";
    private ILogger<ModuleAssemblyLoadContext>? _logger;

    private ILogger<ModuleAssemblyLoadContext> GetLogger()
    {
        if (_logger == null)
        {
            lock (loggerLock)
            {
                if (_logger == null)
                {
                    if (_options?.LoggerFactory == null)
                        _logger = NullLogger<ModuleAssemblyLoadContext>.Instance;
                    else
                        _logger = _options.LoggerFactory();
                }
            }
        }

        return _logger;
    }

    private bool TryUseHostApplicationAssembly(AssemblyName assemblyName)
    {
        Log(LogLevel.Debug, $"Determinando si {assemblyName} debería ser cargado desde el AssemblyLoadContext de la aplicación host o desde el módulo");

        if (_options.UseHostApplicationAssemblies == UseHostApplicationAssembliesEnum.Never)
        {
            Log(LogLevel.Debug, $"UseHostApplicationAssemblies está seteado en Nunca. Se intenta cargar el ensamblado desde el AssemblyLoadContext del módulo.");

            return false;
        }

        if (_options.UseHostApplicationAssemblies == UseHostApplicationAssembliesEnum.Always)
        {
            Log(LogLevel.Debug, $"UseHostApplicationAssemblies está seteado en Siempre. Se intenta cargar el ensamblado desde el AssemblyLoadContext de la aplicación host.");

            return true;
        }

        if (_options.UseHostApplicationAssemblies == UseHostApplicationAssembliesEnum.Selected)
        {
            var name = assemblyName.Name;
            var result = _options.HostApplicationAssemblies?.Any(x => string.Equals(x.Name, name, StringComparison.InvariantCultureIgnoreCase)) == true;

            Log(LogLevel.Debug, $"UseHostApplicationAssemblies está seteado en Seleccionado. {assemblyName} listado en el HostApplicationAssemblies: {result}");

            return result;
        }

        if (_options.UseHostApplicationAssemblies == UseHostApplicationAssembliesEnum.PreferModule)
        {
            Log(LogLevel.Debug, $"UseHostApplicationAssemblies está seteado en Preferir Módulo. Se intenta cargar ensamblado desde el AssemblyLoadContext del módulo");

            return false;
        }

        return false;
    }

    protected override nint LoadUnmanagedDll(string? unmanagedDllName)
    {
        var nativeHint = _runtimeAssemblyHints.FirstOrDefault(x => x.IsNative && string.Equals(x.FileName, unmanagedDllName));

        if (nativeHint != null)
            return LoadUnmanagedDllFromPath(nativeHint.Path);

        var libraryPath = _resolver.ResolveUnmanagedDllToPath(unmanagedDllName);

        if (libraryPath != null)
            return LoadUnmanagedDllFromPath(libraryPath);

        return IntPtr.Zero;
    }

    public Assembly FindAssembly(string assemblyName)
    {
        return Load(new AssemblyName(assemblyName));
    }

    public Type FindType(Type type)
    {
        var assemblyName = type.Assembly.GetName();
        var assembly = Load(assemblyName);

        if (assembly == null)
            assembly = Assembly.Load(assemblyName);

        var result = assembly.GetType(type.FullName);

        return result;
    }
}
