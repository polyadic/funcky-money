namespace Funcky;

public static class MoneySummationExtension
{
    extension(IMoneyExpression augend)
    {
        public static IMoneyExpression operator +(IMoneyExpression left, IMoneyExpression right)
            => left.Add(right);

        public IMoneyExpression Add(IMoneyExpression addend)
            => new MoneySum(augend, addend);
    }
}
