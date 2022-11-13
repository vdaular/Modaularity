using System.Reflection;

namespace Modaularity.Abstractions;

public class Module
{
    public string? Name { get; }
    public Version Version { get; }
    public Type? Type { get; }
    public Assembly? Assembly { get; }
    public IModuleCatalog Source { get; }
    public string? Description { get; }
    public string? ProductVersion { get; }
    public List<string>? Tags { get; }
    public string? Tag 
    { 
        get 
        { 
            return Tags?.FirstOrDefault();
        } 
    }

    public Module(Assembly assembly, Type type, string name, Version version, IModuleCatalog source, 
        string description = "", string productVersion = "", string tag = "", List<string>? tags = null)
    {
        Assembly= assembly;
        Type = type;
        Name = name;
        Version = version;
        Source = source;
        Description = description;
        ProductVersion = productVersion;
        Tags = tags;

        if (Tags == null)
            Tags = new();

        if (!string.IsNullOrWhiteSpace(tag))
            Tags.Add(tag);
    }

    public static implicit operator Type(Module module) => module.Type;

    public override string ToString() => $"{Name}: {Version}";
}