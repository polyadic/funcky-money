namespace Funcky;

public static class MoneyDivisionExtension
{
    public static IMoneyExpression Divide(this IMoneyExpression dividend, decimal divisor)
        => new MoneyProduct(dividend, 1.0m / divisor);

    public static decimal Divide(this IMoneyExpression dividend, IMoneyExpression divisor)
    {
        return 0m;
    }
}
