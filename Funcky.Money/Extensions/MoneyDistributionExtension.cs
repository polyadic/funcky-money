namespace Funcky;

public static class MoneyDistributionExtension
{
    extension(IMoneyExpression expression)
    {
        /// <summary>Distributes the money into <paramref name="numberOfParts"/> equal parts.</summary>
        /// <param name="numberOfParts">The number of parts to distribute the money into.</param>
        /// <param name="precision">The unit in which the money is distributed; by default the smallest distribution unit of the context or the minor unit of the currency.</param>
        public IEnumerable<IMoneyExpression> Distribute(int numberOfParts, Option<decimal> precision = default)
            => expression.Distribute([.. Enumerable.Repeat(1, numberOfParts)], precision);

        /// <summary>Distributes the money proportionally to the given <paramref name="factors"/>.</summary>
        /// <param name="factors">The proportions of the parts, e.g. [1, 2] for a third and two thirds.</param>
        /// <param name="precision">The unit in which the money is distributed; by default the smallest distribution unit of the context or the minor unit of the currency.</param>
        public IEnumerable<IMoneyExpression> Distribute(IReadOnlyList<int> factors, Option<decimal> precision = default)
            => new MoneyDistribution(expression, factors, precision);
    }
}
