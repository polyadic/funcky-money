using System.Diagnostics;

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

        TState IMoneyExpression.Accept<TState>(IMoneyExpressionVisitor<TState> visitor, TState state)
            => visitor.Visit(this, state);
    }
}
