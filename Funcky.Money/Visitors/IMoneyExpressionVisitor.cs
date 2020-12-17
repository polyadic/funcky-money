namespace Funcky
{
    internal interface IMoneyExpressionVisitor<TState>
    {
        TState Visit(Money money, TState state);

        TState Visit(MoneySum sum, TState state);

        TState Visit(MoneyProduct product, TState state);

        TState Visit(MoneyDistributionPart part, TState state);
    }
}
