using Modaularity.Samples.Shared;

namespace Modaularity.Samples.ConsoleApp;

public class MyModule : IMyModule
{
    public void Run()
    {
        Console.WriteLine("My module implementing IMyModule");
    }
}
