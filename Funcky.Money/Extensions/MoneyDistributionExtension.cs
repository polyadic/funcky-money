namespace Funcky;

public static class MoneyDistributionExtension
{
    extension(IMoneyExpression expression)
    {
        /// <summary>Distributes the money into <paramref name="numberOfParts"/> equal parts.</summary>
        public IEnumerable<IMoneyExpression> Distribute(int numberOfParts)
            => expression.Distribute([.. Enumerable.Repeat(1, numberOfParts)]);

        /// <summary>Distributes the money proportionally to the given <paramref name="factors"/>.</summary>
        public IEnumerable<IMoneyExpression> Distribute(IReadOnlyList<int> factors)
            => new MoneyDistribution(expression, factors, Option<decimal>.None);
    }
}
