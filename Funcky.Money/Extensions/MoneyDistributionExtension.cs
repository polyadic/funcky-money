using Funcky.Monads;

namespace Funcky;

public static class MoneyDistributionExtension
{
    public static IEnumerable<MoneyExpression> Distribute(this MoneyExpression moneyExpression, int numberOfSlices, Option<decimal> precision = default)
        => moneyExpression.Distribute(Enumerable.Repeat(element: 1, count: numberOfSlices), precision);

    public static IEnumerable<MoneyExpression> Distribute(this MoneyExpression moneyExpression, IEnumerable<int> factors, Option<decimal> precision = default)
        => new MoneyExpression.MoneyDistribution(moneyExpression, factors, precision);
}
