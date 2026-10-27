using FsCheck;
using FsCheck.Fluent;
using FsCheck.Xunit;
using Xunit;

namespace Funcky.Test;

[Properties(Arbitrary = [typeof(MoneyArbitraries)])]
public sealed class DivisionImplementationTest
{
    [Fact]
    public void ReferenceTestWhichFailsWithInversionInsteadOfDivide()
    {
        var reference = Money.CHF(76);
        var factor = 0.6666m;

        var result = reference
            .Multiply(factor)
            .Divide(factor)
            .Evaluate();

        Assert.Equal(reference, result);
    }

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
