namespace Funcky;

public static class ToHumanReadableExtension
{
    public static string ToHumanReadable(this IMoneyExpression moneyExpression)
        => moneyExpression.Accept(ToHumanReadableVisitor.Instance);
}
