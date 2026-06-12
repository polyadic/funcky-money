namespace Funcky;

public static class MoneySubtractionExtension
{
    public static IMoneyExpression Subtract(this IMoneyExpression minuend, IMoneyExpression subtrahend)
        => new MoneyDifference(minuend, subtrahend);
}
