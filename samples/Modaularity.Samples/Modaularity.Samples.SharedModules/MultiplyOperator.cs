using Modaularity.Samples.Shared;
using System.ComponentModel;

namespace Modaularity.Samples.SharedModules;

public class MultiplyOperator : IOperator
{
    [DisplayName("The multiplier module")]
    public int Calculate(int x, int y)
    {
        return x * y;
    }
}
