using System.Collections;

namespace Funcky;

/// <summary>
/// Distributes the evaluated <paramref name="Expression"/> into parts proportional to <paramref name="Factors"/>.
/// Enumerating a distribution yields one <see cref="MoneyDistributionPart"/> per factor.
/// </summary>
public sealed record MoneyDistribution(IMoneyExpression Expression, IReadOnlyList<int> Factors, Option<decimal> Precision) : IEnumerable<IMoneyExpression>
{
    public IEnumerator<IMoneyExpression> GetEnumerator()
        => Enumerable.Range(0, Factors.Count).Select(index => new MoneyDistributionPart(this, index)).GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator()
        => GetEnumerator();
}
