namespace Funcky;

public static class MoneySummationExtension
{
    public static MoneyExpression Add(this MoneyExpression augend, MoneyExpression addend)
        => new MoneyExpression.MoneySum(augend, addend);
}
