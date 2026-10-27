namespace Funcky;

public static class MoneyMultiplicationExtension
{
    extension(IMoneyExpression multiplicand)
    {
        public IMoneyExpression Multiply(decimal factor)
            => new MoneyProduct(multiplicand, factor);
    }
}
