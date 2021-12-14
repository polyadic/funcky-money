namespace Funcky;

public static class MoneySummationExtension
{
    public static IMoneyExpression Add(this IMoneyExpression augend, IMoneyExpression addend)
        => new MoneySum(augend, addend);
}
