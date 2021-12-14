namespace Funcky;

public static class MoneySubtractionExtension
{
    public static IMoneyExpression Subtract(this IMoneyExpression minuend, IMoneyExpression subtrahend)
        => new MoneySum(minuend, new MoneyProduct(subtrahend, -1));
}
