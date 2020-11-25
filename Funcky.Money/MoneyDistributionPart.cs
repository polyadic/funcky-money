namespace Funcky
{
    internal class MoneyDistributionPart : IMoneyExpression
    {
        public MoneyDistributionPart(MoneyDistribution distribution, int index)
        {
            Distribution = distribution;
            Index = index;
        }

        public MoneyDistribution Distribution { get; }

        public int Index { get; }

        void IMoneyExpression.Accept(IMoneyExpressionVisitor visitor)
            => visitor.Visit(this);
    }
}
