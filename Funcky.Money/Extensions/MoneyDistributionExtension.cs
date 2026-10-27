namespace Funcky;

public static class MoneyDistributionExtension
{
    extension(IMoneyExpression expression)
    {
        /// <summary>Distributes the money into <paramref name="numberOfParts"/> equal parts.</summary>
        public IEnumerable<IMoneyExpression> Distribute(int numberOfParts)
            => new MoneyDistribution(expression, [.. Enumerable.Repeat(1, numberOfParts)], Option<decimal>.None);
    }
}
