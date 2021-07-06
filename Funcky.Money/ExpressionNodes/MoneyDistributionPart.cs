namespace Funcky
{
    internal sealed class MoneyDistributionPart : IMoneyExpression
    {
        public MoneyDistributionPart(MoneyDistribution distribution, int index)
            => (Distribution, Index) = (distribution, index);

        public MoneyDistribution Distribution { get; }

        public int Index { get; }

        TState IMoneyExpression.Accept<TState>(IMoneyExpressionVisitor<TState> visitor)
            => visitor.Visit(this);
    }
}
