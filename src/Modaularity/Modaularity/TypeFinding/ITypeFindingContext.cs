using System.Reflection;

namespace Modaularity.TypeFinding;

public interface ITypeFindingContext
{
    Assembly FindAssembly(string assemblyName);
    Type FindType(Type type);
}
