using Modaularity.Samples.Shared;

namespace Modaularity.Samples.SharedModules;

public class SecondSharedModule : IOutModule
{
    public string Get()
    {
        return "Second shared module";
    }
}
