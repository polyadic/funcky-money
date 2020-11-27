using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Funcky.Extensions;

namespace Funcky
{
    internal class MoneyDistribution : IEnumerable<IMoneyExpression>, IMoneyExpression
    {
        public MoneyDistribution(IMoneyExpression moneyExpression, IEnumerable<int> factors)
        {
            Expression = moneyExpression;
            Factors = factors.ToList();
        }

        public IMoneyExpression Expression { get; }

        public List<int> Factors { get; }

        public IEnumerator<IMoneyExpression> GetEnumerator()
            => Factors
                .WithIndex()
                .Select(f => (IMoneyExpression)new MoneyDistributionPart(this, f.Index))
                .GetEnumerator();

        void IMoneyExpression.Accept(IMoneyExpressionVisitor visitor)
            => visitor.Visit(this);

        IEnumerator IEnumerable.GetEnumerator()
            => GetEnumerator();
    }
}
