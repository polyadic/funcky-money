namespace Funcky;

public interface IMoneyExpressionVisitor<out TResult>
{
    TResult Visit(Money money);

    TResult Visit(MoneySum sum);

    TResult Visit(MoneyDifference difference);

    TResult Visit(MoneyProduct product);

    TResult Visit(MoneyQuotient quotient);

    TResult Visit(MoneyDistributionPart part);
}
