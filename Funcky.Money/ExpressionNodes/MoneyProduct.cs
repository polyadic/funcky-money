namespace Funcky
{
    internal sealed record MoneyProduct : IMoneyExpression
    {
        public MoneyProduct(IMoneyExpression moneyExpression, decimal factor)
            => (Expression, Factor) = (moneyExpression, factor);

        public IMoneyExpression Expression { get; }

        public decimal Factor { get; }

        TState IMoneyExpression.Accept<TState>(IMoneyExpressionVisitor<TState> visitor)
            => visitor.Visit(this);
    }
}
