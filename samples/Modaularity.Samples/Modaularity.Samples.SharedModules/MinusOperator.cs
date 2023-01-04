using Modaularity.Samples.Shared;

namespace Modaularity.Samples.SharedModules;

public class MinusOperator : IOperator
{
    public int Calculate(int x, int y)
    {
        return x - y;
    }
}
