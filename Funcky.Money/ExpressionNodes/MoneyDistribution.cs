using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Funcky.Extensions;
using Funcky.Monads;

namespace Funcky
{
    internal sealed class MoneyDistribution : IEnumerable<IMoneyExpression>
    {
        public MoneyDistribution(IMoneyExpression moneyExpression, IEnumerable<int> factors, Option<decimal> precision)
        {
            Expression = moneyExpression;
            Factors = factors.ToList();
            Precision = precision;

            if (Factors.None())
            {
                throw new ImpossibleDistributionException("we need at least one factor to distribute.");
            }
        }

        public IMoneyExpression Expression { get; }

        public List<int> Factors { get; }

        public Option<decimal> Precision { get; }

        public IEnumerator<IMoneyExpression> GetEnumerator()
            => Factors
                .WithIndex()
                .Select(f => (IMoneyExpression)new MoneyDistributionPart(this, f.Index))
                .GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator()
            => GetEnumerator();
    }
}
