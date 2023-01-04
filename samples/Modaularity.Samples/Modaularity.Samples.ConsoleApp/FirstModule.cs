using Modaularity.Samples.Shared;

namespace Modaularity.Samples.ConsoleApp;

public class FirstModule : IModule
{
    public void Run()
    {
        Console.WriteLine("First module");
    }
}
