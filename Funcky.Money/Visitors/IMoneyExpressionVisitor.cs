namespace Funcky;

internal interface IMoneyExpressionVisitor<out TState>
    where TState : notnull
{
    TState Visit(Money money);

    TState Visit(MoneySum sum);

    TState Visit(MoneyProduct product);

    TState Visit(MoneyDistributionPart part);
}
