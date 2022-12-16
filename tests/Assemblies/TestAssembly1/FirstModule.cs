using TestInterfaces;

namespace TestAssembly1;

public class FirstModule : ICommand
{
    public string RunMe() => "Hola desde RunMe";
}
