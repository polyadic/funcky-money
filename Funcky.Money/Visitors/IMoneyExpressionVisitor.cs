namespace Funcky;

public interface IMoneyExpressionVisitor<out TResult>
{
    TResult Visit(Money money);

    TResult Visit(MoneySum sum);

    TResult Visit(MoneyProduct product);
}
