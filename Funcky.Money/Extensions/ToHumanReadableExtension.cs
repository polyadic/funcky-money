namespace Funcky;

public static class ToHumanReadableExtension
{
    extension(IMoneyExpression expression)
    {
        /// <summary>Renders the expression tree as text, e.g. "((2 * 7.00CHF) + 0.50CHF)".</summary>
        public string ToHumanReadable()
            => expression.Accept(ToHumanReadableVisitor.Instance);
    }
}
