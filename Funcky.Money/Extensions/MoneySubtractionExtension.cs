namespace Funcky;

public static class MoneySubtractionExtension
{
    public static MoneyExpression Subtract(this MoneyExpression minuend, MoneyExpression subtrahend)
        => new MoneyExpression.MoneySum(minuend, new MoneyExpression.MoneyProduct(subtrahend, -1));
}
