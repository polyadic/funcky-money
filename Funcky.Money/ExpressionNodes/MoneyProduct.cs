namespace Funcky
{
    internal sealed record MoneyProduct : IMoneyExpression
    {
        public MoneyProduct(IMoneyExpression moneyExpression, decimal factor)
        {
            Expression = moneyExpression;
            Factor = factor;
        }

        public IMoneyExpression Expression { get; }

        public decimal Factor { get; }

        void IMoneyExpression.Accept(IMoneyExpressionVisitor visitor)
            => visitor.Visit(this);
    }
}
