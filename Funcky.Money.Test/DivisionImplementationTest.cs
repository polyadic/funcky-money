using FsCheck;
using FsCheck.Xunit;

namespace Funcky.Test;

public sealed class DivisionImplementationTest
{
    public DivisionImplementationTest()
        => Arb.Register<MoneyArbitraries>();

    [Property]
    public Property MultiplyingAndDividingByTheSameFactorRoundTripsToAnEqualMoney(Money reference, decimal factor)
    {
        if (factor == 0)
        {
            return true.ToProperty();
        }

        var result = reference
            .Multiply(factor)
            .Divide(factor)
            .Evaluate();

        return (reference == result).ToProperty();
    }
}
