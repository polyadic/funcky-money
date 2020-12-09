using System.Collections.Generic;

namespace Funcky
{
    internal class ToHumanReadableVisitor : IMoneyExpressionVisitor
    {
        private readonly Stack<string> _stack = new();

        public string Result
            => _stack.Peek();

        public void Visit(Money money)
        {
            _stack.Push(string.Format($"{{0:N{money.Currency.MinorUnitDigits}}}{{1}}", money.Amount, money.Currency.AlphabeticCurrencyCode));
        }

        public void Visit(MoneySum sum)
        {
            sum.Left.Accept(this);
            sum.Right.Accept(this);

            var right = _stack.Pop();
            var left = _stack.Pop();

            _stack.Push($"({left} + {right})");
        }

        public void Visit(MoneyProduct product)
        {
            product.Expression.Accept(this);
            var expression = _stack.Pop();

            _stack.Push($"({product.Factor} * {expression})");
        }

        public void Visit(MoneyDistributionPart part)
        {
            var distribution = part.Distribution;
            distribution.Expression.Accept(this);

            var total = _stack.Pop();
            _stack.Push($"{total}.Distribute({FormatFactors(distribution.Factors)})[{part.Index}]");
        }

        private string FormatFactors(IEnumerable<int> factors)
            => string.Join(", ", factors);
    }
}
