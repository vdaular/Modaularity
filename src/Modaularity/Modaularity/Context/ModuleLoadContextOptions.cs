using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using System.Reflection;

namespace Modaularity.Context;

public class ModuleLoadContextOptions
{
    public UseHostApplicationAssembliesEnum UseHostApplicationAssemblies { get; set; } = Defaults.UseHostApplicationAssemblies;
    public List<AssemblyName> HostApplicationAssemblies { get; set; } = Defaults.HostApplicationAssemblies;
    public Func<ILogger<ModuleAssemblyLoadContext>> LoggerFactory { get; set; } = Defaults.LoggerFactory;
    public List<string> AdditionalRuntimePaths { get; set; } = Defaults.AdditionalRuntimePaths;
    public List<RuntimeAssemblyHint> RuntimeAssemblyHints { get; set; } = Defaults.RuntimeAssemblyHints;

    public static class Defaults
    {
        public static UseHostApplicationAssembliesEnum UseHostApplicationAssemblies { get; set; } = UseHostApplicationAssembliesEnum.Always;
        public static List<AssemblyName> HostApplicationAssemblies { get; set; } = new();
        public static Func<ILogger<ModuleAssemblyLoadContext>> LoggerFactory { get; set; } = () => NullLogger<ModuleAssemblyLoadContext>.Instance;
        public static List<string> AdditionalRuntimePaths { get; set; } = new();
        public static List<RuntimeAssemblyHint> RuntimeAssemblyHints { get; set; } = new();
    }
}