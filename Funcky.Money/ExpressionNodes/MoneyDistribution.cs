using System.Collections;
using System.Numerics;
using Funcky.Extensions;
using Funcky.Monads;

namespace Funcky;

internal sealed class MoneyDistribution<TUnderlyingType> : IEnumerable<IMoneyExpression<TUnderlyingType>>
    where TUnderlyingType : IFloatingPoint<TUnderlyingType>
{
    public MoneyDistribution(IMoneyExpression<TUnderlyingType> moneyExpression, IEnumerable<int> factors, Option<TUnderlyingType> precision)
    {
        Expression = moneyExpression;
        Factors = factors.ToList();
        Precision = precision;

        if (Factors.None())
        {
            throw new ImpossibleDistributionException("we need at least one factor to distribute.");
        }
    }

    public IMoneyExpression<TUnderlyingType> Expression { get; }

    public List<int> Factors { get; }

    public Option<TUnderlyingType> Precision { get; }

    public IEnumerator<IMoneyExpression<TUnderlyingType>> GetEnumerator()
        => Factors
            .WithIndex()
            .Select(f => (IMoneyExpression<TUnderlyingType>)new MoneyDistributionPart<TUnderlyingType>(this, f.Index))
            .GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator()
        => GetEnumerator();
}
