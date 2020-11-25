using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Funcky.Extensions;

namespace Funcky
{
    internal class MoneyDistribution : IEnumerable<IMoneyExpression>
    {
        public MoneyDistribution(IMoneyExpression moneyExpression, IEnumerable<int> factors)
        {
            MoneyExpression = moneyExpression;
            Factors = factors.ToList();
        }

        public IMoneyExpression MoneyExpression { get; }

        public List<int> Factors { get; }

        public IEnumerator<IMoneyExpression> GetEnumerator()
            => Factors
                .WithIndex()
                .Select(f => (IMoneyExpression)new MoneyDistributionPart(this, f.Index))
                .GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator()
            => GetEnumerator();
    }
}
