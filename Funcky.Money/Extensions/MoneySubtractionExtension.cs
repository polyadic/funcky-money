namespace Funcky;

public static class MoneySubtractionExtension
{
    extension(IMoneyExpression minuend)
    {
        public static IMoneyExpression operator -(IMoneyExpression left, IMoneyExpression right)
            => left.Subtract(right);

        public IMoneyExpression Subtract(IMoneyExpression subtrahend)
            => new MoneyDifference(minuend, subtrahend);
    }
}
