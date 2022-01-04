namespace Funcky;

public static class MoneyMultiplicationExtension
{
    public static MoneyExpression Multiply(this MoneyExpression multiplicand, decimal multiplier)
        => new MoneyExpression.MoneyProduct(multiplicand, multiplier);
}
