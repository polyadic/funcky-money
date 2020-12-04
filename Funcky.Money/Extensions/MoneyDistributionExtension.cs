using System.Collections.Generic;
using System.Linq;
using Funcky.Monads;

namespace Funcky
{
    public static class MoneyDistributionExtension
    {
        public static IEnumerable<IMoneyExpression> Distribute(this IMoneyExpression moneyExpression, int numberOfSlices, Option<decimal> precision = default)
            => moneyExpression.Distribute(Enumerable.Repeat(1, numberOfSlices));

        public static IEnumerable<IMoneyExpression> Distribute(this IMoneyExpression moneyExpression, IEnumerable<int> factors, Option<decimal> precision = default)
            => new MoneyDistribution(moneyExpression, factors, precision);
    }
}
