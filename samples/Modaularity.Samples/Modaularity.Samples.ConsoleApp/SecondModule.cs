using Modaularity.Samples.Shared;

namespace Modaularity.Samples.ConsoleApp;

public class SecondModule : IModule
{
    public void Run()
    {
        Console.WriteLine("Second module");
    }
}
