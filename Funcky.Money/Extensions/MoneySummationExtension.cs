namespace Funcky;

public static class MoneySummationExtension
{
    extension(IMoneyExpression augend)
    {
        public IMoneyExpression Add(IMoneyExpression addend)
            => new MoneySum(augend, addend);
    }
}
