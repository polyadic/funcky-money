namespace Funcky
{
    public static class ToHumanReadableExtension
    {
        public static string ToHumanReadable(this IMoneyExpression moneyExpression)
        {
            var visitor = new ToHumanReadableVisitor();

            moneyExpression.Accept(visitor);

            return visitor.Result;
        }
    }
}
