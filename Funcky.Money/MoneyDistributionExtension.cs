using System.Collections.Generic;
using System.Linq;

namespace Funcky
{
    public static class MoneyDistributionExtension
    {
        public static IEnumerable<IMoneyExpression> Distribute(this IMoneyExpression moneyExpression, int numberOfSlices)
            => moneyExpression.Distribute(Enumerable.Repeat(1, numberOfSlices));

        public static IEnumerable<IMoneyExpression> Distribute(this IMoneyExpression moneyExpression, IEnumerable<int> factors)
            => new MoneyDistribution(moneyExpression, factors);
    }
}
