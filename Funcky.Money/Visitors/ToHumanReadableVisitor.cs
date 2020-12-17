using System;
using System.Collections.Generic;
using System.Collections.Immutable;

namespace Funcky
{
    internal class ToHumanReadableVisitor : IMoneyExpressionVisitor<ImmutableStack<string>>
    {
        private static readonly Lazy<ToHumanReadableVisitor> LazyInstance = new(() => new());

        public static ToHumanReadableVisitor Instance
            => LazyInstance.Value;

        public ImmutableStack<string> Visit(Money money, ImmutableStack<string> stack)
            => stack
                .Push(string.Format($"{{0:N{money.Currency.MinorUnitDigits}}}{{1}}", money.Amount, money.Currency.AlphabeticCurrencyCode));

        public ImmutableStack<string> Visit(MoneySum sum, ImmutableStack<string> stack)
            => sum
                .Right
                .Accept(this, sum.Left.Accept(this, stack))
                .Pop(out var right)
                .Pop(out var left)
                .Push($"({left} + {right})");

        public ImmutableStack<string> Visit(MoneyProduct product, ImmutableStack<string> stack)
            => product
                .Expression
                .Accept(this, stack)
                .Pop(out var expression)
                .Push($"({product.Factor} * {expression})");

        public ImmutableStack<string> Visit(MoneyDistributionPart part, ImmutableStack<string> stack)
            => part.Distribution
                .Expression
                .Accept(this, stack)
                .Pop(out var total)
                .Push($"{total}.Distribute({FormatFactors(part.Distribution.Factors)})[{part.Index}]");

        private string FormatFactors(IEnumerable<int> factors)
            => string.Join(", ", factors);
    }
}
