namespace Funcky;

public static class MoneyDivisionExtension
{
    public static MoneyExpression Divide(this MoneyExpression dividend, decimal divisor)
        => new MoneyExpression.MoneyProduct(dividend, 1.0m / divisor);
}
