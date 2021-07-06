namespace Funcky
{
    internal sealed record MoneySum : IMoneyExpression
    {
        public MoneySum(IMoneyExpression leftMoneyExpression, IMoneyExpression rightMoneyExpression)
            => (Left, Right) = (leftMoneyExpression, rightMoneyExpression);

        public IMoneyExpression Left { get; }

        public IMoneyExpression Right { get; }

        TState IMoneyExpression.Accept<TState>(IMoneyExpressionVisitor<TState> visitor)
            => visitor.Visit(this);
    }
}
